using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using TMPro;

public class WifiButtonReceiver : MonoBehaviour
{
    [Header("ESP Settings")]
    public string espIp = "192.168.0.12";
    public int port = 4210;

    [Header("Targets")]
    public ThreeCharacterManager charManager;   // 需有 SetExclusive(int) 或 SetPressedXxx

    [Header("UI (Optional)")]
    public TMP_Text statusText;

    private TcpClient client;
    private StreamReader reader;
    private Thread listenThread;
    private volatile bool running = false;

    private readonly ConcurrentQueue<string> lineQueue = new ConcurrentQueue<string>();
    private float nextReconnectTime = 0f;
    private const float ReconnectInterval = 2f;

    void Start()
    {
        TryConnect();
        // 開場強制全 idle
        charManager?.SetExclusive(0);
    }

    void TryConnect()
    {
        try {
            client = new TcpClient { NoDelay = true };
            client.Connect(espIp, port);
            reader = new StreamReader(client.GetStream());
            running = true;
            listenThread = new Thread(ListenLoop) { IsBackground = true };
            listenThread.Start();
            Debug.Log($"Connected to ESP: {espIp}:{port}");
        } catch (Exception e) {
            Debug.LogWarning($"ConnectToESP failed: {e.Message}");
            ScheduleReconnect();
        }
    }

    void ScheduleReconnect() {
        running = false;
        nextReconnectTime = Time.time + ReconnectInterval;
    }

    void ListenLoop()
    {
        try {
            while (running && client != null && client.Connected)
            {
                var line = reader.ReadLine(); // ESP 每行以 \n 結尾
                if (!string.IsNullOrEmpty(line))
                    lineQueue.Enqueue(line);
            }
        } catch (Exception e) {
            Debug.LogWarning($"ListenLoop stopped: {e.Message}");
        } finally { running = false; }
    }

    void Update()
    {
        // 斷線重連
        if ((!running || client == null || !client.Connected) && Time.time >= nextReconnectTime)
        {
            Cleanup();
            TryConnect();
        }

        // 只處理最新一行
        string last = null;
        while (lineQueue.TryDequeue(out var l)) last = l;

        if (!string.IsNullOrEmpty(last))
        {
            HandleEventLine(last);
            if (statusText) statusText.text = last;
        }
    }

    void HandleEventLine(string line)
    {
        // 例：EV=B1_DOWN / EV=B1_UP / EV=B2_DOWN / EV=B2_UP / EV=B3_DOWN / EV=B3_UP
        // 也容忍前後有其它欄位，用逗號分割找 EV=...
        string ev = null;
        var parts = line.Split(',');
        foreach (var p in parts)
        {
            var kv = p.Split('=');
            if (kv.Length == 2 && kv[0].Trim() == "EV")
            {
                ev = kv[1].Trim();
                break;
            }
        }
        if (string.IsNullOrEmpty(ev)) return;

        // 對應：B1=Left, B2=Center, B3=Right
        switch (ev)
        {
            case "B1_DOWN": charManager?.SetExternalExclusive(1); break;
            case "B1_UP":   charManager?.SetExternalExclusive(0); break;

            case "B2_DOWN": charManager?.SetExternalExclusive(2); break;
            case "B2_UP":   charManager?.SetExternalExclusive(0); break;

            case "B3_DOWN": charManager?.SetExternalExclusive(3); break;
            case "B3_UP":   charManager?.SetExternalExclusive(0); break;

            // 可選：INIT 一律 idle
            case "INIT":
            case "B0":      charManager?.SetExternalExclusive(0); break;
        }
        // Debug.Log($"EV <- {ev}");
    }

    void OnDisable() => Cleanup();
    void OnApplicationQuit() => Cleanup();

    void Cleanup()
    {
        try { running = false; } catch { }
        try { listenThread?.Abort(); } catch { }
        try { reader?.Close(); } catch { }
        try { client?.Close(); } catch { }
        listenThread = null;
        reader = null;
        client = null;
    }
}

using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using TMPro;

public class SerialPosReceiver : MonoBehaviour
{
    [Header("Serial Settings")]
    public string portName = "COM4";
    public int baudRate = 115200;

    [Header("Targets")]
    public ThreeCharacterManager charManager;   
    public IntroManager introManager;           
    public RhythmJudge judge;                   

    [Header("UI (Optional)")]
    public TMP_Text statusText; // 如果你不想在 UI 上看到字串跳动，Inspector 里这个栏位可以留空

    private SerialPort port;
    private Thread listenThread;
    private volatile bool running = false;
    private readonly ConcurrentQueue<string> lineQueue = new ConcurrentQueue<string>();

    void Start()
    {
        TryOpenPort();
        if (charManager != null) charManager.SetExclusive(0);
    }

    void TryOpenPort()
    {
        try
        {
            port = new SerialPort(portName, baudRate);
            port.ReadTimeout = 100; 
            port.NewLine = "\n";
            port.Open();

            running = true;
            listenThread = new Thread(ListenLoop) { IsBackground = true };
            listenThread.Start();
            Debug.Log($"Serial opened: {portName}");
        }
        catch (Exception e) { Debug.LogError($"Serial Error: {e.Message}"); }
    }

    void ListenLoop()
    {
        while (running && port != null && port.IsOpen)
        {
            try
            {
                string line = port.ReadLine();
                if (!string.IsNullOrWhiteSpace(line)) lineQueue.Enqueue(line.Trim());
            }
            catch { }
        }
    }

    void Update()
    {
        string last = null;
        while (lineQueue.TryDequeue(out var l)) last = l;

        if (!string.IsNullOrEmpty(last))
        {
            // ★ 这里原本会显示 Text，如果你不想一直看到 POS:xxx，可以把下面这行注解掉
            // if (statusText != null) statusText.text = last; 
            
            HandleLine(last);
        }

        // 键盘模拟 (保留给你测试用)
        if (Input.GetKeyDown(KeyCode.Alpha1)) OnFsPosLogic(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) OnFsPosLogic(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) OnFsPosLogic(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) OnFsPosLogic(4); 
    }

    public void OnFsPosLogic(int pos)
    {
        // Debug.Log($"[ACTION] 触发位置: {pos}"); // ★ 注解掉，就不洗频了

        if (charManager != null) charManager.SetExternalExclusive(pos);

        if (introManager != null) introManager.OnFsPos(pos); 
        
        if (judge != null)
        {
            if (pos == 1) judge.OnPadHit(1);
            if (pos == 2) judge.OnPadHit(2);
            if (pos == 3) judge.OnPadHit(3);
            if (pos == 4) { judge.OnPadHit(1); judge.OnPadHit(3); } 
            if (pos == 5) { judge.OnPadHit(1); judge.OnPadHit(2); }
            if (pos == 6) { judge.OnPadHit(2); judge.OnPadHit(3); }
        }
    }

    void HandleLine(string line)
    {
        // ★ 這裡原本有 Debug.Log("RECV RAW: " + line); 已經移除了

        int pos = -1;
        int idx = -1;

        if (line.Contains("POS:")) idx = line.IndexOf("POS:") + 4;
        else if (line.Contains("Position")) idx = line.IndexOf("Position") + 8;
        
        if (idx == -1) return;

        while (idx < line.Length && !char.IsDigit(line[idx])) idx++;
        if (idx >= line.Length) return;
        
        string numStr = line.Substring(idx, 1); 
        if (int.TryParse(numStr, out pos))
        {
            OnFsPosLogic(pos);
        }
    }
    
    void OnDisable() { running = false; if(port != null && port.IsOpen) port.Close(); }
    void OnApplicationQuit() { running = false; if(port != null && port.IsOpen) port.Close(); }
}
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ESP8266 : MonoBehaviour
{
    public int listenPort = 7777;
    public float accX, accY, accZ;
    public Transform target;   // 要跟著轉的物件（例如一個 Cube）

    public float roll;
    public float pitch;
    public float yaw;

    UdpClient client;
    Thread recvThread;
    volatile bool running;

    void Start()
    {
        client = new UdpClient(listenPort);
        running = true;
        recvThread = new Thread(ReceiveLoop);
        recvThread.IsBackground = true;
        recvThread.Start();
    }

    void ReceiveLoop()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        while (running)
        {
            try
            {
                byte[] data = client.Receive(ref remoteEP);
                string json = Encoding.UTF8.GetString(data);

                float x = ExtractFloat(json, "\"accX\":");
                float y = ExtractFloat(json, "\"accY\":");
                float z = ExtractFloat(json, "\"accZ\":");

                accX = x;
                accY = y;
                accZ = z;
            }
            catch { }
        }
    }

    static float ExtractFloat(string s, string key)
    {
        int idx = s.IndexOf(key, StringComparison.Ordinal);
        if (idx < 0) return 0f;
        idx += key.Length;

        int i = idx;
        while (i < s.Length && ("-+.eE0123456789".IndexOf(s[i]) >= 0)) i++;
        if (float.TryParse(s.Substring(idx, i - idx), out float v))
            return v;
        return 0f;
    }

    void Update()
    {
        if (target != null)
        {
            // 你可以依照實際裝板方式調整軸，這裡示範簡單 ZYX
            target.rotation = Quaternion.Euler(pitch, yaw, roll);
            Debug.Log($"accX={accX:F2}, accY={accY:F2}, accZ={accZ:F2}");
        }
    }

    void OnApplicationQuit()
    {
        running = false;
        try { client?.Close(); } catch { }
    }
}

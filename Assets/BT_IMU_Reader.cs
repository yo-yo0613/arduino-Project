using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDP_ADXL_Receiver : MonoBehaviour
{
    [Header("Network")]
    public int listenPort = 4210;
    public bool logRaw = true;      // 勾起來就會印原始封包

    UdpClient _udp;
    Thread _th;
    volatile bool _run;

    // 解析後數值（照你ESP送 ax,ay,az）
    public Vector3 accel_g;

    void Start()
    {
        // 綁 0.0.0.0 表示任何網卡都收得到
        _udp = new UdpClient(new IPEndPoint(IPAddress.Any, listenPort));
        _run = true;

        _th = new Thread(RecvLoop) { IsBackground = true };
        _th.Start();

        Debug.Log($"UDP listening on {listenPort}");
    }

    void RecvLoop()
    {
        var remote = new IPEndPoint(IPAddress.Any, 0);
        while (_run)
        {
            try
            {
                byte[] data = _udp.Receive(ref remote);
                string msg = Encoding.ASCII.GetString(data);

                if (logRaw)
                    Debug.Log($"[UDP] {remote.Address}:{remote.Port} -> {msg}");

                // 預期格式: "ax,ay,az"
                var tok = msg.Split(',');
                if (tok.Length >= 3 &&
                    float.TryParse(tok[0], out float ax) &&
                    float.TryParse(tok[1], out float ay) &&
                    float.TryParse(tok[2], out float az))
                {
                    accel_g = new Vector3(ax, ay, az);
                    // 也可在這裡再 Debug.Log 解析後數值
                    // Debug.Log($"accel_g = {accel_g}");
                }
            }
            catch { /* 忽略暫時性錯誤 */ }
        }
    }

    void OnApplicationQuit()
    {
        _run = false;
        _th?.Join(200);
        _udp?.Close();
    }
}

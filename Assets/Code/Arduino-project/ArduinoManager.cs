using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class ArduinoManager : MonoBehaviour
{
    // 公開實體，讓其他腳本（如 CakeMakingManager）可以隨時抓取
    public static ArduinoManager Instance { get; private set; }

    [Header("測試設定")]
    public bool useSimulation = false;
    public string portName = "COM4";
    public int baudRate = 115200;

    private SerialPort _serialPort;
    private Thread _readThread;
    private bool _isRunning = false;

    public int CurrentPressure { get; private set; }
    public Vector2Int Joystick { get; private set; }

    void Awake()
    {
        // 修正：確保全遊戲只有一個實體存在
        if (Instance != null && Instance != this) {
            Destroy(gameObject); // 如果場景中已經有一個，就刪除現在這一個
            return;
        }

        Instance = this;
        // 關鍵：讓此物件跨場景生存，不被銷毀
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (useSimulation) 
        {
            Debug.Log("模擬模式已啟動");
            return; 
        }

        // 初始化 Port
        _serialPort = new SerialPort(portName, baudRate);
        _serialPort.ReadTimeout = 10;

        try
        {
            _serialPort.Open();
            _isRunning = true;
            _readThread = new Thread(ReadSerialData);
            _readThread.IsBackground = true;
            _readThread.Start();
            Debug.Log("Serial Port 已成功開啟");
        }
        catch (System.Exception e)
        {
            Debug.LogError("無法開啟 Serial Port: " + e.Message); //
        }
    }

    // 數據解析部分保持不變
    void ReadSerialData()
    {
        while (_isRunning && _serialPort != null && _serialPort.IsOpen)
        {
            try {
                string data = _serialPort.ReadLine();
                string[] parts = data.Split(',');
                if (parts.Length == 3) {
                    if (int.TryParse(parts[0], out int fsr)) CurrentPressure = fsr;
                    if (int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
                        Joystick = new Vector2Int(x, y);
                }
            } catch { }
            Thread.Sleep(1);
        }
    }

    void OnApplicationQuit()
    {
        _isRunning = false;
        if (_readThread != null) _readThread.Abort();
        if (_serialPort != null && _serialPort.IsOpen) _serialPort.Close();
    }
}
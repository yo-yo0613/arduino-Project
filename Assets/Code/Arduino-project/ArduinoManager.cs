using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class ArduinoManager : MonoBehaviour
{
    [Header("測試設定")]
    public bool useSimulation = false; // 在 Inspector 打勾即可進入模擬模式
    public string portName = "COM4";
    public int baudRate = 115200;

    private SerialPort _serialPort;
    private Thread _readThread;
    private bool _isRunning = false;

    public int CurrentPressure { get; private set; }

    void Start()
    {
        // 如果是模擬模式，直接跳過 Serial Port 初始化
        if (useSimulation) 
        {
            Debug.Log("目前處於模擬模式：請按住鍵盤 [W] 增加壓力，[S] 減少壓力");
            return; 
        }

        _serialPort = new SerialPort(portName, baudRate);
        _serialPort.ReadTimeout = 10;

        try
        {
            _serialPort.Open();
            _isRunning = true;
            _readThread = new Thread(ReadSerialData);
            _readThread.IsBackground = true; // 優化：確保程式關閉時執行緒會自動結束
            _readThread.Start();
            Debug.Log("Serial Port 已開啟");
        }
        catch (System.Exception e)
        {
            Debug.LogError("無法開啟 Serial Port: " + e.Message);
        }
    }

    void Update()
    {
        // 模擬模式下的鍵盤操作
        if (useSimulation)
        {
            if (Input.GetKey(KeyCode.W)) 
                CurrentPressure = Mathf.Min(CurrentPressure + 10, 1023); // 模擬增加壓力
            else if (Input.GetKey(KeyCode.S)) 
                CurrentPressure = Mathf.Max(CurrentPressure - 10, 0);   // 模擬減少壓力
            else
                CurrentPressure = Mathf.Max(CurrentPressure - 5, 0);    // 放開鍵盤壓力自動回降
        }
    }
    
    void ReadSerialData()
    {
        while (_isRunning && _serialPort != null && _serialPort.IsOpen)
        {
            try
            {
                string data = _serialPort.ReadLine();
                if (int.TryParse(data, out int value))
                {
                    CurrentPressure = value;
                }
            }
            catch (System.TimeoutException) { }
            catch (System.Exception) { }
            
            Thread.Sleep(1); // 優化：防止此執行緒吃光 CPU 資源
        }
    }

    void OnApplicationQuit()
    {
        _isRunning = false;
        if (_readThread != null) _readThread.Abort(); // 強制結束執行緒
        if (_serialPort != null && _serialPort.IsOpen) _serialPort.Close();
    }
}
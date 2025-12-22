using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChartRecorder : MonoBehaviour
{
    [Header("把 MusicPlayer 的 AudioSource 拖進來")]
    public AudioSource audioSource;

    [Header("三軌按鍵")]
    public KeyCode lane0Key = KeyCode.A;
    public KeyCode lane1Key = KeyCode.S;
    public KeyCode lane2Key = KeyCode.D;

    [Header("控制鍵")]
    public KeyCode startRecordKey = KeyCode.R; // 開始播放+錄譜
    public KeyCode stopAndSaveKey = KeyCode.F; // 停止+存檔

    // 是否正在錄譜
    private bool isRecording = false;

    // 歌曲開始播放的DSP時間（用來算相對時間，會更準）
    private double songStartDspTime = 0;

    // 記錄下來的音符清單
    private List<NoteEvent> notes = new List<NoteEvent>();

    void Update()
    {
        // 按R：開始錄譜
        if (Input.GetKeyDown(startRecordKey))
        {
            StartRecording();
        }

        // 按S：停止並存檔
        if (Input.GetKeyDown(stopAndSaveKey))
        {
            StopAndSave();
        }

        // 沒在錄譜就不做下面的記錄
        if (!isRecording) return;

        // 任何一個軌道鍵被按下，就記錄一筆
        if (Input.GetKeyDown(lane0Key)) AddNote(0);
        if (Input.GetKeyDown(lane1Key)) AddNote(1);
        if (Input.GetKeyDown(lane2Key)) AddNote(2);
    }

    // 開始錄譜：清空資料、排程播放、進入錄譜狀態
    void StartRecording()
    {
        // 防呆：沒拖AudioSource或沒指定音樂就直接報錯
        if (audioSource == null || audioSource.clip == null)
        {
            Debug.LogError("沒有設定 AudioSource 或 AudioClip！請把 MusicPlayer 的 AudioSource 拖進來，並指定音樂。");
            return;
        }

        notes.Clear();       // 清掉上次錄的
        isRecording = true;  // 進入錄譜狀態

        // 取得目前音訊系統時間
        double nowDsp = AudioSettings.dspTime;

        // 讓音樂延遲0.1秒再開始（比較穩，避免你按R瞬間產生誤差）
        songStartDspTime = nowDsp + 0.1;

        // 用更準確的方式在指定時間開始播放
        audioSource.Stop();
        audioSource.PlayScheduled(songStartDspTime);

        Debug.Log("開始錄譜！按 A/S/D 記錄。按 S 停止並存檔。");
    }

    // 記錄一個音符事件
    void AddNote(int lane)
    {
        
        double tSec = AudioSettings.dspTime - songStartDspTime;

    // 防呆：如果在歌還沒開始就按到，避免變負的
    if (tSec < 0) tSec = 0;

    // 轉成毫秒，並四捨五入（這樣雙押更容易變成同一時間）
    int tMs = (int)Math.Round(tSec * 1000.0);

    notes.Add(new NoteEvent { timeMs = tMs, lane = lane });

    Debug.Log($"記錄：lane={lane}, time={tMs}ms");

    }

    // 停止錄譜並存成JSON檔
    void StopAndSave()
    {
        if (!isRecording)
        {
            Debug.Log("目前沒有在錄譜。");
            return;
        }

        isRecording = false;
        audioSource.Stop();

        // 包一層，JsonUtility 才能序列化 List
        ChartData chart = new ChartData { notes = notes };

        // 轉成漂亮格式的JSON
        string json = JsonUtility.ToJson(chart, true);

        // 存檔路徑（這個路徑在打包後也能用）
        string fileName = $"chart_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string path = Path.Combine(Application.persistentDataPath, fileName);

        File.WriteAllText(path, json);

        Debug.Log($"✅ 存檔完成：{path}");
        Debug.Log("你可以去那個路徑把json拿出來，之後播放時讀取它就能生成譜面。");
    }

    // ===== 資料結構 =====

    [Serializable]
    public class NoteEvent
    {
        public float timeMs; // 秒
        public int lane;   // 軌道
    }

    [Serializable]
    public class ChartData
    {
        public List<NoteEvent> notes;
    }
}


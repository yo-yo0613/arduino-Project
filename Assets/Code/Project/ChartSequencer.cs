using System.Collections.Generic;
using UnityEngine;
using System.Linq; 
using System.Collections; // 需要這個來用 Coroutine

public class ChartSequencer : MonoBehaviour
{
    [Header("檔案與設定")]
    // ★ 修改 1：改成陣列，讓你可以在 Inspector 拖入多個譜面
    public TextAsset[] jsonFiles;   
    
    [Header("連結其他元件")]
    public AudioSource musicSource;        
    public ZombieLaneManager3x3 laneManager; 

    [Header("重要參數")]
    public float noteFallTime = 2.0f; 
    public float startDelay = 2.0f;

    // 內部變數
    private List<NoteEvent> allNotes = new List<NoteEvent>();
    private int nextIndex = 0;
    private bool isPlaying = false;
    private double musicStartDspTime; 
    private bool gameFinished = false; // 防止重複結算

    void Start()
    {
        // ★ 修改 2：隨機抽選一個譜面
        if (jsonFiles != null && jsonFiles.Length > 0)
        {
            int randomIndex = Random.Range(0, jsonFiles.Length);
            Debug.Log($"隨機選中了第 {randomIndex + 1} 個譜面");
            
            LoadChart(jsonFiles[randomIndex].text);
            StartGame();
        }
        else
        {
            Debug.LogError("請在 Inspector 的 Json Files 陣列中放入至少一個 JSON 檔案！");
        }
    }

    void LoadChart(string jsonContent)
    {
        ChartData data = JsonUtility.FromJson<ChartData>(jsonContent);
        allNotes = data.notes.OrderBy(n => n.timeMs).ToList();
        Debug.Log($"譜面讀取成功！共有 {allNotes.Count} 個音符。");
    }

    public void StartGame()
    {
        if (allNotes.Count == 0) return;

        nextIndex = 0;
        isPlaying = true;
        gameFinished = false;

        musicStartDspTime = AudioSettings.dspTime + startDelay;
        musicSource.Stop();
        musicSource.PlayScheduled(musicStartDspTime);

        Debug.Log("遊戲開始！");
    }

    void Update()
    {
        if (!isPlaying || gameFinished) return;

        double currentSongTime = AudioSettings.dspTime - musicStartDspTime;

        // 生成殭屍邏輯 (保持不變)
        while (nextIndex < allNotes.Count)
        {
            NoteEvent nextNote = allNotes[nextIndex];
            double spawnTime = (nextNote.timeMs / 1000.0) - noteFallTime;

            if (currentSongTime >= spawnTime)
            {
                SpawnZombie(nextNote.lane);
                nextIndex++; 
            }
            else
            {
                break; 
            }
        }

        // ★ 修改 3：偵測遊戲結束並上傳分數
        // 條件：所有音符都生成了 + 音樂播完了 + 緩衝 3 秒讓最後一隻殭屍跑完
        if (nextIndex >= allNotes.Count && !musicSource.isPlaying && currentSongTime > (allNotes.Last().timeMs / 1000.0) + 3.0f)
        {
            EndGameAndUpload();
        }
    }

    void SpawnZombie(int jsonLane)
    {
        int targetLane = jsonLane + 1; 
        if (laneManager != null) laneManager.SpawnZombie(targetLane);
    }

    // ★ 新增：遊戲結束處理
    void EndGameAndUpload()
    {
        gameFinished = true;
        isPlaying = false;
        Debug.Log("音樂結束，開始結算分數...");

        // 1. 計算總分 (從 GlobalGameManager 抓數據)
        // 假設：Perfect 100, Great 50, Good 20 (請依照你的 RhythmJudge 設定調整)
        int totalScore = (GlobalGameManager.perfectCount * 100) + 
                         (GlobalGameManager.greatCount * 50) + 
                         (GlobalGameManager.goodCount * 20);

        Debug.Log($"結算總分: {totalScore} (Player: {GlobalGameManager.playerName})");

        // 2. 呼叫 GoogleSheet 上傳
        if (GoogleSheetDataHandler.Instance != null)
        {
            GoogleSheetDataHandler.Instance.UploadScore(totalScore);
        }
        else
        {
            Debug.LogError("找不到 GoogleSheetDataHandler，無法上傳分數！");
        }
    }
}
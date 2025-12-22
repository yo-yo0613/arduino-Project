using System.Collections.Generic;
using UnityEngine;
using System.Linq; // 用來排序

public class ChartSequencer : MonoBehaviour
{
    [Header("檔案與設定")]
    public TextAsset jsonFile;   // ★ 把你錄好的 .json 檔拖進來
    
    [Header("連結其他元件")]
    public AudioSource musicSource;        // 播音樂的喇叭
    public ZombieLaneManager3x3 laneManager; // 控制殭屍的管理器

    [Header("重要參數")]
    [Tooltip("殭屍從生成走到判定點需要幾秒？必須跟 ZombieRunner 的 RunDuration 一樣")]
    public float noteFallTime = 2.0f; 

    [Tooltip("音樂延遲幾秒開始？(給第一隻殭屍一點預備時間)")]
    public float startDelay = 2.0f;

    // 內部變數
    private List<NoteEvent> allNotes = new List<NoteEvent>();
    private int nextIndex = 0;
    private bool isPlaying = false;
    private double musicStartDspTime; // 精準的音樂開始時間

    void Start()
    {
        // 1. 讀取並解析 JSON
        if (jsonFile != null)
        {
            LoadChart(jsonFile.text);
            StartGame();
        }
        else
        {
            Debug.LogError("請在 Inspector 放入 JSON 譜面檔案！");
        }
    }

    void LoadChart(string jsonContent)
    {
        // 解析 JSON
        ChartData data = JsonUtility.FromJson<ChartData>(jsonContent);
        
        // 排序 (確保時間是從小到大，雖然錄的時候通常也是這樣)
        allNotes = data.notes.OrderBy(n => n.timeMs).ToList();

        Debug.Log($"譜面讀取成功！共有 {allNotes.Count} 個音符。等待播放...");
    }

    // ★ 呼叫這個函式來開始遊戲
    public void StartGame()
    {
        if (allNotes.Count == 0) return;

        nextIndex = 0;
        isPlaying = true;

        // 設定音樂開始的精準時間 (現在時間 + 延遲時間)
        musicStartDspTime = AudioSettings.dspTime + startDelay;

        // 排程播放音樂
        musicSource.Stop();
        musicSource.PlayScheduled(musicStartDspTime);

        Debug.Log("遊戲開始！音樂將在 " + startDelay + " 秒後播放...");
    }

    void Update()
    {
        if (!isPlaying) return;

        // 1. 算出「現在音樂播到哪裡了」 (單位：秒)
        // 注意：在 startDelay 期間，這個數值會是負的，這正是我們要的！
        // 因為殭屍要在音樂響起 "之前" (負數時間) 就先生成，才能準時到達。
        double currentSongTime = AudioSettings.dspTime - musicStartDspTime;

        // 2. 檢查佇列，看看有沒有殭屍該出門了
        while (nextIndex < allNotes.Count)
        {
            NoteEvent nextNote = allNotes[nextIndex];

            // ★ 核心公式：生成時間 = (打擊時間 / 1000) - 跑步時間
            // 例如：打擊時間是 2.2秒，跑步要 2秒，那 0.2秒 時就要生成
            double spawnTime = (nextNote.timeMs / 1000.0) - noteFallTime;

            // 如果現在時間已經超過了生成時間，就生怪！
            if (currentSongTime >= spawnTime)
            {
                SpawnZombie(nextNote.lane);
                nextIndex++; // 檢查下一個
            }
            else
            {
                // 因為排過序了，如果這一隻時間還沒到，後面的一定也沒到
                break; 
            }
        }
    }

    void SpawnZombie(int jsonLane)
    {
        // JSON 紀錄的是 0, 1, 2
        // 但你的 LaneManager 可能是用 1, 2, 3
        int targetLane = jsonLane + 1; 

        if (laneManager != null)
        {
            laneManager.SpawnZombie(targetLane);
        }
    }
}
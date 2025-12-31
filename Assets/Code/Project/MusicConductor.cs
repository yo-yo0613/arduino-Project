using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MusicConductor : MonoBehaviour
{
    [Header("音樂來源")]
    public AudioSource music;

    [Header("節奏設定")]
    public float bpm = 120f;            
    public float firstBeatOffsetSec = 0f; 
    public int spawnIntervalBeats = 1;   // 每隔幾拍觸發一次隨機出怪 (一條橫排)

    [Header("出怪管理器 (請連結 ZombieLaneManager3x3)")]
    public ZombieLaneManager3x3 laneManager; 

    [Header("隨機出怪設定")]
    [Range(1, 3)]
    public int minLanesPerSpawn = 1; // 每次觸發，最少選擇幾條 Lane 出怪
    [Range(1, 3)]
    public int maxLanesPerSpawn = 2; // 每次觸發，最多選擇幾條 Lane 出怪
    
    // 內部追蹤變數
    [HideInInspector] public double songStartDspTime;
    [HideInInspector] public float secPerBeat;
    
    private double nextSpawnBeat = 0; // 下一個隨機出怪的節拍點
    private int[] allLanes = { 1, 2, 3 };

    public System.Action OnBeat { get; internal set; }

    void Start()
    {
        if (music == null)
        {
            Debug.LogError("MusicConductor: 沒指定 AudioSource");
            enabled = false;
            return;
        }
        if (laneManager == null)
        {
            Debug.LogError("MusicConductor: 沒指定 ZombieLaneManager3x3，請在 Inspector 連結!");
            enabled = false;
            return;
        }

        secPerBeat = 60f / bpm;

        songStartDspTime = AudioSettings.dspTime + firstBeatOffsetSec;
        music.PlayScheduled(songStartDspTime);

        // 確保第一個出怪點從第 0 拍開始
        nextSpawnBeat = 0; 
    }

    void Update()
    {
        if (music == null || !music.isPlaying) return;
        
        double songPosBeats = GetSongPosBeats();

        // 檢查是否達到下一個出怪節拍點
        // 使用 while 循環處理可能在單一幀內略過多個節拍點的情況
        while (songPosBeats >= nextSpawnBeat)
        {
            // 觸發隨機出怪邏輯
            TriggerRandomSpawn();

            // 設定下一個出怪節拍點
            nextSpawnBeat += spawnIntervalBeats;
        }
    }

    /// <summary>
    /// 執行隨機選擇 Lane 並呼叫管理器生成 (即一條橫排)
    /// </summary>
    void TriggerRandomSpawn()
    {
        // 1. 決定本拍要出現在幾條 Lane 上
        int count = Random.Range(minLanesPerSpawn, maxLanesPerSpawn + 1);
        
        // 2. 隨機選擇要出 Zombie 的 Lane (通道)
        List<int> lanesToChooseFrom = new List<int>(allLanes);
        
        // 隨機打亂所有 Lane 的順序，然後選前 'count' 條
        lanesToChooseFrom = lanesToChooseFrom.OrderBy(x => Random.value).ToList();
        List<int> selectedLanes = lanesToChooseFrom.Take(count).ToList();

        // 3. 在選定的 Lane 上請求生成 Zombie
        foreach (int lane in selectedLanes)
        {
            // 呼叫 LaneManager，它內部會執行「冷卻檢查」，
            // 如果該 Lane 正在冷卻，這次呼叫會被忽略。
            laneManager.SpawnZombie(lane);
        }
    }

    /// <summary>
    /// 給其它腳本取得目前的「第幾拍」
    /// </summary>
    public double GetSongPosBeats()
    {
        if (music == null) return 0.0;
        double songPosSec = AudioSettings.dspTime - songStartDspTime;
        return songPosSec / secPerBeat;
    }

    /// <summary>
    /// 重播這首歌
    /// </summary>
    public void RestartSong()
    {
        nextSpawnBeat = 0;

        if (music == null) return;

        music.Stop();
        songStartDspTime = AudioSettings.dspTime + firstBeatOffsetSec;
        music.PlayScheduled(songStartDspTime);
    }
}
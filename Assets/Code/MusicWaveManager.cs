using UnityEngine;
using System.Collections.Generic;
using System.Linq; // 用於 List.OrderBy

public class MusicWaveManager : MonoBehaviour
{
    [Header("核心管理器")]
    public ZombieLaneManager3x3 laneManager;
    
    [Header("音樂節奏系統 (必須提供，用於節拍同步)")]
    public MusicConductor conductor; 
    
    [Header("隨機生成設定")]
    [Range(1, 3)]
    public int minLanesPerBeat = 1; // 每拍最少選擇幾條 Lane
    [Range(1, 3)]
    public int maxLanesPerBeat = 2; // 每拍最多選擇幾條 Lane
    
    private int[] allLanes = { 1, 2, 3 };

    void Start()
    {
        if (laneManager == null)
        {
            Debug.LogError("請在 Inspector 中連結 ZombieLaneManager3x3!");
            return;
        }
        
        if (conductor == null)
        {
            Debug.LogError("請在 Inspector 中連結 MusicConductor!");
            return;
        }
        
        // ⭐ 訂閱 Conductor 的節拍事件
        conductor.OnBeat += NextBeat;
    }
    
    void OnDestroy()
    {
        // 離開時取消訂閱，避免錯誤
        if (conductor != null)
            conductor.OnBeat -= NextBeat;
    }

    /// <summary>
    /// 由 MusicConductor 呼叫：在指定的節拍點執行隨機生成。
    /// </summary>
    public void NextBeat()
    {
        // 1. 決定本拍要出現在幾條 Lane 上
        int count = Random.Range(minLanesPerBeat, maxLanesPerBeat + 1);
        
        // 2. 隨機選擇要出 Zombie 的 Lane (通道)
        // 使用 Fisher-Yates shuffle 算法來隨機選擇不重複的 Lane
        List<int> lanesToChooseFrom = new List<int>(allLanes);
        
        // 隨機打亂所有 Lane 的順序
        lanesToChooseFrom = lanesToChooseFrom.OrderBy(x => Random.value).ToList();

        // 選擇前 'count' 條 Lane
        List<int> selectedLanes = lanesToChooseFrom.Take(count).ToList();

        // 3. 在選定的 Lane 上生成 Zombie
        foreach (int lane in selectedLanes)
        {
            // ⭐ 這裡會呼叫 ZombieLaneManager3x3.SpawnZombie()
            // 該函式會自動執行您要求的「冷卻檢查」邏輯，確保前一隻跑完才能出下一隻。
            laneManager.SpawnZombie(lane);
        }
    }
}
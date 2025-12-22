using UnityEngine;

public class ZombieLaneManager3x3 : MonoBehaviour
{
    [Header("Lane 1 的三隻 Zombie (由前到後)")]
    public ZombieRunner[] lane1Zombies = new ZombieRunner[3];

    [Header("Lane 2 的三隻 Zombie (由前到後)")]
    public ZombieRunner[] lane2Zombies = new ZombieRunner[3];

    [Header("Lane 3 的三隻 Zombie (由前到後)")]
    public ZombieRunner[] lane3Zombies = new ZombieRunner[3];

    [Header("三條 Lane 的出生點 (Z 起點)")]
    public Transform lane1SpawnPoint;
    public Transform lane2SpawnPoint;
    public Transform lane3SpawnPoint;

    // ⭐ 移除 private float[] laneNextAllowedTime 陣列，不再進行時間冷卻

    [Header("同一條 Lane 冷卻比例 (此數值不再用於時間冷卻)")]
    [Range(0.1f, 1.0f)]
    public float laneCooldownRatio = 1.0f;   // 保持此變數但其功能被忽略

    void Start()
    {
        DisableAll();
    }

    void DisableAll()
    {
        foreach (var z in lane1Zombies) if (z != null) z.gameObject.SetActive(false);
        foreach (var z in lane2Zombies) if (z != null) z.gameObject.SetActive(false);
        foreach (var z in lane3Zombies) if (z != null) z.gameObject.SetActive(false);
    }

    /// <summary>
    /// 給 MusicConductor 呼叫：在指定的 lane 上出一隻 zombie。
    /// 現在只檢查該 Lane 是否有空閒的 Zombie。
    /// </summary>
    public void SpawnZombie(int lane)
    {
        int laneIndex = lane - 1;
        if (laneIndex < 0 || laneIndex >= 3) return;

        // 1) 找這條 Lane 裡「第一隻空閒的 Zombie」
        // 這是現在唯一的冷卻機制：只有 IsRunning = false 的 Zombie 才能被選中
        ZombieRunner[] list = GetLaneArray(laneIndex);
        Transform spawnPoint = GetSpawnPoint(laneIndex);

        if (list == null || spawnPoint == null) return;

        ZombieRunner chosen = null;
        foreach (var z in list)
        {
            if (z == null) continue; 
            
            if (!z.IsRunning) // ⭐ 關鍵：檢查它是否還在跑
            {
                chosen = z;
                break;
            }
        }

        if (chosen == null)
        {
            // 這條 Lane 的所有 3 隻 Zombie 都在跑 → 當拍略過
            return;
        }

        // 2) 真正出 Zombie
        chosen.SpawnAt(spawnPoint);

        // ⭐ 移除：不再設定 laneNextAllowedTime，改由 IsRunning 狀態控制
    }

    ZombieRunner[] GetLaneArray(int laneIndex)
    {
        switch (laneIndex)
        {
            case 0: return lane1Zombies;
            case 1: return lane2Zombies;
            case 2: return lane3Zombies;
        }
        return null;
    }

    Transform GetSpawnPoint(int laneIndex)
    {
        switch (laneIndex)
        {
            case 0: return lane1SpawnPoint;
            case 1: return lane2SpawnPoint;
            case 2: return lane3SpawnPoint;
        }
        return null;
    }
}
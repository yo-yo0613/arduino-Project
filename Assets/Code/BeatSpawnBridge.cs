using UnityEngine;

public class BeatSpawnBridge : MonoBehaviour
{
    [Header("來源元件")]
    public AudioProcessor processor;          // 拖 AudioProcessor 進來
    public ZombieLaneManager3x3 laneManager;  // 拖你現在的 ZombieWaveManager 進來

    [Header("節奏分頻")]
    [Tooltip("1 = 每一拍都出怪, 2 = 每兩拍出一隻, 4 = 每四拍出一隻…")]
    public float beatDivisor = 1;

    [Header("要生在哪條 Lane")]
    public bool randomLane = true;            // true = 隨機 1~3
    [Range(1, 3)]
    public int fixedLane = 2;                 // randomLane = false 時用這條

    private int beatCount = 0;

    void Awake()
    {
        // 如果沒手動指定，就自動找場景裡的 AudioProcessor
        if (processor == null)
            processor = FindObjectOfType<AudioProcessor>();

        if (processor == null)
        {
            Debug.LogError("[BeatSpawnBridge] 找不到 AudioProcessor，請在 Inspector 指定。");
            enabled = false;
            return;
        }

        if (laneManager == null)
        {
            Debug.LogError("[BeatSpawnBridge] 沒有指定 ZombieWaveManager。");
            enabled = false;
            return;
        }

        // 訂閱拍點事件（這是 Allan 專案裡 AudioProcessor 提供的）
        processor.onBeat.AddListener(OnBeat);
        // 如果你只想聽某一段頻帶，也可以用 onSpectrum，但先不用
    }

    void OnDestroy()
    {
        if (processor != null)
            processor.onBeat.RemoveListener(OnBeat);
    }

    private void OnBeat()
    {
        beatCount++;

        // 分頻：例如 beatDivisor=2 → 每兩拍才真的出怪一次
        if (beatDivisor > 1 && beatCount % beatDivisor != 0)
            return;

        int lane = randomLane ? Random.Range(1, 4) : fixedLane;

        //Debug.Log($"[BEAT] beat #{beatCount}, spawn lane {lane}");

        laneManager.SpawnZombie(lane);
    }
}

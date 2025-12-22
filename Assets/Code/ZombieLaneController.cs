using System.Collections;
using UnityEngine;

public class ZombieLaneController : MonoBehaviour
{
    [Header("三個 Lane 的定位點")]
    public Transform lane1Point;  // 左邊靶心
    public Transform lane2Point;  // 中間靶心
    public Transform lane3Point;  // 右邊靶心

    [Header("如果要微調水平位置，可以用這個")]
    public float xOffset = 0f;

    public int currentLane { get; private set; } = 0;  // 0 = 不在任何 Lane

    [Header("自動消失設定")]
    public bool autoHide = true;
    public float lifeTime = 2.0f;   // 僵屍出現多久後自動關掉（秒）

    private Coroutine hideRoutine;

    [Header("動畫")]
    public Animator animator;
    public float animSpeed = 1.5f;        // 預設加快 1.5 倍
    public string runStateName = "Run";   // 這隻殭屍跑步/向前走的動畫 state 名稱

    // ⭐ 給 WaveManager / 判定用：這隻殭屍現在是不是在跑
    public bool IsRunning { get; private set; } = false;

    void Start()
    {
        if (animator != null)
            animator.speed = animSpeed;
    }

    /// <summary>
    /// 把這隻僵屍移到指定 Lane (1/2/3)，只改 X。
    /// （原本的邏輯：決定 lane 上的靶心位置）
    /// </summary>
    public void SetLane(int lane)
    {
        lane = Mathf.Clamp(lane, 1, 3);
        currentLane = lane;

        Transform target = null;
        switch (lane)
        {
            case 1: target = lane1Point; break;
            case 2: target = lane2Point; break;
            case 3: target = lane3Point; break;
        }

        if (target != null)
        {
            // ⭐ 只改 X，Y/Z 保持原來（讓 Animator 的軌道不會亂）
            Vector3 pos = transform.position;
            pos.x = target.position.x + xOffset;
            transform.position = pos;
        }

        gameObject.SetActive(true);

        // 這邊不再處理 IsRunning，讓 TrySpawnOnLane 控制
        // 但如果你有別的地方直接呼叫 SetLane，也沒關係，還是會顯示出來
    }

    /// <summary>
    /// WaveManager 呼叫：如果這隻還在跑，就忽略；空閒才會真的出發。
    /// 回傳 true 表示這拍「有成功出發」，false 表示這拍被略過。
    /// </summary>
    public bool TrySpawnOnLane(int lane)
    {
        if (IsRunning)
        {
            // 這隻殭屍上一輪還沒跑完，這拍直接略過
            return false;
        }

        // 先決定 Lane（只改 X）
        SetLane(lane);

        // 播放跑步動畫
        if (animator != null && !string.IsNullOrEmpty(runStateName))
        {
            animator.Play(runStateName, 0, 0f);  // 從頭播放
        }

        // 標記為「正在跑」
        IsRunning = true;

        // 重啟自動關閉計時
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        if (autoHide)
            hideRoutine = StartCoroutine(AutoHideAfterLifeTime());

        return true;
    }

    public void Hide()
    {
        currentLane = 0;
        IsRunning   = false;
        gameObject.SetActive(false);
    }

    IEnumerator AutoHideAfterLifeTime()
    {
        yield return new WaitForSeconds(lifeTime);
        // 跑完時間到，就關掉 & 解除 IsRunning
        IsRunning = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// （選用）如果你想用 Animation Event 控制結束，
    /// 在跑步動畫最後一幀加 event 呼叫這個。
    /// </summary>
    public void OnRunAnimationEnd()
    {
        IsRunning = false;
        if (autoHide)
            gameObject.SetActive(false);
    }
}

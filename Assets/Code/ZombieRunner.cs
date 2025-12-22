using System.Collections;
using UnityEngine;

public class ZombieRunner : MonoBehaviour
{
    [Header("這隻 Zombie 的 Animator")]
    public Animator animator;
    public string runStateName = "Run"; 

    [Header("音效設定")]
    public AudioSource audioSource; 
    public AudioClip zombieRunSound; 

    [Header("參數")]
    public float runDuration = 2.0f;      
    public float animSpeed = 1.0f; // ★ 建議改回 1.0 慢慢調，太快很難抓節奏        

    public bool IsRunning { get; private set; } = false;
    private Coroutine finishRoutine; 

    void Start()
    {
        if (animator != null) animator.speed = animSpeed;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void SpawnAt(Transform spawnPoint)
    {
        // 雖然不靠距離判定，但設定位置還是必要的，確保視覺正確
        if (spawnPoint != null) transform.position = spawnPoint.position;

        gameObject.SetActive(true);

        if (animator != null && !string.IsNullOrEmpty(runStateName))
        {
            // 強制從頭播放
            animator.Play(runStateName, 0, 0f);
            animator.speed = animSpeed; // 確保速度正確
        }

        if (audioSource != null && zombieRunSound != null)
        {
            audioSource.PlayOneShot(zombieRunSound);
        }

        if (finishRoutine != null) StopCoroutine(finishRoutine);
        
        IsRunning = true;
        finishRoutine = StartCoroutine(AutoFinish());
    }

    IEnumerator AutoFinish()
    {
        yield return new WaitForSeconds(runDuration); 
        IsRunning = false;
        gameObject.SetActive(false);
    }

    public void OnRunAnimationEnd()
    {
        HideImmediately();
    }

    public void HideImmediately()
    {
        if (finishRoutine != null) StopCoroutine(finishRoutine);
        IsRunning = false;
        gameObject.SetActive(false);
    }

    // ★ 核心：回傳目前動畫進度 (0.0 = 剛開始, 1.0 = 結束)
    public float GetAnimationProgress()
    {
        if (animator == null) return -1f;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // Debug.Log($"目前播放的動畫 State 是: {animator.GetCurrentAnimatorClipInfo(0)[0].clip.name} / Hash對應: {stateInfo.shortNameHash}");
        // ★ 防呆：先抓出 Clip 資訊，但不要馬上用 [0] 去讀
        var clipInfo = animator.GetCurrentAnimatorClipInfo(0);

        // 如果現在沒有任何 Clip (陣列長度為 0)，就直接回傳 -1，不要繼續執行
        if (clipInfo == null || clipInfo.Length == 0)
        {
            return -1f;
        }
        // 只有在播 "Run" (或你指定的動畫) 時才算數
        if (stateInfo.IsName(runStateName))
        {
            // 如果你的動畫是 Loop 的，normalizedTime 會超過 1 (例如 1.5, 2.3)
            // 這裡我們取小數部分，確保它永遠在 0~1 之間 (如果你是非 Loop 動畫，直接回傳即可)
            return stateInfo.normalizedTime % 1.0f; 
        }
        return -1f;
    }
}
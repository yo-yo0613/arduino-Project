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
    public float animSpeed = 1.0f; 

    public bool IsRunning { get; private set; } = false;
    private Coroutine finishRoutine; 

    void Start()
    {
        if (animator != null) animator.speed = animSpeed;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void SpawnAt(Transform spawnPoint)
    {
        if (spawnPoint != null) transform.position = spawnPoint.position;

        gameObject.SetActive(true);

        if (animator != null && !string.IsNullOrEmpty(runStateName))
        {
            animator.Play(runStateName, 0, 0f);
            animator.speed = animSpeed; 
        }

        if (audioSource != null && zombieRunSound != null)
        {
            audioSource.PlayOneShot(zombieRunSound);
        }

        if (finishRoutine != null) StopCoroutine(finishRoutine);
        
        IsRunning = true;
        
        // ★★★ 修正 1：不要讓它自動結束！註解掉這行！ ★★★
        // finishRoutine = StartCoroutine(AutoFinish());
    }

    // ★★★ 修正 1 的相關函式：這個函式現在用不到了，但我留著以免報錯 ★★★
    IEnumerator AutoFinish()
    {
        yield return new WaitForSeconds(runDuration); 
        IsRunning = false;
        
        // ★★★ 兇手就是這行！把它註解掉！ ★★★
        // gameObject.SetActive(false); 
        
        // 建議：這裡只把 IsRunning 設為 false 就好，讓身體留著給法官判刑
        // 或者甚至連這個協程都不要跑
    }

    public void OnRunAnimationEnd()
    {
        // ★★★ 修正 2：動畫播完也不准消失！ ★★★
        // HideImmediately(); 
    }

    public void HideImmediately()
    {
        if (finishRoutine != null) StopCoroutine(finishRoutine);
        IsRunning = false;
        gameObject.SetActive(false);
    }

    public float GetAnimationProgress()
    {
        if (animator == null) return -1f;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        var clipInfo = animator.GetCurrentAnimatorClipInfo(0);

        if (clipInfo == null || clipInfo.Length == 0) return -1f;

        if (stateInfo.IsName(runStateName))
        {
            // ★★★ 修正 3：拿掉 % 1.0f ★★★
            // 如果動畫跑過頭變成 1.1，我們就要回傳 1.1，這樣法官才知道它超過了！
            // 不要讓它歸零！
            return stateInfo.normalizedTime; 
        }
        return -1f;
    }
}
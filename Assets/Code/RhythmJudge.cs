using System.Collections;
using UnityEngine;
using System.Linq; 

public class RhythmJudge : MonoBehaviour
{
    [Header("【時間判定設定】(0.0 ~ 1.0)")]
    public float targetTime = 0.90f; 

    [Header("【誤差容許值】")]
    public float perfectTol = 0.05f; 
    public float greatTol   = 0.15f; 
    public float goodTol    = 0.25f; 
    public float missTol    = 0.40f; 

    [Header("【分數設定】")]
    public int scorePerfect = 100;
    public int scoreGreat   = 50;
    public int scoreGood    = 20;

    [Header("Lane 管理器")]
    public ZombieLaneManager3x3 laneManager;

    [Header("★ 爆炸特效物件")]
    public GameObject explosionLeftObj;
    public GameObject explosionCenterObj;
    public GameObject explosionRightObj;
    public float explosionDuration = 0.5f;

    [Header("UI Feedback")]
    public GameObject perfectObj;
    public GameObject greatObj;
    public GameObject goodObj;
    public GameObject missObj;
    public GameObject badObj;
    public float feedbackDuration = 0.5f;

    [Header("Combo UI")]
    public GameObject comboEffectObj; 
    public float comboDuration = 1.5f;

    [Header("Hitted Robot 表情回饋")]
    public SpriteRenderer robotFaceRenderer; 
    public Sprite robotGoodSprite; 
    public Sprite robotBadSprite;  

    [Header("音效設定")]
    public AudioSource audioSource; 
    public AudioClip failureSound;  
    public AudioClip hitExplosionSound;

    private Coroutine feedbackRoutine;
    private Coroutine comboRoutine; 

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        DisableAllFeedback();
        
        if (comboEffectObj != null) comboEffectObj.SetActive(false);
        if (explosionLeftObj) explosionLeftObj.SetActive(false);
        if (explosionCenterObj) explosionCenterObj.SetActive(false);
        if (explosionRightObj) explosionRightObj.SetActive(false);
    }

    void Update()
    {
        // 玩家輸入判定
        if (Input.GetKeyDown(KeyCode.Alpha1)) OnPadHit(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) OnPadHit(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) OnPadHit(3);

        // ★★★ 新增：每一幀檢查是否有殭屍跑掉 (漏接) ★★★
        CheckMissedZombies();
    }

    // ★★★ 新增：檢查漏接邏輯 ★★★
    void CheckMissedZombies()
    {
        if (laneManager == null) return;

        // 檢查三條跑道
        CheckLaneForMiss(laneManager.lane1Zombies);
        CheckLaneForMiss(laneManager.lane2Zombies);
        CheckLaneForMiss(laneManager.lane3Zombies);
    }

    // ★★★ 修改後的檢查漏接邏輯 ★★★
    void CheckLaneForMiss(ZombieRunner[] zombies)
    {
        if (zombies == null) return;

        foreach (var z in zombies)
        {
            // 1. 必須檢查殭屍是否還開啟 (Active)
            if (z != null && z.gameObject.activeSelf && z.IsRunning)
            {
                float progress = z.GetAnimationProgress();

                // ★★★ Debug 用：如果你發現還是沒反應，請把這行取消註解，看看進度是多少 ★★★
                // Debug.Log($"Zombie Progress: {progress}");

                // 2. 關鍵修改：不要等到 1.0f，改用 0.98f 或 0.99f
                // 因為浮點數誤差，有時候它會卡在 0.99999，永遠不到 1.0
                // 而且我們要趕在它自動消失前抓到它！
                if (progress >= 0.99f) 
                {
                    //Debug.Log($"<color=red>抓到了！漏接殭屍！進度: {progress}</color>");

                    // 3. 觸發 Bad/Miss 懲罰
                    HandleBadHit(); 
                    
                    // 4. 處決殭屍 (讓它消失)
                    z.HideImmediately();
                }
            }
        }
    }

    public void OnPadHit(int lane)
    {
        ZombieRunner[] laneZombies = null;
        if (laneManager != null)
        {
            if (lane == 1) laneZombies = laneManager.lane1Zombies;
            else if (lane == 2) laneZombies = laneManager.lane2Zombies;
            else if (lane == 3) laneZombies = laneManager.lane3Zombies;
        }

        if (laneZombies == null) return;

        ZombieRunner bestZombie = null;
        float minDiff = float.MaxValue; 

        foreach (var z in laneZombies)
        {
            if (z == null || !z.gameObject.activeSelf || !z.IsRunning) continue;

            float progress = z.GetAnimationProgress();
            if (progress < 0) continue; 

            float diff = Mathf.Abs(progress - targetTime);

            if (diff < minDiff)
            {
                minDiff = diff;
                bestZombie = z;
            }
        }

        if (bestZombie == null || minDiff > missTol)
        {
            HandleBadHit();
            return;
        }

        bestZombie.HideImmediately();
        
        bool isGoodHit = true;
        GameObject feedbackObj = null;

        if (minDiff <= perfectTol)
        {
            feedbackObj = perfectObj; isGoodHit = true;
            GlobalGameManager.perfectCount++;  
            AddCombo();
            TriggerExplosion(lane);
        }
        else if (minDiff <= greatTol)
        {
            feedbackObj = greatObj; isGoodHit = true;
            GlobalGameManager.greatCount++; 
            AddCombo();
            TriggerExplosion(lane);
        }
        else if (minDiff <= goodTol)
        {
            feedbackObj = goodObj; isGoodHit = true;
            GlobalGameManager.goodCount++; 
            AddCombo();
            TriggerExplosion(lane);
        }
        else
        {
            feedbackObj = missObj; isGoodHit = false;
            GlobalGameManager.missCount++; 
            ResetCombo();
            Debug.Log($"<color=red>MISS</color>");
        }

        ShowFeedback(feedbackObj);
        UpdateRobotFace(isGoodHit);
    }

    void TriggerExplosion(int lane)
    {
        if (audioSource != null && hitExplosionSound != null)
        {
            audioSource.PlayOneShot(hitExplosionSound);
        }

        GameObject targetExplosion = null;
        switch(lane)
        {
            case 1: targetExplosion = explosionLeftObj; break;
            case 2: targetExplosion = explosionCenterObj; break;
            case 3: targetExplosion = explosionRightObj; break;
        }

        if (targetExplosion != null)
        {
            StartCoroutine(ShowExplosionRoutine(targetExplosion));
        }
    }

    IEnumerator ShowExplosionRoutine(GameObject explosion)
    {
        explosion.SetActive(false);
        yield return null; 
        explosion.SetActive(true);
        yield return new WaitForSeconds(explosionDuration);
        explosion.SetActive(false);
    }

    void HandleBadHit()
    {
        // 這裡會顯示 Bad，如果你想要顯示 Miss，可以把 badObj 改成 missObj
        ShowFeedback(badObj); 
        UpdateRobotFace(false);
        PlayFailureSound();
        ResetCombo();
    }

    void AddCombo()
    {
        GlobalGameManager.currentCombo++;

        if (GlobalGameManager.currentCombo > GlobalGameManager.maxCombo)
        {
            GlobalGameManager.maxCombo = GlobalGameManager.currentCombo;
        }

        UpdateComboUI();
    }

    void ResetCombo()
    {
        GlobalGameManager.currentCombo = 0;
        if (comboEffectObj != null) comboEffectObj.SetActive(false);
    }

    void UpdateComboUI()
    {
        if (comboEffectObj == null) return;

        if (GlobalGameManager.currentCombo >= 3)
        {
            if (comboRoutine != null) StopCoroutine(comboRoutine);
            comboRoutine = StartCoroutine(ReplayComboAnim());
        }
    }

    IEnumerator ReplayComboAnim()
    {
        if (comboEffectObj != null)
        {
            comboEffectObj.SetActive(false);
            yield return null; 
            comboEffectObj.SetActive(true);
            yield return new WaitForSeconds(comboDuration);
            comboEffectObj.SetActive(false);
        }
    }

    void PlayFailureSound()
    {
        if (audioSource != null && failureSound != null)
            audioSource.PlayOneShot(failureSound);
    }

    void UpdateRobotFace(bool isGood)
    {
        if (robotFaceRenderer == null) return;
        robotFaceRenderer.sprite = isGood ? robotGoodSprite : robotBadSprite;
    }

    private void DisableAllFeedback()
    {
        if (perfectObj) perfectObj.SetActive(false);
        if (greatObj)   greatObj.SetActive(false);
        if (goodObj)    goodObj.SetActive(false);
        if (missObj)    missObj.SetActive(false);
        if (badObj)     badObj.SetActive(false);
    }

    private void ShowFeedback(GameObject obj)
    {
        if (obj == null) return;
        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(ShowFeedbackRoutine(obj));
    }

    private IEnumerator ShowFeedbackRoutine(GameObject obj)
    {
        DisableAllFeedback();
        obj.SetActive(true);
        yield return new WaitForSeconds(feedbackDuration);
        obj.SetActive(false);
    }
}
using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class RhythmJudge : MonoBehaviour
{
    [Header("【時間判定設定】(0.0 ~ 1.0)")]
    public float targetTime = 0.90f; 

    [Header("角色管理器 (請拖入 Manager 物件)")]
    public ThreeCharacterManager charManager; // ★ 新增這個欄位

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

        // ★ 修改：傳入跑道編號 (1, 2, 3)
        CheckLaneForMiss(laneManager.lane1Zombies, 1);
        CheckLaneForMiss(laneManager.lane2Zombies, 2);
        CheckLaneForMiss(laneManager.lane3Zombies, 3);
    }

    // ★ 修改：增加 int laneId 參數
    void CheckLaneForMiss(ZombieRunner[] zombies, int laneId)
    {
        if (zombies == null) return;

        foreach (var z in zombies)
        {
            if (z != null && z.gameObject.activeSelf && z.IsRunning)
            {
                float progress = z.GetAnimationProgress();

                if (progress >= 0.99f) 
                {
                    // ★ 修改：傳入 laneId
                    HandleBadHit(laneId); 
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
            HandleBadHit(lane);
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

            // ★ 新增：按錯的時候也要觸發失敗動畫
            if (charManager != null) charManager.TriggerFailure(lane);
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

    void HandleBadHit(int lane = -1)
    {
        // 這裡會顯示 Bad，如果你想要顯示 Miss，可以把 badObj 改成 missObj
        ShowFeedback(missObj); 
        UpdateRobotFace(false);
        PlayFailureSound();
        ResetCombo();

        // ==========================================
        // ★★★ 漏掉的關鍵在這裡！加上這行才會真正紀錄到 Miss ★★★
        // ==========================================
        GlobalGameManager.missCount++;

        // ★ 新增：呼叫角色管理器播動畫
        if (lane != -1 && charManager != null)
        {
            charManager.TriggerFailure(lane);
        }
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
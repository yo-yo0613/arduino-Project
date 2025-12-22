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
        if (Input.GetKeyDown(KeyCode.Alpha1)) OnPadHit(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) OnPadHit(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) OnPadHit(3);
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
            // 處理 Combo (包含歸零邏輯)
            AddCombo();

            TriggerExplosion(lane);
            //Debug.Log($"<color=cyan>PERFECT!</color> +{scorePerfect}");
        }
        else if (minDiff <= greatTol)
        {
            feedbackObj = greatObj; isGoodHit = true;
            GlobalGameManager.greatCount++; 
            AddCombo();

            TriggerExplosion(lane);
            //Debug.Log($"<color=green>GREAT!</color> +{scoreGreat}");
        }
        else if (minDiff <= goodTol)
        {
            feedbackObj = goodObj; isGoodHit = true;
            GlobalGameManager.goodCount++; 
            AddCombo();

            TriggerExplosion(lane);
            //Debug.Log($"<color=yellow>GOOD</color> +{scoreGood}");
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
        ShowFeedback(badObj);
        UpdateRobotFace(false);
        PlayFailureSound();
        ResetCombo();
    }

    // ★ 修改處 1：加入歸零邏輯
    void AddCombo()
    {
        GlobalGameManager.currentCombo++;
        
        // 更新 UI (如果滿3次就會顯示)
        UpdateComboUI();

        // ★ 核心邏輯：如果達到 3 次，就歸零重新計算
        // 這樣下一次打中就會變回 Combo 1，不會一直觸發特效
        if (GlobalGameManager.currentCombo >= 3)
        {
            GlobalGameManager.currentCombo = 0;
        }
    }

    void ResetCombo()
    {
        GlobalGameManager.currentCombo = 0;
        if (comboEffectObj != null) comboEffectObj.SetActive(false);
    }

    // ★ 修改處 2：移除 else 區塊
    void UpdateComboUI()
    {
        if (comboEffectObj == null) return;

        if (GlobalGameManager.currentCombo >= 3)
        {
            if (comboRoutine != null) StopCoroutine(comboRoutine);
            comboRoutine = StartCoroutine(ReplayComboAnim());
        }
        // ★ 注意：這裡原本有 else { SetActive(false) } 被我拿掉了。
        // 這是為了防止「剛觸發 Combo 3 (歸零) -> 下一秒打出 Combo 1 -> 把 Combo 3 的特效切斷」的情況。
        // 現在特效只會靠下面的 ReplayComboAnim 時間到自動消失，或者 ResetCombo (Miss) 時強制消失。
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

    // ... (其餘部分保持不變) ...
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
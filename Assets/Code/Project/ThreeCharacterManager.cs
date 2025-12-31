using UnityEngine;

public class ThreeCharacterManager : MonoBehaviour
{
    [Header("音效設定")]
    public AudioSource audioSource;

    [Header("通用音效")]
    public AudioClip buttonSound;
    public AudioClip penguinSound;

    [Header("三個方位的專屬音效")]
    public AudioClip leftSound;
    public AudioClip centerSound;
    public AudioClip rightSound;

    [Header("B1/B2/B3 對應的角色 (按鈕)")]
    public CharacterButtonControlled leftPenguin;
    public CharacterButtonControlled centerPenguin;
    public CharacterButtonControlled rightPenguin;

    [Header("射擊特效 (拖入帶有圖片的 GameObject)")]
    public GameObject RightShootingObject;
    public GameObject CenterShootingObject;
    public GameObject LeftShootingObject;

    [Header("機器人 (拖入帶有圖片的 GameObject)")]
    public GameObject RightRobotObject;
    public GameObject CenterRobotObject;
    public GameObject LeftRobotObject;

    private int externalIndex = 0;
    private int lastIndex = -1;

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        UpdateState(0);
    }

    void Update()
    {
        // 1. 鍵盤輸入偵測
        bool k1 = Input.GetKey(KeyCode.Alpha1);
        bool k2 = Input.GetKey(KeyCode.Alpha2);
        bool k3 = Input.GetKey(KeyCode.Alpha3);

        int keyboardIndex = 0;

        if (k1 && k3) keyboardIndex = 4;
        else if (k1 && k2) keyboardIndex = 5;
        else if (k2 && k3) keyboardIndex = 6;
        else if (k1) keyboardIndex = 1;
        else if (k2) keyboardIndex = 2;
        else if (k3) keyboardIndex = 3;
        else keyboardIndex = 0; 

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            externalIndex = 0;
            UpdateState(0);
            return;
        }

        // 2. 決定最終訊號 (鍵盤優先，沒按鍵盤才看外部訊號)
        int finalIndex = (keyboardIndex != 0) ? keyboardIndex : externalIndex;

        // ★ 偵錯用：如果你放開按鍵但圖片不消失，請看 Console 視窗
        // 如果 Console 一直顯示 "Current Index: 1" 代表訊號卡住了
        // Debug.Log($"Current Index: {finalIndex}"); 

        if (finalIndex != lastIndex)
        {
            UpdateState(finalIndex);
        }
    }

    private void UpdateState(int index)
    {
        SetExclusive(index);
        lastIndex = index;
    }

    public void SetExternalExclusive(int index)
    {
        externalIndex = Mathf.Clamp(index, 0, 6);
        // 當外部訊號改變時，強制更新一次狀態，避免延遲
        UpdateState(externalIndex);
    }

    public void TriggerFailure(int lane)
    {
        if (lane == 1 && leftPenguin != null) leftPenguin.PlayFailure();
        else if (lane == 2 && centerPenguin != null) centerPenguin.PlayFailure();
        else if (lane == 3 && rightPenguin != null) rightPenguin.PlayFailure();
    }

    public void SetExclusive(int index)
    {
        // 定義哪個方位該開啟
        bool activeLeft = (index == 1 || index == 4 || index == 5);
        bool activeCenter = (index == 2 || index == 5 || index == 6);
        bool activeRight = (index == 3 || index == 4 || index == 6);

        if (leftPenguin) leftPenguin.SetPressed(activeLeft);
        if (centerPenguin) centerPenguin.SetPressed(activeCenter);
        if (rightPenguin) rightPenguin.SetPressed(activeRight);

        // 更新畫面 (傳入物件與是否開啟)
        UpdateVisuals(LeftShootingObject, LeftRobotObject, activeLeft, leftSound);
        UpdateVisuals(CenterShootingObject, CenterRobotObject, activeCenter, centerSound);
        UpdateVisuals(RightShootingObject, RightRobotObject, activeRight, rightSound);
    }

    // ★ 這是你要的：單純的顯示與隱藏
    private void UpdateVisuals(GameObject shootObj, GameObject robotObj, bool isActive, AudioClip directionSound)
    {
        bool isTurningOn = false;

        // --- 1. 處理水砲 ---
        if (shootObj != null)
        {
            if (isActive)
            {
                // 如果需要開啟，且目前是關的 -> 開啟
                if (!shootObj.activeSelf)
                {
                    shootObj.SetActive(true);
                    isTurningOn = true;
                }
            }
            else
            {
                // ★ 如果不需要開啟，強制關閉 (SetActive false)
                // 這行保證了「不按就不出現」
                if (shootObj.activeSelf)
                {
                    shootObj.SetActive(false);
                }
            }
        }

        // --- 2. 處理機器人 (邏輯同上) ---
        if (robotObj != null)
        {
            if (isActive)
            {
                if (!robotObj.activeSelf)
                {
                    robotObj.SetActive(true);
                    isTurningOn = true;
                }
            }
            else
            {
                // 沒按就關閉
                if (robotObj.activeSelf)
                {
                    robotObj.SetActive(false);
                }
            }
        }

        // --- 3. 播放音效 ---
        if (isTurningOn && audioSource != null)
        {
            if (directionSound != null) audioSource.PlayOneShot(directionSound);
            if (buttonSound != null) audioSource.PlayOneShot(buttonSound);
            if (penguinSound != null) audioSource.PlayOneShot(penguinSound);
        }
    }
}
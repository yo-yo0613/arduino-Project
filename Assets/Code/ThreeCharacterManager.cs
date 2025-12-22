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

    [Header("射擊特效 (靜態物件/Sprite)")]
    public GameObject RightShootingObject; 
    public GameObject CenterShootingObject;
    public GameObject LeftShootingObject;  

    [Header("★ 修改處：機器人也改成靜態物件 (拖入 GameObject)")]
    // 原本是 AnimatorSpeed，現在改成 GameObject
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
        bool k1 = Input.GetKey(KeyCode.Alpha1);
        bool k2 = Input.GetKey(KeyCode.Alpha2);
        bool k3 = Input.GetKey(KeyCode.Alpha3);

        int keyboardIndex = 0;

        if (k1 && k3)        keyboardIndex = 4;
        else if (k1 && k2)   keyboardIndex = 5;
        else if (k2 && k3)   keyboardIndex = 6;
        else if (k1)         keyboardIndex = 1;
        else if (k2)         keyboardIndex = 2;
        else if (k3)         keyboardIndex = 3;
        else                 keyboardIndex = 0;

        if (Input.GetKeyDown(KeyCode.Alpha0)) 
        {
            externalIndex = 0;
            UpdateState(0);
            return;
        }

        int finalIndex = (keyboardIndex != 0) ? keyboardIndex : externalIndex;

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
    }

    public void SetExclusive(int index)
    {
        bool activeLeft   = (index == 1 || index == 4 || index == 5); 
        bool activeCenter = (index == 2 || index == 5 || index == 6); 
        bool activeRight  = (index == 3 || index == 4 || index == 6); 

        if (leftPenguin)   leftPenguin.SetPressed(activeLeft);
        if (centerPenguin) centerPenguin.SetPressed(activeCenter);
        if (rightPenguin)  rightPenguin.SetPressed(activeRight);

        // 傳入對應的 GameObject
        UpdateVisuals(LeftShootingObject, LeftRobotObject, activeLeft, leftSound);
        UpdateVisuals(CenterShootingObject, CenterRobotObject, activeCenter, centerSound);
        UpdateVisuals(RightShootingObject, RightRobotObject, activeRight, rightSound);
    }

    // ★ 修改處：第二個參數也改成 GameObject
    private void UpdateVisuals(GameObject shootObj, GameObject robotObj, bool isActive, AudioClip directionSound)
    {
        bool shouldPlaySound = false;

        // 1. 處理射擊特效 (GameObject)
        if (shootObj != null)
        {
            if (!shootObj.activeSelf && isActive)
            {
                shootObj.SetActive(true); // 開啟
                shouldPlaySound = true;
            }
            else if (!isActive)
            {
                shootObj.SetActive(false); // 關閉
            }
        }

        // 2. ★ 處理機器人圖片 (GameObject) - 邏輯現在跟上面一樣了
        if (robotObj != null)
        {
            if (!robotObj.activeSelf && isActive)
            {
                robotObj.SetActive(true); // 開啟
                shouldPlaySound = true; 
            }
            else if (!isActive)
            {
                robotObj.SetActive(false); // 關閉
            }
        }

        // 播放音效邏輯
        if (shouldPlaySound && audioSource != null)
        {
            if (directionSound != null) audioSource.PlayOneShot(directionSound);
            if (buttonSound != null) audioSource.PlayOneShot(buttonSound);
            if (penguinSound != null) audioSource.PlayOneShot(penguinSound);
        }
    }
}
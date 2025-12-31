using UnityEngine;
using System.Collections; // 需要這個來用 Coroutine

public class CharacterButtonControlled : MonoBehaviour
{
    [Header("請拖入狀態物件")]
    public GameObject idleObject;   // 待機
    public GameObject actionObject; // 動作
    public GameObject failureObject; // ★ 新增：失敗動畫物件 (請把 R失敗/G失敗 拖進來)

    public float failureDuration = 0.5f; // 失敗動畫播多久

    private bool isFailing = false; // 是否正在播失敗動畫

    void Start()
    {
        SetPressed(false);
        if (failureObject != null) failureObject.SetActive(false);
    }

    // ★ 邏輯不變
    public void SetPressed(bool isPressed)
    {
        // 如果正在播失敗動畫，就暫時不要理會按鍵切換，以免穿幫
        // (或者你想讓按鍵優先，就把這行拿掉)
        if (isFailing) return; 

        UpdateVisuals(isPressed);
    }

    // 內部用來更新顯示的函式
    private void UpdateVisuals(bool isAction)
    {
        if (idleObject != null) idleObject.SetActive(!isAction);
        if (actionObject != null) actionObject.SetActive(isAction);
        if (failureObject != null) failureObject.SetActive(false);
    }

    // ★ 新增：播放失敗動畫
    public void PlayFailure()
    {
        if (failureObject == null) return;
        
        // 停止之前的協程 (如果連續 Miss)
        StopAllCoroutines();
        StartCoroutine(ShowFailureRoutine());
    }

    IEnumerator ShowFailureRoutine()
    {
        isFailing = true;

        // 關掉待機與動作，只開失敗
        if (idleObject != null) idleObject.SetActive(false);
        if (actionObject != null) actionObject.SetActive(false);
        if (failureObject != null) failureObject.SetActive(true);

        // 等待指定時間
        yield return new WaitForSeconds(failureDuration);

        // 還原回待機狀態
        isFailing = false;
        UpdateVisuals(false); // 變回待機
    }
}
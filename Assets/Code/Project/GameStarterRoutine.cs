using UnityEngine;
using System.Collections;

public class GameStarterRoutine : MonoBehaviour
{
    public GameObject countdownObject; 
    public float countdownTime = 3f;   

    [Header("需要延後啟動的物件")]
    public GameObject zombieManager;
    public GameObject sequencer; 

    [Header("背景音樂設定")]
    public AudioSource bgmAudio; // ★ 新增：用來放 Main Camera 上的音樂組件

    void Start()
    {
        // 凍結時間
        Time.timeScale = 0f; 

        if (zombieManager != null) zombieManager.SetActive(false);
        if (sequencer != null) sequencer.SetActive(false);

        StartCoroutine(StartGameAfterDelay());
    }

    IEnumerator StartGameAfterDelay()
    {
        countdownObject.SetActive(true);

        // 使用真實時間倒數 (不受 Time.timeScale 影響)
        yield return new WaitForSecondsRealtime(countdownTime);

        countdownObject.SetActive(false);
        Debug.Log("遊戲正式開始！");

        // 恢復時間流動
        Time.timeScale = 1f;

        // 打開原本關閉的物件
        if (zombieManager != null) zombieManager.SetActive(true);
        if (sequencer != null) sequencer.SetActive(true);

        // ==========================================
        // ★ 倒數結束，開始播放背景音樂！
        // ==========================================
        if (bgmAudio != null)
        {
            bgmAudio.Play();
        }
    }
}
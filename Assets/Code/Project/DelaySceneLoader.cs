using UnityEngine;
using UnityEngine.SceneManagement;

public class DelaySceneLoader : MonoBehaviour
{
    [Tooltip("等幾秒後切換場景")]
    public float delaySeconds = 5f;

    [Tooltip("要載入的下一個場景名稱")]
    public string nextSceneName;

    void Start()
    {
        // 一開始就啟動協程，等 delaySeconds 秒再跳場景
        StartCoroutine(LoadSceneAfterDelay());
    }

    System.Collections.IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(delaySeconds);
        SceneManager.LoadScene(nextSceneName);
    }
}

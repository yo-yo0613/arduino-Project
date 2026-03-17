using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    [Header("連結組件")]
    public ArduinoManager arduino;    // 拖入 Scene1 的 ArduinoManager
    public GameObject loadingScreen;
    public Slider progressBar;

    [Header("設定")]
    public string nextScene = "Scene2_Kitchen";
    public int fsrJumpThreshold = 600; // 壓力大於 600 則跳轉

    private bool _hasTriggered = false;

    void Update()
    {
        // 邏輯優化：在 Scene 1 監測 FSR 數值
        if (!_hasTriggered && arduino != null)
        {
            if (arduino.CurrentPressure > fsrJumpThreshold)
            {
                _hasTriggered = true; // 防止重複觸發
                Debug.Log("FSR 壓力達標，載入下一場景..." + arduino.CurrentPressure);
                LoadKitchenScene();
            }
        }
    }

    public void LoadKitchenScene()
    {
        StartCoroutine(LoadAsynchronously(nextScene));
    }

    IEnumerator LoadAsynchronously(string name)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(name);
        if (loadingScreen != null) loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            if (progressBar != null) progressBar.value = progress;
            yield return null;
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    public GameObject loadingScreen; // 載入畫面 UI
    public Slider progressBar;       // 進度條

    public string SceneName = "Scene2_Kitchen";

    public void LoadKitchenScene()
    {
        StartCoroutine(LoadAsynchronously(SceneName));
    }

    IEnumerator LoadAsynchronously(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        
        loadingScreen.SetActive(true); // 顯示載入畫面

        while (!operation.isDone)
        {
            // operation.progress 範圍是 0 到 0.9
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            if (progressBar != null)
                progressBar.value = progress;

            yield return null; // 等待下一幀，確保畫面不卡死
        }
    }
}
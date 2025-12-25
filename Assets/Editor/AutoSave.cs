#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class AutoSave
{
    // ★ 設定：每幾秒存一次？ (這裡設 300秒 = 5分鐘)
    private static float saveInterval = 60; 
    private static double nextSaveTime;

    // 建構子：Unity 啟動或編譯完時會執行
    static AutoSave()
    {
        nextSaveTime = EditorApplication.timeSinceStartup + saveInterval;
        EditorApplication.update += Update;
    }

    static void Update()
    {
        // 檢查時間到了沒
        if (EditorApplication.timeSinceStartup > nextSaveTime)
        {
            SaveScene();
            // 重設下一次存檔時間
            nextSaveTime = EditorApplication.timeSinceStartup + saveInterval;
        }
    }

    static void SaveScene()
    {
        // 只有在「非播放模式」且「非編譯中」才存檔，避免干擾測試
        if (!EditorApplication.isPlaying && !EditorApplication.isCompiling)
        {
            // 存檔目前開啟的場景
            EditorSceneManager.SaveOpenScenes();
            // 存檔所有 Asset 設定 (Prefab, Material 等)
            AssetDatabase.SaveAssets();
            
            Debug.Log($"<color=cyan>[AutoSave] 自動存檔完成！時間：{System.DateTime.Now:HH:mm:ss}</color>");
        }
    }
}
#endif
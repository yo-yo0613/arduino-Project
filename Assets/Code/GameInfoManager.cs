using UnityEngine;
using TMPro; // ★ 引用 TextMeshPro
using UnityEngine.SceneManagement; // ★ 引用場景管理

public class GameInfoManager : MonoBehaviour
{
    [Header("UI 連結")]
    public TextMeshPro scoreText; // 拖入顯示分數的 Text
    public TextMeshPro timeText;  // 拖入顯示時間的 Text 
       

    [Header("遊戲設定")]
    public float totalGameTime = 40.0f; // 遊戲時間 (秒)
    public string endSceneName = "End"; // 時間到要跳去的場景名稱

    private float timer;
    private bool isGameOver = false;

    void Start()
    {
        // 初始化計時器
        timer = totalGameTime;
        isGameOver = false;
    }

    void Update()
    {
        if (isGameOver) return;

        // 1. 倒數計時邏輯
        timer -= Time.deltaTime;

        // 2. 更新時間顯示 (Mathf.CeilToInt 會無條件進位，讓 0.1秒 也顯示 1)
        if (timeText != null)
        {
            // 確保顯示不小於 0
            float displayTime = Mathf.Max(0, timer); 
            timeText.text = $"{Mathf.CeilToInt(displayTime)}"; 
        }

        // 3. 更新分數顯示 (隨時讀取 GlobalGameManager)
        if (scoreText != null)
        {
            int currentScore = GlobalGameManager.CalculateTotalScore();
            scoreText.text = $"{currentScore}";
        }

        // 4. 檢查時間是否結束
        if (timer <= 0)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        isGameOver = true;
        GoogleSheetDataHandler.Instance.UploadScore(GlobalGameManager.CalculateTotalScore());
        Debug.Log("時間到！遊戲結束，切換場景...");

        // 這裡可以加一點延遲或特效，目前直接切換
        if (!string.IsNullOrEmpty(endSceneName))
        {
            SceneManager.LoadScene(endSceneName);
        }
        else
        {
            Debug.LogError("請在 Inspector 設定 End Scene Name！");
        }
    }
}
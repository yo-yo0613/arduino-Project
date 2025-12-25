using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultManager : MonoBehaviour
{
    [Header("UI 顯示")]
    public TMP_Text satisfactionText;
    public TMP_Text profitText;
    public TMP_Text finalScoreText;
    public TMP_Text highScoreText;

    void Start()
    {
        // 1. 執行效率優化：從靜態變數或 DataManager 讀取數據 (不需重新計算)
        float finalSatisfaction = GameData.Satisfaction;
        float finalProfit = GameData.Profit;
        
        // 2. 結算公式：滿意度 * 10 + 收益 (可自訂權重)
        float totalScore = (finalSatisfaction * 10f) + finalProfit;

        // 顯示數據
        satisfactionText.text = $"滿意度: {finalSatisfaction:F0}%";
        profitText.text = $"收益: ${finalProfit:F1}";
        finalScoreText.text = $"總分: {totalScore:F0}";

        // 3. 預防 Bug：檢查並更新最高分
        HandleHighScore(totalScore);
    }

    void HandleHighScore(float currentScore)
    {
        float topScore = PlayerPrefs.GetFloat("HighScore", 0);
        if (currentScore > topScore)
        {
            PlayerPrefs.SetFloat("HighScore", currentScore);
            PlayerPrefs.Save(); // 強制存檔，預防閃退導致記錄消失
            highScoreText.text = "New High Score!";
        }
        else
        {
            highScoreText.text = $"High Score: {topScore:F0}";
        }
    }

    public void RestartGame()
    {
        // 優化：重玩時清空暫存數據
        GameData.Reset();
        SceneManager.LoadScene("Scene1_Order");
    }
}

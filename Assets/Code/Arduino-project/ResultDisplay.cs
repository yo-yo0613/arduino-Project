using UnityEngine;
using TMPro;

public class ResultDisplay : MonoBehaviour {
    public TextMeshProUGUI scoreText;

    void Start() {
        // 從全域記憶體讀取
        int finalAmount = PlayerPrefs.GetInt("FinalScore", 0);
        
        // 只顯示純數字轉字串
        if (scoreText != null) {
            scoreText.text = finalAmount.ToString();
        } else {
            Debug.LogError("ResultDisplay 找不到 scoreText 物件！請在 Inspector 連結它。");
        }
    } 
}
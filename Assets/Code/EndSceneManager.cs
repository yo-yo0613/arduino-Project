using UnityEngine;
using UnityEngine.Video;
using TMPro;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class HighScoreEntry
{
    public string name;
    public int score;
    public string id; // 唯一識別碼
}

[System.Serializable]
public class LeaderboardData
{
    public List<HighScoreEntry> entries = new List<HighScoreEntry>();
}

public class EndSceneManager : MonoBehaviour
{
    [Header("影片控制")]
    public VideoPlayer videoPlayer;
    public float showStatsTime = 7.0f;
    public float showLeaderboardTime = 10.0f;
    public float hideAllTime = 13.0f;

    [Header("UI - 個人成績面板")]
    public GameObject statsPanel;
    public TMP_Text PerfectText;
    public TMP_Text GoodText;
    public TMP_Text MissText;
    public TMP_Text ComboText;
    public TMP_Text ScoreText;

    [Header("UI - 排行榜面板")]
    public GameObject leaderboardPanel;
    public TMP_Text NumberOneText;
    public TMP_Text NumberTwoText;
    public TMP_Text NumberThreeText;
    public TMP_Text NumberFourText;
    public TMP_Text NumberFiveText;
    public TMP_Text NumberSixText;
    public TMP_Text YourRankText;

    private bool hasSaved = false;
    private string myCurrentGameID; 

    void Start()
    {
        if (statsPanel) statsPanel.SetActive(false);
        if (leaderboardPanel) leaderboardPanel.SetActive(false);

        // ★ Debug: 確保名字有傳過來
        Debug.Log($"[EndScene START] Global Name: '{GlobalGameManager.playerName}'");
        Debug.Log($"[EndScene START] Score: {GlobalGameManager.CalculateTotalScore()}");

        if (!hasSaved)
        {
            SaveScoreToLeaderboard();
            hasSaved = true;
        }

        // UI 顯示
        int totalScore = GlobalGameManager.CalculateTotalScore();
        // ★ 這裡修改：從 Global 讀取 maxCombo
        int maxCombo = GlobalGameManager.maxCombo;

        if (PerfectText) PerfectText.text = $"{GlobalGameManager.perfectCount}";
        if (GoodText) GoodText.text = $"{GlobalGameManager.goodCount + GlobalGameManager.greatCount}";
        if (MissText) MissText.text = $"{GlobalGameManager.missCount + GlobalGameManager.badCount}";
        if (ComboText)   ComboText.text = $"{maxCombo}";
        if (ScoreText) ScoreText.text = $"{totalScore}";
    }

    void Update()
    {
        if (videoPlayer == null) return;
        double t = videoPlayer.time;

        if (t >= showStatsTime && t < showLeaderboardTime)
        {
            if (statsPanel) statsPanel.SetActive(true);
            if (leaderboardPanel) leaderboardPanel.SetActive(false);
        }
        else if (t >= showLeaderboardTime && t < hideAllTime)
        {
            if (statsPanel) statsPanel.SetActive(false);
            if (leaderboardPanel) leaderboardPanel.SetActive(true);
            UpdateLeaderboardUI(); // 持續更新確保顯示正確
        }
        else if (t >= hideAllTime)
        {
            if (statsPanel) statsPanel.SetActive(false);
            if (leaderboardPanel) leaderboardPanel.SetActive(false);
        }
    }

    void SaveScoreToLeaderboard()
    {
        string json = PlayerPrefs.GetString("Leaderboard", "{}");
        LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json);
        if (data == null) data = new LeaderboardData();

        int currentScore = GlobalGameManager.CalculateTotalScore();
        string currentName = GlobalGameManager.playerName;

        // ★★★ 修改處開始 ★★★
        // 原本這裡會把空名字改成 "Player"，我們把它註解掉或刪除
        // 這樣如果名字是空的，排行榜上就會顯示空白，而不會顯示 "Player"
        
        /* if (string.IsNullOrEmpty(currentName)) 
        {
            currentName = "Player";
            Debug.LogWarning("注意：GlobalGameManager.playerName 是空的！使用預設值。");
        }
        */

        // 如果你希望「沒有名字的人根本不要紀錄到排行榜」，請取消下面這行的註解：
        // if (string.IsNullOrEmpty(currentName)) return; 
        
        // ★★★ 修改處結束 ★★★

        // 產生 ID
        myCurrentGameID = System.Guid.NewGuid().ToString();

        // 新增資料
        data.entries.Add(new HighScoreEntry { 
            name = currentName, 
            score = currentScore,
            id = myCurrentGameID 
        });

        // 排序與裁切
        data.entries = data.entries.OrderByDescending(x => x.score).ToList();
        if (data.entries.Count > 10) data.entries = data.entries.GetRange(0, 10);

        // 存檔
        string newJson = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("Leaderboard", newJson);
        PlayerPrefs.Save();
        
        Debug.Log($"分數已儲存: {currentName} - {currentScore} (ID: {myCurrentGameID})");
    }

    void UpdateLeaderboardUI()
    {
        string json = PlayerPrefs.GetString("Leaderboard", "{}");
        LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json);
        if (data == null) return;

        SetRankText(NumberOneText, data.entries, 0);
        SetRankText(NumberTwoText, data.entries, 1);
        SetRankText(NumberThreeText, data.entries, 2);
        SetRankText(NumberFourText, data.entries, 3);
        SetRankText(NumberFiveText, data.entries, 4);
        SetRankText(NumberSixText, data.entries, 5);

        if (YourRankText != null)
        {
            int rank = -1;
            for(int i=0; i<data.entries.Count; i++)
            {
                if (data.entries[i].id == myCurrentGameID)
                {
                    rank = i + 1;
                    break;
                }
            }
            YourRankText.text = (rank != -1) ? $"{rank}" : "-";
        }
    }

    void SetRankText(TMP_Text textComp, List<HighScoreEntry> entries, int index)
    {
        if (textComp == null) return;
        if (index < entries.Count)
            textComp.text = $"{entries[index].name}";
        else
            textComp.text = "---";
    }
    
    [ContextMenu("Clear Leaderboard")]
    public void ClearLeaderboard()
    {
        PlayerPrefs.DeleteKey("Leaderboard");
        Debug.Log("排行榜已清除");
    }
}
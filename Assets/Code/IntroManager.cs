using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroManager : MonoBehaviour
{
    [Header("開場影片")]
    public VideoPlayer videoPlayer;

    [Header("UI 顯示")]
    public TMP_Text nameText;        // 顯示最終組合的名字 (例如：超級帥氣企鵝)
    
    // 如果你想讓玩家知道現在選到哪個詞，可以分開三個 Text 顯示，或者只用上面那個顯示總合
    [Header("分開顯示的 UI (選用，可不填)")]
    public TMP_Text part1Text; // 顯示程度詞
    public TMP_Text part2Text; // 顯示形容詞
    public TMP_Text part3Text; // 顯示名詞

    [Header("下一個 Scene 名稱")]
    public string nextSceneName = "SampleScene";

    // ================== 詞庫設定 ==================
    [Header("詞庫 1 (紅踏板/按鍵1)：程度形容詞")]
    public string[] prefixWords = new string[] { 
        "超級", "無敵", "有點", "非常", "絕對", "傳說的", "暴走的", "究極" 
    };

    [Header("詞庫 2 (綠踏板/按鍵2)：形容詞")]
    public string[] adjWords = new string[] { 
        "帥氣", "搞笑", "暴力", "聰明", "軟爛", "幸運", "瘋狂", "神祕" 
    };

    [Header("詞庫 3 (藍踏板/按鍵3)：名詞")]
    public string[] nounWords = new string[] { 
        "企鵝", "隊長", "機器人", "殭屍", "大叔", "殺手", "王者", "小兵" 
    };

    // 記錄目前選到第幾個詞
    private int index1 = 0;
    private int index2 = 0;
    private int index3 = 0;

    private bool videoFinished = false;

    void Start()
    {
        // 初始化名字
        GlobalGameManager.playerName = "";

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
        else
        {
            videoFinished = true;
            UpdateNameDisplay();
        }

        // 初始化顯示
        UpdateNameDisplay();
    }

    void OnDestroy()
    {
        if (videoPlayer != null) videoPlayer.loopPointReached -= OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        videoFinished = true;
        Debug.Log("[INTRO] 影片結束，開始選名字");
        UpdateNameDisplay();
    }

    void Update()
    {
        // 測試用：跳過影片
        if (!videoFinished && Input.GetKeyDown(KeyCode.S))
        {
            videoFinished = true;
            UpdateNameDisplay();
        }

        // 鍵盤模擬 FSR
        if (Input.GetKeyDown(KeyCode.Alpha1)) OnFsPos(1); // 切換詞 1
        if (Input.GetKeyDown(KeyCode.Alpha2)) OnFsPos(2); // 切換詞 2
        if (Input.GetKeyDown(KeyCode.Alpha3)) OnFsPos(3); // 切換詞 3
        
        // 確認鍵 (4 / Enter / Space)
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) 
        {
            OnFsPos(4); 
        }
    }

    public void OnFsPos(int pos)
    {
        if (!videoFinished) return;

        if (pos == 1)
        {
            // 紅踏板：切換程度詞
            index1 = (index1 + 1) % prefixWords.Length; // 循環切換
            UpdateNameDisplay();
        }
        else if (pos == 2)
        {
            // 綠踏板：切換形容詞
            index2 = (index2 + 1) % adjWords.Length;
            UpdateNameDisplay();
        }
        else if (pos == 3)
        {
            // 藍踏板：切換名詞
            index3 = (index3 + 1) % nounWords.Length;
            UpdateNameDisplay();
        }
        else if (pos == 4)
        {
            // 確認並開始
            ConfirmAndGoNext();
        }
    }

    // 更新畫面上的文字
    void UpdateNameDisplay()
    {
        string p1 = prefixWords[index1];
        string p2 = adjWords[index2];
        string p3 = nounWords[index3];

        // 組合起來：超級 + 帥氣 + 企鵝
        string fullName = p1 + p2 + p3;

        // 如果你有拉 Main Name Text，顯示完整名字
        if (nameText != null) nameText.text = fullName;

        // 如果你有拉分開的 Text (選用)，也可以分開顯示，讓玩家看清楚每一段變什麼
        if (part1Text != null) part1Text.text = p1;
        if (part2Text != null) part2Text.text = p2;
        if (part3Text != null) part3Text.text = p3;
    }

    public void ConfirmAndGoNext()
    {
        // 組合最終名字
        string finalName = prefixWords[index1] + adjWords[index2] + nounWords[index3];

        GlobalGameManager.ResetData();
        GlobalGameManager.playerName = finalName;

        Debug.Log($"[INTRO] 名字已決定: '{finalName}' -> 前往 {nextSceneName}");

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
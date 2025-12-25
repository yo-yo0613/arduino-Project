using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("1. 影片與場景")]
    public VideoPlayer videoPlayer;
    public string nextSceneName = "Open"; 

    [Header("2. 文字顯示")]
    public TMP_Text mainDisplayText; // 顯示名字的 Text

    [Header("3. 提示文字 (接力顯示)")]
    public GameObject hintText1; // 先出來 (例如: 你的署名是...)
    public GameObject hintText2; // 後出來 (例如: 按123選字...)

    // --- 詞庫 ---
    private string[] adjectives = new string[] { "胖胖的", "邪惡的", "熟透的", "毛茸茸的", "超派的", "溼答答的", "健美的", "黏黏的", "歐氣的", "香香軟軟的", "可悲的", "窮困的", "頂級的", "高雅的" };
    private string[] nouns = new string[] { "企鵝", "殭屍", "鳳梨酥", "高速婆婆", "青蛙", "羊咩咩", "湖中女神", "霸總", "大野狼", "鴿子", "韓老師", "賽馬娘", "白飯", "工程師", "406" };
    private string[] verbs = new string[] { "拋出完美的拋物線", "翻身", "被拔了吃掉", "呱呱叫", "墜落落落落↓", "飛上天", "掉毛毛", "光頭亮光光", "流汗", "吁吁叫", "恨透專題", "emo了", "酸宗痛", "爆炸了" };

    private int idx1 = 0;
    private int idx2 = 0;
    private int idx3 = 0;
    private bool isInteractionEnabled = false;

    void Start()
    {
        // 1. 畫面初始化：隱藏所有文字
        if (hintText1 != null) hintText1.SetActive(false);
        if (hintText2 != null) hintText2.SetActive(false);
        if (mainDisplayText != null) mainDisplayText.gameObject.SetActive(false);
        
        isInteractionEnabled = false;
        RandomizeName();

        // 2. 影片設定
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false; 
            videoPlayer.isLooping = false;   // 強制不循環
            videoPlayer.time = 0;
            videoPlayer.loopPointReached += OnVideoFinished; // 綁定結束事件
            videoPlayer.Play(); // 直接播放
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        // 影片播完，定格
        vp.Pause();
        
        // 開始執行「切換顯示」的動畫
        StartCoroutine(ShowUISequence());
    }

    // ★★★ 關鍵修改：Hint1 消失後 -> Hint2 才出來 ★★★
    IEnumerator ShowUISequence()
    {
        // 1. 先顯示 Hint 1 (例如：你的名字是...)
        if (hintText1 != null) hintText1.SetActive(true);
        
        // 2. 讓 Hint 1 停留 2 秒 (讓玩家看清楚)
        yield return new WaitForSeconds(2.0f);

        // 3. ★ 關鍵：把 Hint 1 關掉！
        if (hintText1 != null) hintText1.SetActive(false);

        // 4. 再顯示 Hint 2 (操作說明) 和 名字
        if (hintText2 != null) hintText2.SetActive(true);
        if (mainDisplayText != null) mainDisplayText.gameObject.SetActive(true);

        // 5. 最後才允許玩家按按鍵
        isInteractionEnabled = true;
    }

    void Update()
    {
        // 鍵盤備用輸入
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) OnFsPos(1);
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) OnFsPos(2);
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) OnFsPos(3);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) OnFsPos(4);
    }

    public void OnFsPos(int pos)
    {
        // 如果 UI 還沒全部跑出來，不准動
        if (!isInteractionEnabled) return;

        if (pos == 1) { idx1 = (idx1 + 1) % adjectives.Length; UpdateDisplay(); }
        else if (pos == 2) { idx2 = (idx2 + 1) % nouns.Length; UpdateDisplay(); }
        else if (pos == 3) { idx3 = (idx3 + 1) % verbs.Length; UpdateDisplay(); }
        else if (pos == 4) ConfirmAndNext();
    }

    void RandomizeName()
    {
        idx1 = Random.Range(0, adjectives.Length);
        idx2 = Random.Range(0, nouns.Length);
        idx3 = Random.Range(0, verbs.Length);
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (mainDisplayText != null)
            mainDisplayText.text = adjectives[idx1] + nouns[idx2] + verbs[idx3];
    }

    void ConfirmAndNext()
    {
        string finalName = adjectives[idx1] + nouns[idx2] + verbs[idx3];
        GlobalGameManager.playerName = finalName;
        // ★★★ Debug 顯示確認 ★★★
        Debug.Log($"<color=yellow>[IntroManager] 玩家選擇的名字是: {finalName}</color>");
        Debug.Log($"<color=green>[GlobalGameManager] 確認已存檔! 現在 GlobalGameManager.playerName = {GlobalGameManager.playerName}</color>");
        SceneManager.LoadScene(nextSceneName);
    }

    void OnDestroy()
    {
        if (videoPlayer != null) videoPlayer.loopPointReached -= OnVideoFinished;
    }
}
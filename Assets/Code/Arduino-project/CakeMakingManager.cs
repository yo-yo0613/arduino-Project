using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class CakeMakingManager : MonoBehaviour {
    public ArduinoManager arduino;
    public Transform lifePointBar; 
    public TextMeshProUGUI titleText;
    public RectTransform selector;
    public TextMeshProUGUI[] options;

    [Header("Stage Parents (空物件)")]
    public GameObject[] stage1_Parents; 
    public GameObject[] stage2_Parents; 
    public GameObject[] stage3_Parents; 

    private int currentStage = 1; 
    private int selectedIndex = 0;
    private bool isSqueezing = false;
    private bool inputLock = false; 
    private GameObject currentScalingTarget; 
    private GameObject currentNumberUI; 
    
    // 修正點 1：移除 static，確保每次進入場景金額都是 1000
    private int currentBalance = 1000; 
    private int successCount = 0; 
    private const float MAX_LIFE_SCALE = 8f; 

    void Start() {
        arduino = ArduinoManager.Instance; 
        // 自動連結跨場景的 ArduinoManager
        if (arduino == null) {
            Debug.LogError("找不到 ArduinoManager！請確認第一個場景有放它。");
        }
        
        // 修正點 2：明確重置金額與 UI 長度
        currentBalance = 1000; 
        UpdateLifeBar(); 

        SetGroupsActive(stage1_Parents, false);
        SetGroupsActive(stage2_Parents, false);
        SetGroupsActive(stage3_Parents, false);
        
        UpdateSelectorUI();
        UpdateOptionTexts("Peanut", "Mint", "Vanilla"); 
        Debug.Log("Game Started. Initial Balance: " + currentBalance);
    }

    void Update() {
        if (arduino == null) return;
        if (!isSqueezing) {
            HandleJoystickSelection();
            // 搖桿向右確認開始擠壓
            if (arduino.Joystick.x > 800 && !inputLock) StartSqueezing();
        } else {
            HandleFSRScaling();
        }
    }

    void HandleJoystickSelection() {
        int joyY = arduino.Joystick.y;
        if (!inputLock) {
            if (joyY > 800) { selectedIndex = (selectedIndex - 1 + 3) % 3; UpdateSelectorUI(); inputLock = true; }
            else if (joyY < 200) { selectedIndex = (selectedIndex + 1) % 3; UpdateSelectorUI(); inputLock = true; }
            if (inputLock){ 
                Debug.Log("Selected Index: " + selectedIndex);
                StartCoroutine(ResetInputLock());
            }
        }
    }

    IEnumerator ResetInputLock() {
        yield return new WaitForSeconds(0.2f);
        inputLock = false;
    }

    void StartSqueezing() {
        isSqueezing = true;
        GameObject parentObj = (currentStage == 1) ? stage1_Parents[selectedIndex] : 
                               (currentStage == 2) ? stage2_Parents[selectedIndex] : 
                               stage3_Parents[selectedIndex];
        
        if (parentObj != null) {
            parentObj.SetActive(true);
            // 抓取子物件順序：0 是編號，1 是食材模型
            if (parentObj.transform.childCount > 1) {
                currentNumberUI = parentObj.transform.GetChild(0).gameObject;
                currentScalingTarget = parentObj.transform.GetChild(1).gameObject;
                currentScalingTarget.transform.localScale = Vector3.one * 0.05f; 
                Debug.Log("Started squeezing: " + currentScalingTarget.name);
            }
        }
    }

    void HandleFSRScaling() {
        if (currentScalingTarget == null) return;
        if (arduino.CurrentPressure > 100) {
            // FSR 壓力控制模型縮放
            float grow = (arduino.CurrentPressure / 1023f) * Time.deltaTime * 0.4f;
            float s = Mathf.Clamp(currentScalingTarget.transform.localScale.x + grow, 0.05f, 0.5f);
            currentScalingTarget.transform.localScale = Vector3.one * s;
        } else if (currentScalingTarget.transform.localScale.x > 0.06f) {
            FinishStep();
        }
    }

    void FinishStep() {
        isSqueezing = false;
        bool isSuccess = (arduino.CurrentPressure >= 600 && arduino.CurrentPressure <= 800);
        if (isSuccess) successCount++;

        // 根據模型縮放比例扣錢
        int cost = (int)(currentScalingTarget.transform.localScale.x * 300); 
        currentBalance -= cost;
        UpdateLifeBar();
        Debug.Log("Cost: $" + cost + " | Balance: $" + currentBalance);

        // 僅隱藏編號 UI (Index 0)
        if (currentNumberUI != null) currentNumberUI.SetActive(false);

        if (currentStage == 1) NextStage("Choose Jam", new string[]{"Chocolate", "Strawberry", "Honey"});
        else if (currentStage == 2) NextStage("Choose Decoration", new string[]{"Lemon", "Coco", "Candy"});
        else StartCoroutine(DelayedEndGame());
    }

    IEnumerator DelayedEndGame() {
        titleText.text = "Finished!";
        yield return new WaitForSeconds(4f); // 延遲 4 秒跳轉
        EndGame();
    }

    void EndGame() {
        if (successCount >= 2) currentBalance += 200; // 成功兩次加 200
        PlayerPrefs.SetInt("FinalScore", currentBalance);
        PlayerPrefs.Save(); // 強制存檔
        SceneManager.LoadScene("ResultScene"); 
    }

    void UpdateLifeBar() {
        if (lifePointBar == null) return;
        
        // 強制確保餘額不低於 0
        float safeBalance = Mathf.Max(0, (float)currentBalance);
        // 計算剩餘百分比
        float ratio = safeBalance / 1000f;
        // 映射到 Scale X (0 到 8)
        float targetScaleX = ratio * MAX_LIFE_SCALE; 
        
        lifePointBar.localScale = new Vector3(targetScaleX, lifePointBar.localScale.y, 1);
        Debug.Log($"[UI Bar] Balance: {currentBalance} | TargetScaleX: {targetScaleX}");
    }

    void NextStage(string title, string[] opts) {
        currentStage++;
        selectedIndex = 0;
        titleText.text = title;
        UpdateOptionTexts(opts[0], opts[1], opts[2]);
        UpdateSelectorUI();
        inputLock = false;
    }

    void UpdateOptionTexts(string o1, string o2, string o3) {
        options[0].text = o1; options[1].text = o2; options[2].text = o3;
    }
    
    void SetGroupsActive(GameObject[] arr, bool state) { foreach(var g in arr) if(g != null) g.SetActive(state); }
    
    void UpdateSelectorUI() {
        if (options == null || options.Length <= selectedIndex) return;
        float yPos = options[selectedIndex].rectTransform.anchoredPosition.y;
        selector.anchoredPosition = new Vector2(selector.anchoredPosition.x, yPos);
    }
}
using UnityEngine;
using UnityEngine.UI;

public class OverloadWarning : MonoBehaviour
{
    public ArduinoManager arduino; // 引用讀取壓力數值的腳本
    public Animator warningAnimator; // UI 的 Animator
    public int overloadThreshold = 950; // 設定過載門檻 (例如 950 以上)

    private bool _isOverloaded = false;

    void Update()
    {
        int pressure = arduino.CurrentPressure;

        // 邏輯優化：只有在「狀態改變」時才觸發動畫觸發器 (Trigger)
        if (pressure >= overloadThreshold && !_isOverloaded)
        {
            _isOverloaded = true;
            TriggerWarning(true);
        }
        else if (pressure < overloadThreshold && _isOverloaded)
        {
            _isOverloaded = false;
            TriggerWarning(false);
        }
    }

    void TriggerWarning(bool state)
    {
        if (state)
        {
            // 觸發閃爍紅字動畫
            warningAnimator.SetTrigger("ShowWarning");
            // 你也可以在這裡加入手機震動或音效，增加緊張感
            Debug.LogWarning("硬體壓力過載！請玩家放手。");
        }
        else
        {
            // 停止動畫，回到隱藏狀態
            warningAnimator.SetTrigger("HideWarning");
        }
    }
}
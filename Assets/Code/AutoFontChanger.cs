using UnityEngine;
using TMPro; // ★ 記得要引用這個

public class AutoFontChanger : MonoBehaviour
{
    [Header("把轉好的 TMP Font Asset 拖進這裡")]
    public TMP_FontAsset myComicFont;

    void Start()
    {
        ChangeAllFonts();
    }

    // 提供一個公開方法，你也可以在按按鈕時呼叫它
    public void ChangeAllFonts()
    {
        if (myComicFont == null)
        {
            Debug.LogError("請先在 Inspector 拖入字型檔案！");
            return;
        }

        // 1. 抓出當前場景所有開啟的 TMP_Text 物件
        TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>();

        // 2. 跑迴圈全部換掉
        foreach (var text in allTexts)
        {
            text.font = myComicFont;
        }

        Debug.Log($"已將場景中 {allTexts.Length} 個文字物件更換為新字體！");
    }
}
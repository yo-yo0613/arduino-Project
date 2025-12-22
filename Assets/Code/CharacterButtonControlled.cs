using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonControlled : MonoBehaviour
{
    public Sprite idleSprite;    // 平常圖
    public Sprite pressedSprite; // 被按時圖

    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
        if (img != null && idleSprite != null)
            img.sprite = idleSprite;
    }

    // 外部呼叫：設定這個角色目前是不是被「自己的按鈕」按下
    public void SetPressed(bool isPressed)
    {
        if (img == null) return;

        Sprite targetSprite = (isPressed && pressedSprite != null) ? pressedSprite : idleSprite;

        // ★ 優化：如果圖片已經是正確的那張，就直接 return，不要浪費效能去賦值
        if (img.sprite == targetSprite) return;

        img.sprite = targetSprite;
    }
}
using UnityEngine;
using System.Collections;

public class SpriteMover : MonoBehaviour 
{
    public RectTransform targetUI;    // 拖入「甲方黃」的 RectTransform
    public Vector2 startPos;          // 起點座標 (例如 -800, 0)
    public Vector2 endPos;            // 終點座標 (例如 0, 0)
    public float duration = 2.0f;     // 移動花費時間 (秒)

    void Start() 
    {
        // 遊戲開始後執行移動
        StartCoroutine(MoveRoutine());
    }

    IEnumerator MoveRoutine() 
    {
        float elapsedTime = 0;
        
        while (elapsedTime < duration) 
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            // 使用 Lerp 進行平滑插值
            targetUI.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            
            yield return null; // 等待下一幀
        }
        
        targetUI.anchoredPosition = endPos; // 確保最後停在精確位置
    }
}
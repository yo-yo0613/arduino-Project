using UnityEngine;

public class ToggleScreenMode : MonoBehaviour
{
    void Update()
    {
        // 偵測玩家是否按下 F11 鍵
        if (Input.GetKeyDown(KeyCode.F11))
        {
            // 切換全螢幕與視窗化
            ToggleFullscreen();
        }
    }

    void ToggleFullscreen()
    {
        // 反轉目前的螢幕狀態
        Screen.fullScreen = !Screen.fullScreen;

        // 印出當前狀態到 Console 方便偵錯
        if (Screen.fullScreen)
        {
            Debug.Log("已切換至：視窗模式");
        }
        else
        {
            Debug.Log("已切換至：全螢幕模式");
        }
    }
}
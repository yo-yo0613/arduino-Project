using UnityEngine;

public static class GlobalGameManager
{
    public static string playerName = "";

    // 分數計數
    public static int perfectCount = 0;
    public static int greatCount = 0;
    public static int goodCount = 0;
    public static int missCount = 0;
    public static int badCount = 0;

    // ★ 新增：連擊相關
    public static int currentCombo = 0; // 現在連到幾下了
    public static int maxCombo = 0;     // 整場遊戲最高的連擊紀錄

    public static void ResetData()
    {
        perfectCount = 0;
        greatCount = 0;
        goodCount = 0;
        missCount = 0;
        badCount = 0;
        
        // ★ 重置連擊
        currentCombo = 0;
        maxCombo = 0;
    }

    public static int CalculateTotalScore()
    {
        // 你也可以把 maxCombo 加進總分公式裡，這裡先維持原樣
        return (perfectCount * 5) + 
               (greatCount * 4) + 
               (goodCount * 3) + 
               (missCount * 1) + 
               (badCount * 0);
    }
}
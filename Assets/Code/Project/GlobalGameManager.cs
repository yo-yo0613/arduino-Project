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
        // 先把計算結果存進一個變數 total
        int total = (perfectCount * 5) + 
                    (greatCount * 4) + 
                    (goodCount * 3) + 
                    (missCount * -1) +  // ★ 修改這裡：把 1 改成 -1，代表扣 1 分
                    (badCount * 0);     // 如果 Bad 也要扣分，這裡也可以改成 -1 或 -2

        // ★ 額外建議：防止總分變成負數 (如果你的遊戲允許負分，這行可以不用加)
        // Mathf.Max(0, total) 的意思是：在 0 和 total 之間取比較大的那個。
        // 所以如果 total 被扣到變成 -5，它會回傳 0。
        return Mathf.Max(0, total); 
    }
}
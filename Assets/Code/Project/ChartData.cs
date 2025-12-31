using System;
using System.Collections.Generic;

// 這是單純的資料包，用來對應 JSON 格式
[Serializable]
public class NoteEvent
{
    public float timeMs; // 打擊時間 (毫秒)
    public int lane;     // 軌道 (0, 1, 2)
}

[Serializable]
public class ChartData
{
    public List<NoteEvent> notes;
}
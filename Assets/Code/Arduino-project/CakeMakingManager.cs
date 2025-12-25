using UnityEngine;

public class CakeMakingManager : MonoBehaviour
{
    [Header("連結組件")]
    public ArduinoManager arduino;
    public ObjectPooler toppingPool; 
    public Transform cakeTransform; // 拖入 2D 蛋糕 Sprite 的 Transform

    [Header("遊戲數值設定")]
    public float perfectMin = 500f;
    public float perfectMax = 800f;
    public float costPerTopping = 0.5f;

    [Header("即時狀態")]
    public float currentSatisfaction = 100f;
    public float currentProfit = 0f;

    private float _spawnTimer = 0f;
    private float _spawnInterval = 0.12f; // 優化：配合 2D 動畫節奏

    void Update()
    {
        int pressure = arduino.CurrentPressure;

        // 2D 視覺優化：蛋糕高度隨壓力拉長 (1.0 到 1.4 倍)
        float heightScale = Mathf.Lerp(1.0f, 1.4f, pressure / 1024f);
        cakeTransform.localScale = new Vector3(1, heightScale, 1);

        if (pressure > 100) 
        {
            HandleToppingCreation();
            CalculateScore(pressure);
        }

        // 優化：將數據即時存入靜態類別，讓 ResultManager 讀得到
        GameData.Satisfaction = currentSatisfaction;
        GameData.Profit = currentProfit;
    }

    void HandleToppingCreation()
    {
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= _spawnInterval)
        {
            GameObject topping = toppingPool.GetPooledObject();
            if (topping != null)
            {
                // 2D 座標優化：僅使用 Vector2
                topping.transform.position = (Vector2)cakeTransform.position + new Vector2(Random.Range(-0.5f, 0.5f), 2f);
                
                // 預防 Bug：若有 2D 物理，重設速度
                Rigidbody2D rb = topping.GetComponent<Rigidbody2D>();
                if(rb != null) rb.velocity = Vector2.zero;

                topping.SetActive(true);
                currentProfit -= costPerTopping;
            }
            _spawnTimer = 0f;
        }
    }

    void CalculateScore(int pressure)
    {
        if (pressure < perfectMin || pressure > perfectMax)
        {
            currentSatisfaction -= Time.deltaTime * 8f; 
        }
        currentSatisfaction = Mathf.Clamp(currentSatisfaction, 0, 100);
    }
}
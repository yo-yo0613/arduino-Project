using UnityEngine;

public class TiltFromAccel : MonoBehaviour
{
    public UDP_ADXL_Receiver imu;   // 拖進 IMU_Receiver
    [Range(0.1f, 5f)] public float gain = 1.5f;   // 額外放大
    [Range(0f, 1f)] public float smooth = 0.15f;  // 低通濾波

    Vector3 gLP = new Vector3(0, 0, 1);           // 初始重力朝 +Z
    void Update()
    {
        if (imu == null) return;

        // 取得加速度（單位 g）
        var g = imu.accel_g;                 // 例如 (ax, ay, az)
        if (g.sqrMagnitude < 0.01f) return;  // 還沒接到資料

        // 低通
        gLP = Vector3.Lerp(gLP, g * gain, 1f - Mathf.Exp(-smooth * Time.deltaTime));

        // 將當前「重力方向」對齊到 Unity 的「世界 +Z 朝上」的反向
        //（若你的靜止方向不同，可把 target 改成 Vector3.up 或 -Vector3.up）
        var target = gLP.normalized;              // 感測到的重力向量
        var rot = Quaternion.FromToRotation(target, Vector3.forward); // 以 +Z 為上
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, 0.2f);
    }
}

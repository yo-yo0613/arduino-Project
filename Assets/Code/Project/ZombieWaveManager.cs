using UnityEngine;

public class ZombieWaveManager : MonoBehaviour
{
    [Header("三條 Lane 各自一隻 Zombie")]
    public ZombieLaneController lane1Zombie;   // Inspector 拖「Lane1 的僵屍」
    public ZombieLaneController lane2Zombie;   // Inspector 拖「Lane2 的僵屍」
    public ZombieLaneController lane3Zombie;   // Inspector 拖「Lane3 的僵屍」

    [Header("三條 Lane 的出生點 (Z 起點)")]
    public Transform lane1SpawnPoint;
    public Transform lane2SpawnPoint;
    public Transform lane3SpawnPoint;

    // 這個腳本不再自己亂出怪，所以不用 StartCoroutine 了
    void Start()
    {
        // 如果一開始不想看到僵屍，可以先關掉
        // if (lane1Zombie != null) lane1Zombie.gameObject.SetActive(false);
        // if (lane2Zombie != null) lane2Zombie.gameObject.SetActive(false);
        // if (lane3Zombie != null) lane3Zombie.gameObject.SetActive(false);
    }

    /// <summary>
    /// 由 MusicConductor 呼叫：在指定的 lane 上出一隻僵屍
    /// lane = 1 / 2 / 3
    /// </summary>
    public void SpawnZombie(int lane)
    {
        ZombieLaneController z = null;
        Transform spawnPoint   = null;

        switch (lane)
        {
            case 1:
                z = lane1Zombie;
                spawnPoint = lane1SpawnPoint;
                break;
            case 2:
                z = lane2Zombie;
                spawnPoint = lane2SpawnPoint;
                break;
            case 3:
                z = lane3Zombie;
                spawnPoint = lane3SpawnPoint;
                break;
            default:
                return;
        }

        if (z == null || spawnPoint == null) return;

        // 確保這隻僵屍有顯示
        z.gameObject.SetActive(true);

        // 重設到出生點（保持 lane 不變，只改 Z）
        Vector3 pos = z.transform.position;
        pos.x = spawnPoint.position.x;
        pos.y = spawnPoint.position.y;
        pos.z = spawnPoint.position.z;
        z.transform.position = pos;

        // 讓動畫從頭再跑一次（你的 Animator 狀態名稱可能不一樣，自己換）
        var anim = z.GetComponent<Animator>();
        if (anim != null)
        {
            // 0：layer 0；"Base Layer" 預設狀態改成你走路/出現那個 State 名稱
            anim.Play(0, 0, 0f);
        }
    }
}

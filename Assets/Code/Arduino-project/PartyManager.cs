using UnityEngine;
using TMPro; // 使用 TextMeshPro 必備

public class PartyManager : MonoBehaviour
{
    [Header("角色物件")]
    public GameObject partyYellow; // 拖入甲方黃
    public GameObject partyGreen;  // 拖入甲方綠
    public GameObject partyGrey;   // 拖入甲方灰

    [Header("UI 顯示")]
    public TextMeshProUGUI textTMP; // 拖入 Text-square 下的 Text (TMP)

    void Start()
    {
        RandomizeClient();
    }

    public void RandomizeClient()
    {
        // 1. 先將所有角色隱藏
        partyYellow.SetActive(false);
        partyGreen.SetActive(false);
        partyGrey.SetActive(false);

        // 2. 隨機生成 0~2 的整數
        int randomIndex = Random.Range(0, 3);

        // 3. 根據隨機值顯示角色與英文對話
        switch (randomIndex)
        {
            case 0:
                partyYellow.SetActive(true);
                textTMP.text = "Client Yellow: I need a bright and vibrant shade of black!";
                break;
            case 1:
                partyGreen.SetActive(true);
                textTMP.text = "Client Green: Can you make the logo smaller but also much bigger?";
                break;
            case 2:
                partyGrey.SetActive(true);
                textTMP.text = "Client Grey: It doesn't feel right... let's try version 10.";
                break;
        }
    }
}
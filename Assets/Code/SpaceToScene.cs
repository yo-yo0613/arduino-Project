using UnityEngine;
using UnityEngine.SceneManagement;

public class SpaceToScene : MonoBehaviour
{
    [Tooltip("按下空白鍵後切換的場景名稱")]
    public string targetSceneName = "SampleScene";

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}

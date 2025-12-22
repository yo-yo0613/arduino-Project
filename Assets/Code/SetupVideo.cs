using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class SetupVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public int width = 1920;
    public int height = 1080;

    void Start()
    {
        if (videoPlayer == null || rawImage == null) return;

        RenderTexture rt = new RenderTexture(width, height, 0);
        videoPlayer.targetTexture = rt;
        rawImage.texture = rt;

        videoPlayer.Play();
    }
}

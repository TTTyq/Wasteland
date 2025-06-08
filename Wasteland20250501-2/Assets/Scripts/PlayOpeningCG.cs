using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class PlayOpeningCG : MonoBehaviour
{
    public VideoClip cg1;
    public VideoClip cg2;
    public GameObject videoScreen; // 用于显示视频的RawImage或MeshRenderer
    public string nextSceneName = "Scene Paint"; // CG播放完毕后要切换的场景名

    private VideoPlayer videoPlayer;
    private int cgIndex = 0;

    void Start()
    {
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = new RenderTexture(1920, 1080, 0);
        // 绑定到RawImage或MeshRenderer
        var renderer = videoScreen.GetComponent<Renderer>();
        if (renderer) renderer.material.mainTexture = videoPlayer.targetTexture;
        var rawImage = videoScreen.GetComponent<UnityEngine.UI.RawImage>();
        if (rawImage) rawImage.texture = videoPlayer.targetTexture;

        videoPlayer.loopPointReached += OnVideoEnd;
        PlayNextCG();
    }

    void PlayNextCG()
    {
        if (cgIndex == 0 && cg1 != null)
        {
            videoPlayer.clip = cg1;
            videoPlayer.Play();
            cgIndex++;
        }
        else if (cgIndex == 1 && cg2 != null)
        {
            videoPlayer.clip = cg2;
            videoPlayer.Play();
            cgIndex++;
        }
        else
        {
            // 跳转前彻底销毁VideoPlayer和RawImage，避免新场景发黑
            if (videoPlayer != null)
            {
                if (videoPlayer.targetTexture != null)
                {
                    videoPlayer.targetTexture.Release();
                    Destroy(videoPlayer.targetTexture);
                }
                Destroy(videoPlayer); // 彻底销毁VideoPlayer组件
            }
            if (videoScreen != null) Destroy(videoScreen);

            // 初始化VR场景
            if (VRSceneManager.Instance != null)
            {
                VRSceneManager.Instance.InitializeVRScene();
            }

            // 播放完CG后切换到指定场景
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        PlayNextCG();
    }
} 
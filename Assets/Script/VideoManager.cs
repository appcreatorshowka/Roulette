
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;

public class VideoManager : MonoBehaviour
{
    public static VideoManager Instance;

    [Header("Video Player")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoImage;

    [Header("Video Clips")]
    [SerializeField] private VideoClip angelVideo;
    [SerializeField] private VideoClip devilVideo;
    [SerializeField] private VideoClip angelDevilVideo;

    [Header("Probability (%)")]
    [Range(0, 100)]
    [SerializeField] private float angelProbability = 15f;

    [Range(0, 100)]
    [SerializeField] private float devilProbability = 15f;

    [Range(0, 100)]
    [SerializeField] private float angelDevilProbability = 10f;

    [Header("Stop Number")]
    [Range(1, 6)]
    [SerializeField] private int angelStopNumber = 2;

    [Range(1, 6)]
    [SerializeField] private int devilStopNumber = 6;

    [Range(1, 6)]
    [SerializeField] private int angelDevilStopNumber = 1;

    // キャラクターの種類
    public enum CharacterType
    {
        Angel,
        Devil
    }

    // 動画終了後に通知する処理
    private Action<int> onVideoFinished;

    // 再生する動画に対応する停止番号
    private int selectedStopNumber;

    private void Awake()
    {
        Debug.Log("★★★ VideoManager Awake実行！");

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (videoPlayer == null || videoImage == null)
        {
            Debug.LogError("VideoManagerの参照が設定されていません。");
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;

        videoImage.texture = videoPlayer.targetTexture;
        videoImage.gameObject.SetActive(false);

        videoPlayer.loopPointReached += OnVideoFinished;
    }

    // 確率判定と動画再生
    // 戻り値 true = 動画当選、false = 動画なし
    public bool TryPlayCutin(
        CharacterType character,
        Action<int> finishedCallback)
    {
        Debug.Log("★動画抽選開始：" + character);

        float randomValue = UnityEngine.Random.Range(0f, 100f);

        VideoClip selectedVideo = null;

        if (character == CharacterType.Angel)
        {
            if (randomValue < angelProbability)
            {
                selectedVideo = angelVideo;
                selectedStopNumber = angelStopNumber;
            }
            else if (randomValue <
                     angelProbability + angelDevilProbability)
            {
                selectedVideo = angelDevilVideo;
                selectedStopNumber = angelDevilStopNumber;
            }
        }
        else if (character == CharacterType.Devil)
        {
            if (randomValue < devilProbability)
            {
                selectedVideo = devilVideo;
                selectedStopNumber = devilStopNumber;
            }
            else if (randomValue <
                     devilProbability + angelDevilProbability)
            {
                selectedVideo = angelDevilVideo;
                selectedStopNumber = angelDevilStopNumber;
            }
        }

        // 動画が選ばれなかった場合
        if (selectedVideo == null)
        {
            Debug.Log("★動画なし：通常動作");
            return false;
        }

        // 終了後に実行する処理を保存
        onVideoFinished = finishedCallback;

        // 動画再生
        PlayVideo(selectedVideo);

        return true;
    }

    // 動画再生
    private void PlayVideo(VideoClip clip)
    {
        Debug.Log("★動画再生準備：" + clip.name);

        videoPlayer.Stop();

        videoPlayer.clip = clip;

        videoImage.gameObject.SetActive(true);

        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.prepareCompleted += OnVideoPrepared;

        videoPlayer.Prepare();
    }

    // 動画準備完了
    private void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("★動画準備完了！再生開始");

        videoPlayer.prepareCompleted -= OnVideoPrepared;

        videoPlayer.Play();
    }

    // 動画終了時
    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("★動画再生終了！");

        videoImage.gameObject.SetActive(false);

        videoPlayer.Stop();

        // 停止番号を渡す
        onVideoFinished?.Invoke(selectedStopNumber);

        // コールバックをクリア
        onVideoFinished = null;
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}

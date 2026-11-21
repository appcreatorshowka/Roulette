
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;

public class VideoManager : MonoBehaviour
{
    public static VideoManager Instance;

    [Header("Video Player")] // 動画再生用のVideoPlayerコンポーネント
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoImage;

    [Header("Video Clips")] // 再生する動画クリップ
    [SerializeField] private VideoClip angelVideo;
    [SerializeField] private VideoClip devilVideo;
    [SerializeField] private VideoClip angelDevilVideo;

    [Header("Probability (%)")] // 動画再生の確率（％）
    [Range(0, 100)]
    [SerializeField] private float angelProbability = 15f;

    [Range(0, 100)]
    [SerializeField] private float devilProbability = 15f;

    [Range(0, 100)]
    [SerializeField] private float angelDevilProbability = 10f;

    [Header("Stop Number")] // 動画再生後の停止番号
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

    // ★NEW：動画終了後に通知する処理
    private Action<int> onVideoFinished; // 動画終了後に呼び出すコールバック（停止番号を引数として渡す）

    private int selectedStopNumber; // 再生する動画に対応する停止番号

    private void Awake() // VideoManagerのインスタンスを初期化
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

        videoPlayer.playOnAwake = false; // VideoPlayerがAwake時に自動再生しないように設定
        videoPlayer.isLooping = false; // VideoPlayerがループ再生しないように設定

        videoImage.texture = videoPlayer.targetTexture; // RawImageにVideoPlayerのターゲットテクスチャを設定
        videoImage.gameObject.SetActive(false); // RawImageを非表示に設定

        videoPlayer.loopPointReached += OnVideoFinished; // 動画再生終了時にOnVideoFinishedメソッドを呼び出すイベントを登録
    }

    // ★変更：確率判定と動画再生
    // 戻り値 true = 動画当選、false = 動画なし
    public bool TryPlayCutin(
        CharacterType character,
        Action<int> finishedCallback)
    {
        Debug.Log("★動画抽選開始：" + character);

        float randomValue = UnityEngine.Random.Range(0f, 100f); // 0から100の範囲でランダムな値を生成

        VideoClip selectedVideo = null; // 再生する動画クリップを格納する変数

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

        if (selectedVideo == null) // 動画が選ばれなかった場合
        {
            return false; // 動画が選ばれなかった場合、falseを返す
        }

        // ★NEW：終了後に実行する処理を保存
        onVideoFinished = finishedCallback; // 動画終了後に呼び出すコールバックを保存

        PlayVideo(selectedVideo); // 動画を再生

        return true; // 動画が再生されたことを示す
    }

    // 動画再生
    private void PlayVideo(VideoClip clip) // 指定された動画クリップを再生するメソッド
    {
        Debug.Log("★動画再生準備：" + clip.name);

        videoPlayer.Stop(); // 既存の動画再生を停止

        videoPlayer.clip = clip; // VideoPlayerに指定された動画クリップを設定

        videoImage.gameObject.SetActive(true); // RawImageを表示して動画を再生する準備

        // ★変更：イベント登録を先に行う
        videoPlayer.prepareCompleted -= OnVideoPrepared; // 既存のイベント登録を解除
        videoPlayer.prepareCompleted += OnVideoPrepared; // 動画準備完了時にOnVideoPreparedメソッドを呼び出すイベントを登録

        videoPlayer.Prepare(); // 動画の準備を開始
    }

    // 動画準備完了
    private void OnVideoPrepared(VideoPlayer vp) // 動画の準備が完了したときに呼び出されるメソッド
    {
        Debug.Log("★動画準備完了！再生開始");

        videoPlayer.prepareCompleted -= OnVideoPrepared; // イベント登録を解除して重複呼び出しを防止

        videoPlayer.Play(); // 動画の再生を開始
    }

    // ★変更：動画終了時にRouletteControllerへ通知
    private void OnVideoFinished(VideoPlayer vp) // 動画の再生が終了したときに呼び出されるメソッド
    {
        videoImage.gameObject.SetActive(false); // RawImageを非表示にして動画再生を終了

        videoPlayer.Stop(); //  

        // ★NEW：停止番号を渡す
        onVideoFinished?.Invoke(selectedStopNumber); // 動画終了後に登録されたコールバックを呼び出し、停止番号を渡す

        onVideoFinished = null; // コールバックをクリアして再利用可能にする
    }

    private void OnDestroy() // VideoManagerが破棄されるときに呼び出されるメソッド
    {
        if (videoPlayer != null) //     VideoPlayerが存在する場合、イベント登録を解除してメモリリークを防止
        {
            videoPlayer.loopPointReached -= OnVideoFinished; // 動画再生終了時のイベント登録を解除
            videoPlayer.prepareCompleted -= OnVideoPrepared; // 動画準備完了時のイベント登録を解除
        }
    }
}

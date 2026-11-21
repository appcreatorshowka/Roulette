
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RouletteController : MonoBehaviour
{
    // ★変更：回転状態を管理
    private float rotSpeed = 0f;

    private bool isSpinning = false;
    private bool isWaitingForVideo = false;
    private bool isVideoPlaying = false;
    private bool isTargetStopping = false;

    // ★NEW：動画抽選までの時間
    [Header("Video Timing")]
    [SerializeField] private float videoStartDelay = 2f;

    // ★NEW：停止演出の設定
    [Header("Stop Settings")]
    [SerializeField] private float stopDuration = 2.5f;
    [SerializeField] private int extraTurns = 2;

    // ★NEW：角度調整
    // スクリーンショットの初期配置を基準にした値
    [Header("Wheel Angle Settings")]
    [SerializeField] private float no1CenterAngle = 60f;
    [SerializeField] private float pointerAngle = -90f;

    [SerializeField] GameObject plainEffect;
    [SerializeField] GameObject iceEffect;

    [SerializeField] AngelInfinityMove angelController;

    private float spinElapsed = 0f;

    // Start
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    // Update
    void Update()
    {
        // ★変更：回転中は新しいクリックを受け付けない
        if (!isSpinning)
        {
            CheckClick();
        }

        if (!isSpinning || isTargetStopping)
        {
            return;
        }

        // ルーレットを回転
        transform.Rotate(0, 0, -rotSpeed);

        // ★NEW：回転開始から2秒を計測
        if (isWaitingForVideo && !isVideoPlaying)
        {
            spinElapsed += Time.deltaTime;

            if (spinElapsed >= videoStartDelay)
            {
                StartVideoCheck();
            }
        }

        // ★変更：動画抽選中・動画再生中は減速しない
        if (!isWaitingForVideo && !isVideoPlaying)
        {
            // 通常の自然減速
            rotSpeed *= 0.96f;

            if (rotSpeed < 0.05f)
            {
                rotSpeed = 0f;
                isSpinning = false;
            }
        }
    }

    // ★NEW：クリック処理
    private void CheckClick()
    {
        bool clicked =
            (Mouse.current != null &&
             Mouse.current.leftButton.wasPressedThisFrame)
            ||
            (Touchscreen.current != null &&
             Touchscreen.current.primaryTouch.press.wasPressedThisFrame);

        if (!clicked)
        {
            return;
        }

        Vector2 pointerPosition;

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            pointerPosition = Mouse.current.position.ReadValue();
        }
        else
        {
            pointerPosition =
                Touchscreen.current.primaryTouch.position.ReadValue();
        }

        if (IsClickingInputUI(pointerPosition))
        {
            return;
        }

        // Angel / Devil判定
        if (angelController.IsAngel)
        {
            Instantiate(plainEffect, transform.position, Quaternion.identity);

            rotSpeed = 120f;
        }
        else
        {
            Instantiate(iceEffect, transform.position, Quaternion.identity);

            rotSpeed = 1200f;
        }

        // ★NEW：回転開始
        isSpinning = true;
        isWaitingForVideo = true;
        isVideoPlaying = false;
        isTargetStopping = false;

        spinElapsed = 0f;
    }

    // ★NEW：2秒後の動画抽選
    private void StartVideoCheck()
    {
        isWaitingForVideo = false;

        if (VideoManager.Instance == null)
        {
            Debug.LogError("VideoManagerが見つかりません。");
            return;
        }

        VideoManager.CharacterType character =
            angelController.IsAngel
            ? VideoManager.CharacterType.Angel
            : VideoManager.CharacterType.Devil;

        // 動画が当選した場合
        bool videoSelected =
            VideoManager.Instance.TryPlayCutin(
                character,
                OnVideoFinished
            );

        if (videoSelected)
        {
            isVideoPlaying = true;
        }
        // 動画が外れた場合は通常の自然減速に移行
    }

    // ★NEW：動画終了後に呼ばれる
    private void OnVideoFinished(int targetNumber)
    {
        isVideoPlaying = false;

        Debug.Log("動画終了。停止番号：No." + targetNumber);

        // 指定番号への停止処理
        StartCoroutine(StopAtNumber(targetNumber));
    }

    // ★NEW：指定番号に向けて減速
    private IEnumerator StopAtNumber(int number)
    {
        isTargetStopping = true;

        // No.1～No.6の番号チェック
        number = Mathf.Clamp(number, 1, 6);

        float startZ = transform.eulerAngles.z;

        // No.1を基準に60度ずつ反時計回り
        float sectorAngle =
            no1CenterAngle + (number - 1) * 60f;

        // Pointerと指定番号の角度差
        float desiredRotation = pointerAngle - sectorAngle;

        // ★NEW：時計回りに追加回転して指定位置へ
        float deltaAngle =
            -Mathf.Repeat(startZ - desiredRotation, 360f)
            - extraTurns * 360f;

        float elapsed = 0f;

        while (elapsed < stopDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / stopDuration);

            // SmoothStepで滑らかに減速
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            float currentZ = startZ + deltaAngle * smoothT;

            transform.rotation =
                Quaternion.Euler(0f, 0f, currentZ);

            yield return null;
        }

        // ★NEW：最後の角度を正確に設定
        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                startZ + deltaAngle
            );

        rotSpeed = 0f;

        isSpinning = false;
        isTargetStopping = false;

        Debug.Log("ルーレット停止：No." + number);
    }

    // ★変更：UIクリック判定
    bool IsClickingInputUI(Vector2 position)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        PointerEventData eventData =
            new PointerEventData(EventSystem.current);

        eventData.position = position;

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponentInParent<Dropdown>() != null ||
                result.gameObject.GetComponentInParent<TMP_Dropdown>() != null ||
                result.gameObject.GetComponentInParent<TMP_InputField>() != null ||
                result.gameObject.GetComponentInParent<InputField>() != null)
            {
                return true;
            }
        }

        return false;
    }
}

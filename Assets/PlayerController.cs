using System;
using UnityEngine;
using DG.Tweening;
using static UnityEngine.Rendering.DebugUI;

enum PlayerState
{
    normaljump,
    highjump,
    superjump
}

public class PlayerController : MonoBehaviour
{
    [Header("ビート設定")]
    public float bpm;
    private float beatInterval;
    private float timer;

    [Header("ジャンプ設定")]
    private float jumpHeight = 2f;
    private float jumpDuration = 0.3f;
    private float upTimeRatio = 0.3f;    // 上昇時間の割合
    private float stayTimeRatio = 0.4f;  // 滞空時間の割合
    private float downTimeRatio = 0.3f;  // 下降時間の割合
    bool isJumping = false;

    PlayerState state = PlayerState.normaljump;
    PlayerState _state = PlayerState.normaljump;
    private Color flashColor = Color.red;

    [Header("移動設定")]
    public float moveSpeed = 2.0f;

    private Color originalColor;
    private Renderer rend;
    public float flashDuration = 0.1f;  // 色を維持する時間
    public Color beatColor = Color.red; // 一瞬変える色


    void Start()
    {
        beatInterval = 60f / bpm;
        timer = 0f;
        state = PlayerState.normaljump;
        jumpDuration = beatInterval * 2f;  // 2拍分

        upTimeRatio = 0.4f;
        stayTimeRatio = 0.2f;
        downTimeRatio = 0.4f;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f && !isJumping)
        {
            state = _state;
            Jump();
            timer += beatInterval;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.position += Vector3.right * Time.deltaTime * moveSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position += Vector3.left * Time.deltaTime * moveSpeed;
        }

        if (Input.GetKeyDown(KeyCode.A)) { _state = PlayerState.normaljump; Debug.Log("normal"); }
        if (Input.GetKeyDown(KeyCode.S)) { _state = PlayerState.highjump; Debug.Log("highjump"); }
        if (Input.GetKeyDown(KeyCode.D)) { _state = PlayerState.superjump; Debug.Log("superjump"); }

        // 状態ごとの設定
        switch (state)
        {
            case PlayerState.normaljump:
                jumpHeight = 2.0f;
                //jumpDuration = beatInterval * 2f;  // 2拍分
                break;
            case PlayerState.highjump:
                jumpHeight = 4.0f;
                //jumpDuration = beatInterval * 2f;  // 2拍分
                break;
            case PlayerState.superjump:
                jumpHeight = 7.0f;
                //jumpDuration = beatInterval * 2f;  // 2拍分
                break;
        }
    }

    void Jump()
    {
        isJumping = true;
        float baseY = transform.position.y;
        float targetY = baseY + jumpHeight;

        float upTime = jumpDuration * upTimeRatio;
        float stayTime = jumpDuration * stayTimeRatio;
        float downTime = jumpDuration * downTimeRatio;

        Sequence jumpSeq = DOTween.Sequence();
        jumpSeq.Append(transform.DOMoveY(targetY, upTime).SetEase(Ease.OutExpo));
        jumpSeq.AppendInterval(stayTime);
        jumpSeq.Append(transform.DOMoveY(baseY, downTime).SetEase(Ease.InExpo));
        jumpSeq.OnComplete(() => isJumping = false);
        jumpSeq.Play();


    }

    void FlashColor()
    {
        rend.material.DOColor(beatColor, flashDuration / 2f).OnComplete(() =>
        {
            rend.material.DOColor(originalColor, flashDuration / 2f);
        });

    }
}
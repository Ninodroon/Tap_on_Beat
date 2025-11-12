using CriWare;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//マーカーベースのジャンプにする。座標移動の終わりと初めを一瞬同じにして、ジャンプがかくつかないようにする。
//→spaceの入力にも少し幅ができるし、見た目も改善されるし、ある程度のずれをごまかせる。一旦、補間処理はなくす。
//リスポーン時に着地する目標タイミングまでの時間を計算して着地する。
//つねにUpdateでspaceの入力を受け付ける。UpgradeJumpか、PerformBackBeatする。

public class DededeJump3 : MonoBehaviour
{
    public enum JumpType { Normal, High, Super }
    public enum JudgeType { GREAT, GOOD, MISS }

    [Header("ジャンプ高さ")]
    public float normalHeight = 2f;
    public float highHeight = 4f;
    public float superHeight = 7f;

    [Header("タイミング設定")]
    public float bpm = 140f;
    [Range(0.05f, 0.45f)]
    public float jumpSpeedRatio = 0.25f;
    public float minCutWindow = 0.12f;
    public float minMarkerGap = 0.06f;
    public float greatWindow = 0.05f;
    public float goodWindow = 0.1f;

    [Header("移動")]
    public float moveSpeed = 2f;
    public CriAtomSource music;

    private JumpType currentType = JumpType.Normal;
    private float groundY;
    private float beatInterval;
    private float lastJumpTime = -999f;
    private float landingTime;
    private float backBeatTime;
    private bool isJumping;
    private bool canBackBeat;
    private bool pendingJump;
    private bool spacePressed;
    private Sequence seq;
    public int score;

    void OnEnable()
    {
        AdxMarkerBroadcaster.OnMarkerReceived += OnMarker;
    }

    void OnDisable()
    {
        AdxMarkerBroadcaster.OnMarkerReceived -= OnMarker;
        seq?.Kill();
    }

    void Start()
    {
        groundY = transform.position.y;
        beatInterval = 60f / bpm;
        music?.Play();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (canBackBeat)
            {
                PerformBackBeat();
            }
            else
            {
                spacePressed = true;
            }
        }

        
        if (!isJumping && pendingJump)
        {
            pendingJump = false;
            Jump();
        }
    }

    void OnMarker(string tag)
    {
        if (tag == "JUMP")
        {
            if (Time.time - lastJumpTime < minMarkerGap) return;
            lastJumpTime = Time.time;

            if (isJumping)
            {
                float remain = landingTime - Time.time;
                if (remain > minCutWindow)
                {
                    seq?.Kill();
                    var pos = transform.position;
                    pos.y = groundY;
                    transform.position = pos;
                    isJumping = false;
                    Jump();
                }
                else
                {
                    pendingJump = true;
                }
            }
            else
            {
                Jump();
            }
        }
        else if (tag == "BACK")
        {
            backBeatTime = Time.time;
        }
    }

    void Jump()
    {
        isJumping = true;

        float total = beatInterval * 2f;
        float move = total * jumpSpeedRatio;
        float stay = total - move * 2f;
        float targetY = groundY + GetHeight();
        landingTime = Time.time + total;

        seq?.Kill();
        seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(targetY, move).SetEase(Ease.OutQuad));
        seq.AppendCallback(() => canBackBeat = true);
        seq.AppendInterval(stay);
        seq.AppendCallback(() => canBackBeat = false);
        seq.Append(transform.DOMoveY(groundY, move).SetEase(Ease.InQuad));
        seq.OnComplete(() =>
        {
            float error = Time.time - landingTime;
            Debug.Log($"着地誤差: {error:F3}秒");
            JudgeType judge = Judge(error);

            if (spacePressed && judge != JudgeType.MISS)
            {
                UpgradeJump();
                Debug.Log($"着地成功 {judge} → {currentType}");
            }
            else if (!spacePressed || judge == JudgeType.MISS)
            {
                if (currentType != JumpType.Normal)
                {
                    currentType = JumpType.Normal;
                    Debug.Log("ミス → Normal");
                }
            }

            spacePressed = false;
            isJumping = false;

            if (pendingJump)
            {
                pendingJump = false;
                Jump();
            }
        });
    }

    void PerformBackBeat()
    {
        float error = Time.time - backBeatTime;
        JudgeType judge = Judge(error);

        if (judge != JudgeType.MISS)
        {
            int bonus = judge == JudgeType.GREAT ? 100 : 50;
            score += bonus;
            Debug.Log($"裏打ち {judge} +{bonus}");
        }

        canBackBeat = false;
    }

    void UpgradeJump()
    {
        if (currentType == JumpType.Normal) currentType = JumpType.High;
        else if (currentType == JumpType.High) currentType = JumpType.Super;
    }

    JudgeType Judge(float error)
    {
        float abs = Mathf.Abs(error);
        if (abs <= greatWindow) return JudgeType.GREAT;
        if (abs <= goodWindow) return JudgeType.GOOD;
        return JudgeType.MISS;
    }

    float GetHeight()
    {
        switch (currentType)
        {
            case JumpType.High: return highHeight;
            case JumpType.Super: return superHeight;
            default: return normalHeight;
        }
    }
}
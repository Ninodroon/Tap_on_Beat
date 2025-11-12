using UnityEngine;
using DG.Tweening;
using CriWare;
using System;

public enum JumpType
{
    Normal,
    High,
    Super,
    Ottoto
}

public enum JudgeType
{
    GREAT,
    GOOD,
    MISS
}

public class DededeJump : MonoBehaviour
{
    [Header("ビート設定")]
    public float bpm = 0f;
    private float beatInterval;

    [Header("ジャンプ設定")]
    public float normalJumpHeight = 2f;
    public float highJumpHeight = 4f;
    public float superJumpHeight = 7f;
    public float jumpSpeedRatio = 0.25f;
    public float jumpHeight = 2.0f;

    [Header("ジャンプタイプ")]
    public JumpType currentJumpType = JumpType.Normal;
    private bool isJumping = false;
    private bool spacePressed = false;
    private float groundY = 0;
    private float landingTime;
    private bool canBackBeat = false;
    private float backBeatWindow;

    [Header("移動")]
    public float moveSpeed = 2.0f;
    public int score = 0;

    [Header("サウンド設定")]
    public AudioSource seBackBeat;

    public CriAtomSource music;
    public static event Action<CriAtomExBeatSync.Info> OnBeat;

    private RhythmJudge rhythmJudge = new RhythmJudge();

    [Header("スタート台ジャンプ設定")]
    public Transform firstDrum;
    public float firstJumpHeight = 2.5f;
    private Vector3[] jumpPath; // Gizmos用
    private Vector3 firstDrumLandingPoint;
    private bool hasJumpedFromStart = false;

    void Start()
    {
        beatInterval = 60f / bpm;
        groundY = 1.0f;

        if (firstDrum != null)
        {
            firstDrumLandingPoint = firstDrum.position;
            GenerateJumpPath(transform.position, firstDrumLandingPoint, firstJumpHeight);
            JumpToFirstDrum();
        }
    }

    void Update()
    {
          if (Input.GetKey(KeyCode.RightArrow))
            transform.position += Vector3.right * Time.deltaTime * moveSpeed;
        else if (Input.GetKey(KeyCode.LeftArrow))
            transform.position += Vector3.left * Time.deltaTime * moveSpeed;

        if (Input.GetKeyDown(KeyCode.A)) currentJumpType = JumpType.Normal;
        if (Input.GetKeyDown(KeyCode.S)) currentJumpType = JumpType.High;
        if (Input.GetKeyDown(KeyCode.D)) currentJumpType = JumpType.Super;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isJumping && canBackBeat)
            {
                PerformBackBeat();
            }
            else
            {
                spacePressed = true;
            }
        }

        if (!isJumping && hasJumpedFromStart)
        {
            JumpByTween();
        }
    }

    float GetJumpHeight()
    {
        switch (currentJumpType)
        {
            case JumpType.Normal: return normalJumpHeight;
            case JumpType.High: return highJumpHeight;
            case JumpType.Super: return superJumpHeight;
            case JumpType.Ottoto: return normalJumpHeight;
            default: return normalJumpHeight;
        }
    }

    void JumpByTween()
    {
        isJumping = true;
        float baseY = groundY;
        float targetY = baseY + GetJumpHeight();
        float totalDuration = beatInterval * 2f;
        float moveTime = totalDuration * jumpSpeedRatio;
        float floatTime = totalDuration - moveTime * 2f;
        landingTime = Time.time + totalDuration;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(targetY, moveTime).SetEase(Ease.OutQuad));
        seq.AppendCallback(() =>
        {
            if (currentJumpType != JumpType.Ottoto)
            {
                canBackBeat = true;
                backBeatWindow = Time.time + floatTime;
            }
        });
        seq.AppendInterval(floatTime);
        seq.AppendCallback(() => canBackBeat = false);
        seq.Append(transform.DOMoveY(baseY, moveTime).SetEase(Ease.InQuad));
        seq.OnComplete(() =>
        {
            float timingError = Time.time - landingTime;
            JudgeType landingJudge = rhythmJudge.Judge(timingError);

            if (spacePressed && landingJudge != JudgeType.MISS)
            {
                UpgradeJumpType();
                Debug.Log($"着地成功！ {landingJudge} - ジャンプタイプ: {currentJumpType}");
            }
            else if (!spacePressed || landingJudge == JudgeType.MISS)
            {
                if (currentJumpType == JumpType.High || currentJumpType == JumpType.Super)
                {
                    currentJumpType = JumpType.Normal;
                    Debug.Log("ミス！Normalに戻る");
                }
            }

            spacePressed = false;
            isJumping = false;
        });

        seq.Play();
    }

    void UpgradeJumpType()
    {
        switch (currentJumpType)
        {
            case JumpType.Normal:
                currentJumpType = JumpType.High;
                break;
            case JumpType.High:
                currentJumpType = JumpType.Super;
                break;
            case JumpType.Super:
                break;
        }
    }

    void PerformBackBeat()
    {
        if (currentJumpType == JumpType.Ottoto) return;

        float currentTime = Time.time;
        float backBeatCenter = backBeatWindow - (backBeatWindow - (landingTime - beatInterval)) * 0.5f;
        float timingError = currentTime - backBeatCenter;
        JudgeType backBeatJudge = rhythmJudge.Judge(timingError);

        if (backBeatJudge != JudgeType.MISS)
        {
            int bonusScore = backBeatJudge == JudgeType.GREAT ? 100 : 50;
            score += bonusScore;
            Debug.Log($"裏打ち成功！ {backBeatJudge} +{bonusScore}点");
        }

        seBackBeat?.Play();
        canBackBeat = false;
    }

    public class RhythmJudge
    {
        public float greatWindow = 0.05f;
        public float goodWindow = 0.1f;

        public JudgeType Judge(float timingError)
        {
            float absError = Mathf.Abs(timingError);
            if (absError <= greatWindow) return JudgeType.GREAT;
            if (absError <= goodWindow) return JudgeType.GOOD;
            return JudgeType.MISS;
        }
    }

    // ▼▼▼▼▼ 追加：スタート台からジャンプ ▼▼▼▼▼
    void JumpToFirstDrum()
    {
        float beatTime = 60f / bpm;

        Vector3 start = transform.position;
        Vector3 end = firstDrumLandingPoint;
        Vector3 peak = new Vector3(
            (start.x + end.x) / 2f,
            Mathf.Max(start.y, end.y) + firstJumpHeight,
            (start.z + end.z) / 2f
        );

        Sequence jumpSeq = DOTween.Sequence();
        jumpSeq.Append(transform.DOMove(peak, beatTime / 2f).SetEase(Ease.OutQuad));
        jumpSeq.Append(transform.DOMove(end, beatTime / 2f).SetEase(Ease.InQuad));
        jumpSeq.Play();
        hasJumpedFromStart = true;
    }

    void GenerateJumpPath(Vector3 start, Vector3 end, float height)
    {
        int pointCount = 20;
        jumpPath = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            float t = i / (float)(pointCount - 1);
            Vector3 point = Vector3.Lerp(start, end, t);
            float yOffset = Mathf.Sin(t * Mathf.PI) * height;
            point.y = Mathf.Lerp(start.y, end.y, t);// + yOffset;
            jumpPath[i] = point;
        }
    }

    void OnDrawGizmos()
    {
        if (jumpPath == null || jumpPath.Length < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < jumpPath.Length - 1; i++)
        {
            Gizmos.DrawLine(jumpPath[i], jumpPath[i + 1]);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(firstDrumLandingPoint, 0.1f);
    }
}

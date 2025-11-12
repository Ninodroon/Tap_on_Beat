using CriWare;
using DG.Tweening;
using UnityEngine;

//マーカー基準でジャンプ、予測位置あり

public class Player : MonoBehaviour
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
    [Header("ジャンプ中に次ジャンプを割り込む最小残り時間")]
    public float minCutWindow = 0.12f;//0.12f以上遅れたらジャンプをリセット
    [Header("マーカー間の最小許容間隔")]
    public float minMarkerGap = 0.06f;

    public float greatWindow = 0.05f;
    public float goodWindow = 0.1f;

    [Header("移動")]
    public float moveSpeed = 2f;
    public CriAtomSource music;
    private JumpType currentType = JumpType.Normal;

    private float groundY;
    private float beatInterval;
    private float lastJumpTime = -999f;//最後にJUMPマーカーを処理した時刻
    private float landingTime;
    private float nextJumpMarkerTime;
    private float backBeatTime;
    private bool isJumping;
    private bool canBackBeat;
    private bool pendingJump;//次のジャンプを予約。
    private bool spacePressed;
    public int score;
    private bool resetRequested;

    private Sequence seq;

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
        //追記　後で消す
        if (resetRequested)
        {
            resetRequested = false;
            seq?.Kill();
            transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
            isJumping = false;
        }

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
            JumpWithDuration(nextJumpMarkerTime - Time.time);
        }
    }

    void OnMarker(string tag)
    {
        if (tag == "JUMP")
        {
            if (Time.time - lastJumpTime < minMarkerGap) return;//同一フレームで2回JUMPマーカーが来てもminMarkerGap0.06秒以内なら無視
            lastJumpTime = Time.time;

            nextJumpMarkerTime = Time.time + beatInterval * 2f;// 2拍後の時刻を記録

            if (isJumping)
            {
                float remain = landingTime - Time.time;//現在のジャンプの着地までの残り時間
                if (remain > minCutWindow)//まだ余裕があるなら途中で切り替え
                {
                    //seq?.Kill();
                    //var pos = transform.position;
                    //pos.y = groundY;
                    //transform.position = pos;
                    //isJumping = false;
                    //JumpWithDuration(nextJumpMarkerTime - Time.time);

                    //後で消す
                    Debug.Log("ジャンプをリセット予約しました。");
                    resetRequested = true; // 即リセットせず、次フレームに処理
                    pendingJump = true;    // 次ジャンプ予約
                }
                else
                {
                    pendingJump = true;
                }
            }
            else
            {
                JumpWithDuration(beatInterval * 2f);
            }
        }
        else if (tag == "BACK")
        {
            backBeatTime = Time.time; // ★追加
        }
    }

    void JumpWithDuration(float duration)
    {
        isJumping = true;

        float move = duration * jumpSpeedRatio;
        float stay = duration - move * 2f;
        float targetY = groundY + GetHeight();
        landingTime = Time.time + duration;

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
            JudgeType judge = Judge(error);

            if (spacePressed && judge != JudgeType.MISS)
            {
                UpgradeJump();
                Debug.Log($"ジャンプタイプ {judge} → {currentType}");
            }
            else if (!spacePressed || judge == JudgeType.MISS)
            {
                if (currentType != JumpType.Normal)
                {
                    currentType = JumpType.Normal;
                    Debug.Log("ミス → Normal");
                }
            }

            isJumping = false;

            if (pendingJump)
            {
                pendingJump = false;
                JumpWithDuration(nextJumpMarkerTime - Time.time);
            }

            spacePressed = false; 
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
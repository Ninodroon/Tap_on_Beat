using CriWare;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using System.Diagnostics;


public class DededeJump2 : MonoBehaviour
{
    Stopwatch stopwatch = new Stopwatch();

    [Header("ビート設定")]
    public float bpm = 140f;

    [Header("ジャンプ挙動")]
    public float normalJumpHeight = 2f;
    public float highJumpHeight = 4f;
    public float superJumpHeight = 7f;
    [Range(0.05f, 0.45f)]
    public float jumpSpeedRatio = 0.24f;   // 上昇/下降に使う比率
    bool isCutting = false;//確認のため。
    private float lastLandingTime = -999f;   // 着地した瞬間の Time.time
    public float landingWindow = 0.1f; // この時間内ならアップグレード可能

    private bool isStartJump = false;
    public Transform FirstDrum;
    public Transform StartPos;

    [Header("裏打ち")]
    public AudioSource seBackBeat;
    public static DededeJump2 Instance { get; private set; }
    public CriAtomSource music;

    long markerDspTime;
    long jumpDspTime;

    // 内部
    private float beatInterval;//ビート間隔（秒）
    private float groundY;
    private bool isJumping = false;
    private bool canBackBeat = false;
    private float landingTime;
    private float lastJumpMarkerTime = -999f;
    private Sequence currentSeq;
    private enum JumpType { Normal, High, Super, Ottoto }
    private JumpType currentJumpType = JumpType.Normal;
    private bool spacePressed;
    private long spacePressedTime =0;
    private float backBeatTime;
    private float jumpTime;
    private int score;
    private long CRImusic_DspTime;

    public float greatWindow = 0.05f;
    public float goodWindow = 0.1f;

    public static event Action<string> OnMarkerReceived;

    public float moveSpeed = 2.0f;

    bool isMarker = false;
    private float MarkerDeltaTime = 0f;

    //OnSequencerCallbackはグローバルイベントなので登録、解除しないといけない
    private void OnEnable()
    {
        AdxMarkerBroadcaster.OnMarkerReceived += OnSequencerCallback;
    }
    private void OnDisable()
    {
        AdxMarkerBroadcaster.OnMarkerReceived -= OnSequencerCallback;
        if (currentSeq != null && currentSeq.IsActive()) currentSeq.Kill();
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //groundY = transform.position.y;
        groundY = FirstDrum.transform.position.y - 0.2f;
        beatInterval = (60f / bpm);//120bpmなら0.5mm秒
                                    UnityEngine.Debug.Log($"beatInterval : {beatInterval}");
        StartCoroutine(PlayMusicDelayed());
    }

    IEnumerator PlayMusicDelayed()
    {
        yield return new WaitForSeconds(1f);
        music.Play();
    }

    //20msごと
    void FixedUpdate()
    {
        //ジャンプ中かつ着地予定時刻を過ぎていたら、強制着地
        if (isMarker)
        {
            jumpDspTime = music.time;
            long delay = jumpDspTime - markerDspTime;
            //UnityEngine.Debug.Log($"マーカー：{markerDspTime},ジャンプ : {jumpDspTime},遅延: {delay} ",this);

            StartJump();
            isMarker = false;
            //CRImusic_DspTime = music.time;
            //UnityEngine.Debug.Log($"dedede ジャンプマーカー　：{CRImusic_DspTime}");
        }
    }

    //フレーム依存
    void Update()
    {
        if (isStartJump)
        {
            if (Input.GetKey(KeyCode.RightArrow))
                transform.position += Vector3.right * Time.deltaTime * moveSpeed;
            else if (Input.GetKey(KeyCode.LeftArrow))
                transform.position += Vector3.left * Time.deltaTime * moveSpeed;
        }

        //if (Input.GetKeyDown(KeyCode.A)) currentJumpType = JumpType.Normal;
        //if (Input.GetKeyDown(KeyCode.S)) currentJumpType = JumpType.High;
        //if (Input.GetKeyDown(KeyCode.D)) currentJumpType = JumpType.Super;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            spacePressed = true;
        }
        else {
            spacePressed = false;
        }

        if (canBackBeat && spacePressed)
        {
            PerformBackBeat();
        }
        else if(!canBackBeat)
        {
            if (spacePressed)
            {

                float timeSinceLanding = Time.time - lastLandingTime;
                if (timeSinceLanding <= landingWindow)
                {
                    UpgradeJumpType();
                }
            }
        }



    }

    //void Update()
    //{
    //    // 左右移動
    //    if (Input.GetKey(KeyCode.RightArrow))
    //        transform.position += Vector3.right * Time.deltaTime * moveSpeed;
    //    else if (Input.GetKey(KeyCode.LeftArrow))
    //        transform.position += Vector3.left * Time.deltaTime * moveSpeed;

    //    // ジャンプタイプ変更
    //    if (Input.GetKeyDown(KeyCode.A)) currentJumpType = JumpType.Normal;
    //    if (Input.GetKeyDown(KeyCode.S)) currentJumpType = JumpType.High;
    //    if (Input.GetKeyDown(KeyCode.D)) currentJumpType = JumpType.Super;

    //    // バックビート入力
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        if (canBackBeat)
    //        {
    //            PerformBackBeat();
    //        }
    //    }

    //    // ────────────────
    //    // マーカー受信 → 即ジャンプ
    //    // ────────────────
    //    if (isMarker)
    //    {
    //        jumpDspTime = music.time;
    //        long delay = jumpDspTime - markerDspTime;

    //        UnityEngine.Debug.Log(
    //            $"マーカー：{markerDspTime}, ジャンプ : {jumpDspTime}, 遅延: {delay} ,isCutting : {isCutting}"
    //        );

    //        StartJump();
    //        isMarker = false;
    //    }
    //}

    //マーカー拾われたときに呼ばれるコールバック
    private void OnSequencerCallback(string tag)
    {
        if (tag == "JUMP")
        {
            //UnityEngine.Debug.Log($"Delta: {stopwatch.Elapsed.TotalMilliseconds}");

            //CRImusic_DspTime = music.time;
            //UnityEngine.Debug.Log($"dedede ジャンプマーカー　：{CRImusic_DspTime}");

            //UnityEngine.Debug.Log($"Jump Marker : {Jump_DspTime},{stopwatch.Elapsed.TotalMilliseconds}");

            markerDspTime = music.time;

            stopwatch.Reset();
            stopwatch.Start();
            isMarker = true;
        }
        else if (tag == "BACK")
        {
            //backBeatTime = Time.time; //裏打ちが完璧な値
            long Back_DspTime = music.time;
           // UnityEngine.Debug.Log($"裏打ちマーカー　：{Back_DspTime}");

            isMarker = false;

        }
    }

    //ジャンプ----------------------------------------------------------------------

    void UpgradeJumpType()
    {
        switch (currentJumpType)
        {
            case JumpType.Normal: currentJumpType = JumpType.High; break;
            case JumpType.High: currentJumpType = JumpType.Super; break;
            case JumpType.Super: break;
        }
    }

    private void StartJump()
    {
        isJumping = true;

        float total = beatInterval * 2f - 0.02f;
        float move = 0.2f;//total * jumpSpeedRatio;
        float stay = total - move * 2;

        float targetY = groundY + GetJumpHeight();

        var seq = DOTween.Sequence();

        // ==========================================================
        // ★ 1回目だけ、スタート台 → FirstDrum に 移動
        // ==========================================================
        if (isStartJump == false)
        {
            isStartJump = true;   // 次回からは通常ジャンプ

            Vector3 start = transform.position;
            Vector3 end = FirstDrum.transform.position;
            Vector3 target = new Vector3(
           FirstDrum.position.x, // X = 中間
            StartPos.transform.position.y + GetJumpHeight(),               // Y = スタート台の高さ + ジャンプ量
            0f                                          // Z = 0 固定
             );
            //(StartPos.transform.position.x + FirstDrum.transform.position.x) * 0.5f

            seq.Append(StartPos.DOMove(target, move).SetEase(Ease.OutQuad))
              .AppendCallback(() => canBackBeat = true)
              .AppendInterval(stay)
              .AppendCallback(() => canBackBeat = false)
              .Append(transform.DOMove(FirstDrum.transform.position, move).SetEase(Ease.InQuad))
              .AppendCallback(() =>
              {
                  isJumping = false;
                  lastLandingTime = Time.time;
              });

            currentSeq = seq;
            seq.Play();
            return;
        }

        UnityEngine.Debug.Log($"ジャンプ開始: {beatInterval},targetY={targetY}, total={total}, move={move}, stay={stay}");

        seq.Append(transform.DOMoveY(targetY, move).SetEase(Ease.OutQuad))
       .AppendCallback(() => canBackBeat = true)
       .AppendInterval(stay)
       .AppendCallback(() => canBackBeat = false)
       .Append(transform.DOMoveY(groundY, move).SetEase(Ease.InQuad))
       .AppendCallback(() =>
       {
           isJumping = false;
           lastLandingTime = Time.time;

       });

       if (transform.position.y == groundY) { isCutting = true;/* isJumping = false; UnityEngine.Debug.Log("groundYと同じ");*/ } else { isCutting = false; }

        currentSeq = seq;
        seq.Play();

    }

    private float GetJumpHeight()
    {
        switch (currentJumpType)
        {
            case JumpType.Normal: return normalJumpHeight;
            case JumpType.High: return highJumpHeight;
            case JumpType.Super: return superJumpHeight;
            default: return normalJumpHeight;
        }
    }

    private void PerformBackBeat()
    {
        float error = Time.time - backBeatTime;//誤差
        JudgeType judge = Judge(error);

        if (judge != JudgeType.MISS)
        {
            int bonus = judge == JudgeType.GREAT ? 100 : 50;
            score += bonus;
            UnityEngine.Debug.Log($"裏打ち {judge} +{bonus}");
        }

        canBackBeat = false;
    }

    JudgeType Judge(float error)
    {
        float abs = Mathf.Abs(error);
        if (abs <= greatWindow) return JudgeType.GREAT;
        if (abs <= goodWindow) return JudgeType.GOOD;
        return JudgeType.MISS;
    }

}

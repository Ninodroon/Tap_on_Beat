using CriWare;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using System.Diagnostics;


//マーカーベース、リセットあり
//minCutWindowによって挙動が変わる。たまに強制リセットがかかるから、そのあたりいじったらいいかんじかも。
//常にspace入力の受付　PerformBackBeat UpgradeJump。
//座標移動の終わりと初めを一瞬同じにして、ジャンプがかくつかないようにする。
//→spaceの入力にも少し幅ができるし、見た目も改善されるし、ある程度のずれをごまかせる。一旦、補間処理はなくしたい。

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

    [Header("チェイン/カット設定")]

    [Header("裏打ちSE（任意）")]
    public AudioSource seBackBeat;
    public CriAtomSource music;

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
    private float backBeatTime;
    private float jumpTime;
    private int score;

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


    void Start()
    {
        groundY = transform.position.y;
        beatInterval = (60f / bpm);// * 0.888f;  //120bpm
        UnityEngine.Debug.Log($"beatInterval : {beatInterval}");
        music?.Play();
    }

    void FixedUpdate()
    {
        //ジャンプ中かつ着地予定時刻を過ぎていたら、強制着地
        if (isMarker)
        {
            StartJump();
            isMarker = false;
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
            if (canBackBeat)
            {
                PerformBackBeat();
            }
            else
            {
            }
        }

    }

    //マーカー拾われたときに呼ばれるコールバック
    private void OnSequencerCallback(string tag)
    {
        if (tag == "JUMP")
        {
            //UnityEngine.Debug.Log($"Delta: {stopwatch.Elapsed.TotalMilliseconds}");

            stopwatch.Reset();
            stopwatch.Start();
            isMarker = true;

        }
        else if (tag == "BACK")
        {
            backBeatTime = Time.time; //裏打ちが完璧な値
            isMarker = false;
           // UnityEngine.Debug.Log($"Delta: {stopwatch.Elapsed.TotalMilliseconds}");

        }
    }

    //ジャンプ----------------------------------------------------------------------
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

    private void StartJump()
    {
        isJumping = true;

        // 2拍ジャンプ（上昇+滞空+下降）
        MarkerDeltaTime -= Time.deltaTime;
        

       float total = beatInterval * 2f - 0.02f;
        float move = total * jumpSpeedRatio;
        float stay = total - move * 2;//dotweenは１フレーム先で動く。
       
        float targetY = groundY + GetJumpHeight();
        // landingTime = Time.time + total;//着地予定時刻

       // UnityEngine.Debug.Log($"ジャンプ開始: {beatInterval},targetY={targetY}, total={total}, move={move}, stay={stay}");

        var seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(targetY, move).SetEase(Ease.OutQuad));
        seq.AppendCallback(() => canBackBeat = true);//裏打ち受付開始
        seq.AppendInterval(stay);
        seq.AppendCallback(() => canBackBeat = false);//裏打ち受付終了
        seq.Append(transform.DOMoveY(groundY, move).SetEase(Ease.InQuad));

        if (transform.position.y == groundY) { isJumping = false; /*UnityEngine.Debug.Log("groundYと同じ")*/; }

        currentSeq = seq;
        seq.Play();
    }

    // 中断着地（位置を強制的に地面へ）
    private void ForceCutToGround()
    {
        if (currentSeq != null && currentSeq.IsActive()) currentSeq.Kill();
        var p = transform.position; p.y = groundY; transform.position = p;
        isJumping = false;
        canBackBeat = false;

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

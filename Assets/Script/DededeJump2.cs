using CriWare;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
//using System.Collections;
using System.Diagnostics;
//using System.Linq.Expressions;
using System.Threading;
//using Unity.Burst.CompilerServices;
//using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.Playables;
//using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
//using UnityEngine.Serialization;
//using static UnityEngine.EventSystems.EventTrigger;


public class DededeJump2 : MonoBehaviour
{
    Stopwatch stopwatch = new Stopwatch();
    public Animator animator;

    [Header("ビート設定")]
    public float bpm = 140f;

    [Header("ジャンプ挙動")]
    public float normalJumpHeight = 2f;
    public float highJumpHeight = 4f;
    public float superJumpHeight = 7f;
    private float GoalJumpHeight = 20f;

    [Range(0.05f, 0.45f)]
    public float jumpSpeedRatio = 0.24f;   // 上昇/下降に使う比率
    bool isCutting = false;//確認のため。
    private float lastLandingTime = -999f;   // 着地した瞬間の Time.time　UpgradeJumpTypeするか判定するためにつかう
    public float landingWindow = 10f; // この時間内ならアップグレード可能

    [Header("スタート台から飛ぶならfalseにする")] public bool isStartJump = false;
    private Transform FirstDrum;
    public Transform StartPos;

    [Header("裏打ち")]
    public AudioSource seBackBeat;
    public static DededeJump2 Instance { get; private set; }
    public CriAtomSource music;

    long markerDspTime;//ジャンプマーカーのDspTime
    long Jumpflag_DspTime;//ジャンプフラグのDspTime
    long Start_JumpTime;//

    // 内部
    private float beatInterval;//ビート間隔（秒）
    private float groundY;
    private bool isJumping = false;
    public bool canBackBeat = false;
    private float landingTime;
    private float lastJumpMarkerTime = -999f;
    private DG.Tweening.Sequence currentSeq;
    private bool ontheGoal = false;
    public bool ontheDrum = false;
    private Vector3 lastDrumPos;
    int delayStartMusic = 0;
    private bool isOnEdge = false;
    bool Oncetime_Log = false;

    //レイキャスト関連
    public float ray_HorizontalOffset = 0.3f; // 横にどれだけ広げるか
    private bool hit = false;
    private Vector3 leftOrigin;
    private Vector3 rightOrigin;
    private Vector3 dir;
    private bool hitR;
    private bool hitL;
    private bool isleftDrum;
    private bool isrightDrum;

    float rayHeight = 20f;
    float rayLength = 30f;
    float x = 0;
    float y = 0;
    float z = 0;

    [SerializeField]
    private LayerMask groundMask;

    private RaycastHit hitLinfo;
    private RaycastHit hitRinfo;

    private enum JumpType { Normal, High, Super, Ottoto, Goal };//ジャンプの高さ
    private JumpType currentJumpType = JumpType.Normal;
    public enum JumpPhase { Rising, Stay, Falling }//ジャンプのフェーズ
    public JumpPhase currentJumpPhase = JumpPhase.Rising;

    // --- Gizmo用レイ情報 ---
    private Vector3 _gizmoLeftOrigin;
    private Vector3 _gizmoRightOrigin;
    private Vector3 _gizmoDir = Vector3.down;
    private float _gizmoLen;
    private bool _gizmoHitL;
    private bool _gizmoHitR;

    private enum PlayerState//プレイヤーの状態
    {
        START_STATE,
        STAND_STATE,      // 通常プレイ中
        FALL_STATE,
        DAMAGE_STATE,
        RESPAWN_STATE,   // リスポーン中（落下orダメージ→着地待ち）
        GOAL_STATE,
        TIMEOUT_STATE

    }
    private PlayerState playerState = PlayerState.START_STATE;


    public float EdgeSize = 0.3f;//端っこと判定する範囲

    private bool wasUpgradedThisCycle = false;
    private bool Oncetime_Jump_onJumpMarker = false;
    private bool lastJumpUpdate = false;
    private long spacePressedTime = 0;
    private float backBeatTime;
    private float jumpTime;
    private int score;
    private int playerHP = 5;
    public int GetHP() => playerHP;
    public int GetScore() => score;

    private long CRImusic_DspTime;

    public float greatWindow = 0.05f;
    public float goodWindow = 0.1f;

    public static event Action<string> OnMarkerReceived;
    private float moveSpeed = 2.0f;
    private float addMove = 1.3f;
    [Header("1bpmで進むマス")] public float moveDistance = 2.3f;

    bool isJumpMarker = false;
    bool isBackMarker = false;
    private float MarkerDeltaTime = 0f;

    bool lastOntheGoal = false;
    PlayerState lastPlayerState = PlayerState.START_STATE;
    JumpType lastcurrentJumpType = JumpType.Normal;

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
        //groundMask = LayerMask.GetMask("Drum");
        groundMask = LayerMask.GetMask("Ground");
    }

    void Start()
    {
        FirstDrum = StageDataAsset.GetFirstDrumTransform();

        groundY = FirstDrum.transform.position.y;//実行するたびにインスペクターでいれたオブジェクトがリセットされる
        beatInterval = (60f / bpm);//120bpmなら0.5mm秒
                                   //UnityEngine.Debug.Log($"beatInterval : {beatInterval}");
        moveSpeed = moveDistance * 1.3f; // bpmに応じて移動速度を調整

        float twoBwats = beatInterval * 2;
        //moveSpeed = (moveDistance * moveDistance) / twoBwats;
        UnityEngine.Debug.Log($"moveSpeed = {moveSpeed}");

        animator = GetComponent<Animator>();

        PlayMusicDelayed().Forget();
    }

    async UniTaskVoid PlayMusicDelayed()
    {
        await UniTask.Delay(delayStartMusic, cancellationToken: this.GetCancellationTokenOnDestroy());
        music.Play();
    }

    //20msごと
    void FixedUpdate()
    {
        //ここだけ古いほうのdedede2からコピペ
        //ジャンプ中かつ着地予定時刻を過ぎていたら、強制着地
        if (isJumpMarker)
        {
            Jumpflag_DspTime = music.time;
            long delay = Jumpflag_DspTime - markerDspTime;
            //UnityEngine.Debug.Log($"マーカー：{markerDspTime},ジャンプ : {Jumpflag_DspTime},遅延: {delay} ",this);//ここで遅延がおこるのはなぜ？

            /*if(playerState == PlayerState.Normal || playerState == PlayerState.Title) */
            if (!wasUpgradedThisCycle && playerState == PlayerState.STAND_STATE)
            {
                currentJumpType = JumpType.Normal;
            }

            wasUpgradedThisCycle = false; // 次のサイクル用にリセット

            if (playerState == PlayerState.RESPAWN_STATE)
            {
                SyncToMarker();             
            }
            else
            {
                StartJump();
            }

            isJumpMarker = false;
            //CRImusic_DspTime = music.time;
            //UnityEngine.Debug.Log($"dedede ジャンプマーカー　：{CRImusic_DspTime}");
            }

    }


    void Update()
    {
        if (lastPlayerState != playerState)
        { 
            UnityEngine.Debug.Log($"playerstate = {playerState}");
            lastPlayerState = playerState;
        }

        if(currentJumpPhase == JumpPhase.Stay)addMove = 1.5f; else addMove = 1.0f;

        //if (lastcurrentJumpType != currentJumpType)
        //{ 
        //UnityEngine.Debug.Log($"currentJumpType = {currentJumpType}");
        //    lastcurrentJumpType = currentJumpType;
        //}

        if (currentJumpType == JumpType.Ottoto)
        {
            UnityEngine.Debug.Log($"currentJumpType = {currentJumpType}");
        }

        ontheDrum = true;
        //レイのスタート位置をプレイヤーのpositionにしてたけど、Dotweenとpositionはフレーム境界の微妙な差があり綺麗に同期してないので、レイ発射位置のy軸は固定する
        x = transform.position.x;
        y = rayHeight;
        z = transform.position.z;

        leftOrigin = new Vector3(x - ray_HorizontalOffset, y, z);
        rightOrigin = new Vector3(x + ray_HorizontalOffset, y, z);
        dir = Vector3.down;

        Collider col = GetComponent<Collider>();
        col.enabled = false; //自分を無視

        hitL = Physics.Raycast(leftOrigin, dir, out hitLinfo, rayLength, groundMask);
        hitR = Physics.Raycast(rightOrigin, dir, out hitRinfo, rayLength, groundMask);

        isleftDrum = hitL;
        isrightDrum = hitR;

        col.enabled = true;//自分戻す

        ontheDrum = isleftDrum || isrightDrum;
        if (isleftDrum || isrightDrum) { ontheDrum = true; } else { ontheDrum = false; }

        string lastLeftName = "";
        string lastRightName = "";

        string currentL = hitL ? hitLinfo.collider.transform.root.name : "None";
        string currentR = hitR ? hitRinfo.collider.transform.root.name : "None";

        if (currentL != lastLeftName || currentR != lastRightName)
        {
            //UnityEngine.Debug.Log($"●左: {currentL}, ●右: {currentR} , ●ontheDrum :{ontheDrum}");
            lastLeftName = currentL;
            lastRightName = currentR;
        }

        ontheGoal = false;

        //最後に乗ってたドラムの位置を保存
        if (ontheDrum)
        {
            if (isleftDrum)
            {
                Transform drum = hitLinfo.collider.transform.root;
                lastDrumPos = drum.position;
                if (drum.CompareTag("Drum_Goal")) ontheGoal = true;

            }
            if (isrightDrum)
            {
                Transform drum = hitRinfo.collider.transform.root;
                lastDrumPos = drum.position;
                if (drum.CompareTag("Drum_Goal")) ontheGoal = true;

            }
        }

        //UnityEngine.Debug.Log($"hitLinfo.collider.transform.root.name={hitLinfo.collider.transform.root.name}");

        // 横移動
        if (playerState == PlayerState.STAND_STATE)
        {

            if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.position += Vector3.right * Time.deltaTime * moveSpeed;
                //UnityEngine.Debug.Log($"右きー押されてる");
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
                transform.position += Vector3.left * Time.deltaTime * moveSpeed;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                //UnityEngine.Debug.Log($"スペースキー押された");

                SpacePress();
            }
            else
            {
                if (Oncetime_Jump_onJumpMarker == false)
                    lastJumpUpdate = false;
            }
        }
        UnityEngine.Debug.DrawRay(leftOrigin, dir * rayLength, Color.blue);
        UnityEngine.Debug.DrawRay(rightOrigin, dir * rayLength, Color.blue);
    }

    bool HasTagInParents(Transform t, string tagA, string tagB)
    {
        while (t != null)
        {
            if (t.CompareTag(tagA) || t.CompareTag(tagB)) return true;
            t = t.parent;
        }
        return false;
    }


    bool IsValidGround(Collider col)
    {
        return col.CompareTag("Drum") || col.CompareTag("Drum_Goal");
    }

    private void OnTriggerEnter(Collider other)
    {

        if (!other.CompareTag("Enemy")) return;

        TakeDamage(1);

    }

    void TakeDamage(int damage)
    {
        UnityEngine.Debug.Log("TakeDamage");

        if (playerState == PlayerState.DAMAGE_STATE ||
            playerState == PlayerState.RESPAWN_STATE)
            return;

        playerHP -= damage;
        playerState = PlayerState.DAMAGE_STATE;

        DamageSequence();
    }

    async UniTaskVoid DamageSequence()
    {
        // 点滅（2秒）
        await BlinkAsync(2f, this.GetCancellationTokenOnDestroy());

        // ドラムに戻す
        DropToDrum();
    }

    async UniTask BlinkAsync(float duration, CancellationToken ct)
    {
        var renderers = GetComponentsInChildren<Renderer>();
        int count = Mathf.RoundToInt(duration / 0.1f);

        for (int i = 0; i < count; i++)
        {
            bool next = !renderers[0].enabled;
            foreach (var r in renderers) r.enabled = next;
            await UniTask.Delay(100, cancellationToken: ct);
        }
        foreach (var r in renderers) r.enabled = true;
    }



    void CheckDrumCenter(RaycastHit hit)
    {
        Transform drum = hit.collider.transform.root;
        Vector3 drumPos = drum.position;

        // ② 着地フレームだけ軽く補正（吸着）
        bool noInput =
            !Input.GetKey(KeyCode.Space) &&
            !Input.GetKey(KeyCode.RightArrow) &&
            !Input.GetKey(KeyCode.LeftArrow);

        if (!Input.anyKey)
        {
            if (currentJumpPhase == JumpPhase.Falling)
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    new Vector3(drumPos.x, transform.position.y, transform.position.z),
                    0.001f
                );
            }
        }


        // ③ プレイヤー位置をドラムのローカル座標に変換
        Vector3 localPos = drum.InverseTransformPoint(transform.position);

        // ④ ドラムの「実サイズ」を取得
        Collider drumCol = drum.GetComponent<Collider>();
        float halfWidth = drumCol.bounds.extents.x; // ← 半径（ワールド）

        // ⑥ 端判定
        float absX = Mathf.Abs(localPos.x);
        isOnEdge = absX > (halfWidth - EdgeSize);

        // 状態更新
        ontheDrum = true;

        // デバッグ
        //UnityEngine.Debug.Log(
        //    $"プレイヤー − ドラム={absX:F2} / halfWidth={halfWidth:F2} / edge={EdgeSize} / isOnEdge={isOnEdge}"
        //);
    }



    void SpacePress()
    {
        //playerStateがリスポーン中は無効。Normalのときだけ
        if (canBackBeat)
        {
            backBeatTime = Time.time;
            PerformBackBeat();
            return;
        }

        // 着地直後の Upgrade ジャンプ
        float timeSinceLanding = Time.time - lastLandingTime;

        //UnityEngine.Debug.Log($"timeSinceLanding = {timeSinceLanding}landingWindow = {landingWindow}");

        if (timeSinceLanding <= landingWindow)
        {
            if (ontheGoal) { currentJumpType = JumpType.Goal; playerState = PlayerState.GOAL_STATE; UnityEngine.Debug.Log($"ゴールステート");
            }

            UpgradeJumpType();
            wasUpgradedThisCycle = true;
        }
    }


    //マーカーときに呼ばれるコールバック
    private void OnSequencerCallback(string tag)
    {
        if (tag == "JUMP")
        {
            {
                markerDspTime = music.time;
                stopwatch.Reset();
                stopwatch.Start();
                //UnityEngine.Debug.Log($"Delta: {stopwatch.Elapsed.TotalMilliseconds}");
                CRImusic_DspTime = music.time;
                //UnityEngine.Debug.Log($"dedede ジャンプマーカー　：{CRImusic_DspTime}");
                //UnityEngine.Debug.Log($"Jump Marker : {Jump_DspTime},{stopwatch.Elapsed.TotalMilliseconds}");
            }
            isJumpMarker = true;
            isBackMarker = false;
            Oncetime_Jump_onJumpMarker = false;
        }
        else if (tag == "BACK")
        {
            {
                //backBeatTime = Time.time; //裏打ちが完璧な値
                //long Back_DspTime = music.time;
                // UnityEngine.Debug.Log($"裏打ちマーカー　：{Back_DspTime}");            
            }
            isJumpMarker = false;
            isBackMarker = true;
        }
    }

    //ジャンプ----------------------------------------------------------------------
    void UpgradeJumpType()
    {
        //UnityEngine.Debug.Log($"UpgradeJumpType()に入った");
        switch (currentJumpType)
        {
            case JumpType.Normal: currentJumpType = JumpType.High; break;
            case JumpType.High: currentJumpType = JumpType.Super; break;
            case JumpType.Goal: break;
            case JumpType.Super: break;
        }
        return;
    }

    private float GetJumpHeight()
    {
        switch (currentJumpType)
        {
            case JumpType.Normal: return normalJumpHeight;
            case JumpType.High: return highJumpHeight;
            case JumpType.Super: return superJumpHeight;
            case JumpType.Goal: return GoalJumpHeight; // または専用の高さ
            case JumpType.Ottoto: return normalJumpHeight;

            default: return normalJumpHeight;
        }
    }

    private void StartJump()
    {
        if (playerState == PlayerState.RESPAWN_STATE) return;// リスポーン中はジャンプ不可

        isJumping = true;

        float total = beatInterval * 2f - 0.02f;
        float move = total * jumpSpeedRatio;// jumpSpeedRatio = 0.24f
        float stay = total - move * 2;

        float targetY = groundY + GetJumpHeight();

        var seq = DOTween.Sequence();

        // 最初のジャンプ===================================================================
        if (playerState == PlayerState.START_STATE)
        {
            //isStartJump = true;

            // Vector3 start = transform.position;
            // Vector3 end = FirstDrum.transform.position;
            Vector3 target = new Vector3(
           FirstDrum.position.x, // X = 中間
            StartPos.transform.position.y + GetJumpHeight(),               // Y = スタート台の高さ + ジャンプ量
            0f                                          // Z = 0 固定
             );
            //(StartPos.transform.position.x + FirstDrum.transform.position.x) * 0.5f

            Vector3 start = StartPos.position;
            Vector3 end = FirstDrum.position;

            float  jumpPower= GetJumpHeight() * 1.4f;//まるさ
            float duration = total;  //total            // 今の move をそのまま使う

            seq.Append(transform.DOJump(
                end,        // 着地点
                target.y,  // 頂点の高さ
                1,          // ジャンプ回数（1回）
                duration    // 所要時間
            ).SetEase(Ease.OutQuad)).AppendCallback(() => { canBackBeat = false; animator.SetTrigger("Trg_JumpUp"); currentJumpPhase = JumpPhase.Rising; })
              .AppendInterval(stay).AppendCallback(() => { currentJumpPhase = JumpPhase.Stay; })
              .AppendCallback(() => { canBackBeat = true;  })//animator.SetTrigger("Trg_JumpLoop");
              .Append(transform.DOMove(FirstDrum.transform.position, move).SetEase(Ease.InQuad))
              .AppendCallback(() =>
              {
                  canBackBeat = false;
                  currentJumpPhase = JumpPhase.Falling;
                  playerState = PlayerState.STAND_STATE;
                  isStartJump = true;
                  isJumping = false;
                  lastLandingTime = Time.time;
                  animator.SetTrigger("Trg_JumpDown");
                  //UnityEngine.Debug.Log($"はじめのジャンプ完了");
              });

            currentSeq = seq;
            seq.Play();
        }
        //ゴールジャンプ===================================================================
        else if (playerState == PlayerState.GOAL_STATE)
        {
            targetY = 20.0f;
            seq.Append(transform.DOMoveY(targetY, move*3).SetEase(Ease.OutQuad))
               .AppendCallback(() => { animator.SetTrigger("Trg_JumpUp"); })
               .AppendInterval(stay).AppendCallback(() => { currentJumpPhase = JumpPhase.Stay; })
               .AppendCallback(() =>
               {

                   currentJumpPhase = JumpPhase.Rising;

                   // データ保存してシーン遷移
                   GameData.Instance.Score = score;
                   GameData.Instance.HP = playerHP; // HPの変数名に合わせて

                   UnityEngine.Debug.Log("ゴールジャンプ完了");

                   LoadNextScene();

               });//canBackBeat = false);
                  // 着地なし


        }

        // 通常ジャンプ===================================================================
        else if (playerState == PlayerState.STAND_STATE)
        {
            // 上昇
            seq.Append(
            transform.DOMoveY(targetY, move).SetEase(Ease.OutQuad)
            ).AppendCallback(() =>
            {
                // 上昇終わり → 滞空開始
                animator.SetTrigger("Trg_JumpUp");
                //animator.SetTrigger("Trg_JumpLoop");
                canBackBeat = true;                 // ★ここから滞空
                currentJumpPhase = JumpPhase.Stay;
            });

            // 滞空
            seq.AppendInterval(stay);

            // 下降に入る瞬間でOFF & フェーズ変更
            seq.AppendCallback(() =>
            {
                canBackBeat = false;                // ★滞空終了
                currentJumpPhase = JumpPhase.Falling;
                animator.SetTrigger("Trg_JumpDown");
            });

            // 下降
            seq.Append(
                transform.DOMoveY(groundY, move).SetEase(Ease.InQuad)
            ).OnComplete(() =>
            {
                isJumping = false;
                lastLandingTime = Time.time;

                if (!ontheDrum)
                {
                    UnityEngine.Debug.Log($"ドラムの上にいないので落下開始");
                    StartFall();

                    playerState = PlayerState.FALL_STATE;
                }
                else
                {
                    if (isOnEdge)
                    {
                        currentJumpType = JumpType.Ottoto;
                    }
                }
            });

        }

        if (transform.position.y == groundY) { isCutting = true;/* isJumping = false; UnityEngine.Debug.Log("groundYと同じ");*/ } else { isCutting = false; }
        currentSeq = seq;
        seq.Play();

        //UnityEngine.Debug.Log($"ジャンプ開始: {beatInterval},targetY={targetY}, groundY = {groundY},FirstDrum={FirstDrum.position.y}total={total}, move={move}, stay={stay}");
        // UnityEngine.Debug.Log($"groundY = {groundY},FirstDrum={FirstDrum.position.y}");

    }




    private void PerformBackBeat()
    {
        float error = Time.time - backBeatTime;
        JudgeType judge = Judge(error);

        if (judge != JudgeType.MISS)
        {
            int bonus = judge == JudgeType.GREAT ? 100 : 50;
            score += bonus;

            // エフェクト表示
            JudgeEffectManager.Instance.ShowJudge(judge);
        }
        // UnityEngine.Debug.Log($"裏打ち入力! 評価 :{judge} 合計スコア：{score}");

        canBackBeat = false;
    }

    JudgeType Judge(float error)
    {
        float abs = Mathf.Abs(error);
        if (abs <= greatWindow) return JudgeType.GREAT;
        if (abs <= goodWindow) return JudgeType.GOOD;
        return JudgeType.MISS;
    }
    async UniTaskVoid LoadNextScene()
    {
        music.Stop();
        await UniTask.Delay(2000);
        await SceneManager.LoadSceneAsync("ResultScene");
    }



    //void CheckLanding()
    //{
    //    UnityEngine.Debug.Log($"CheckLanding");
    //    // 足元座標
    //    Vector3 footPos = transform.position + Vector3.down;
    //    Vector3 dir = Vector3.down;
    //    float dist = 10.0f;

    //    //Ray ray = new Ray(footPos, dir);

    //    //bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, dist);
    //    //UnityEngine.Debug.DrawRay(ray.origin, ray.direction * dist, Color.red, 10.0f);

    //    bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10f);
    //    UnityEngine.Debug.DrawRay(transform.position, Vector3.down * 10f, Color.red);


    //    if (!isGrounded)
    //    {
    //        StartFall();
    //    }
    //}




    void StartFall()
    {
        UnityEngine.Debug.Log($"落ちた");

        playerState = PlayerState.FALL_STATE;

        DOTween.Kill(transform); // もし残った Tween があれば停止


        transform.DOMoveY(transform.position.y - 9f, 0.8f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                StartRespawn();
            });
    }

    void StartRespawn()
    {
        playerState = PlayerState.RESPAWN_STATE;
        currentJumpType = JumpType.Normal;

        Vector3 respawnPos = new Vector3(lastDrumPos.x, lastDrumPos.y + normalJumpHeight, lastDrumPos.z);
        DOTween.Kill(transform);
        transform.position = respawnPos;

        DOVirtual.DelayedCall(1f, () =>
        {
            DropToDrum();
        });
    }

    void DropToDrum()
    {
        UnityEngine.Debug.Log("DropToDrum");


        Vector3 landPos = new Vector3(
            transform.position.x,//?
            lastDrumPos.y,
            transform.position.z
        );

        DOTween.Kill(transform);
        if (playerState == PlayerState.FALL_STATE) { playerState = PlayerState.STAND_STATE; return; }



            //if (isJumpMarker) { 
            //    transform.DOMove(landPos, 0.5f)
            //    .SetEase(Ease.InQuad)
            //    .OnComplete(() =>
            //    {
            //        //StartRespawnJump();
            //        StartJump();
            //    });
            //}

        var seq = DOTween.Sequence();
        float total = beatInterval * 2f - 0.02f;
        float move = total * jumpSpeedRatio;

        //どっちかのタイミングでおりれば、綺麗に間に合う その間は
        if (isJumpMarker || isBackMarker)
        {
            seq.Append(
                transform.DOMoveY(groundY, move).SetEase(Ease.InQuad)
            ).OnComplete(() =>
            {
                isJumping = false;
                lastLandingTime = Time.time;

                if (!ontheDrum)
                {
                    UnityEngine.Debug.Log($"ドラムの上にいないので落下開始");
                    StartFall();

                    playerState = PlayerState.FALL_STATE;
                }
                else
                {
                    if (isOnEdge)
                    {
                        currentJumpType = JumpType.Ottoto;
                    }
                }
            });
        }
    }


    void StartRespawnJump()
    {
        if (playerState == PlayerState.FALL_STATE) { UnityEngine.Debug.Log("ダメージなのでStartRespawnJump()をスキップ"); playerState = PlayerState.STAND_STATE; return; }

        //落下したときに、targetYからリスポーン
        playerState = PlayerState.RESPAWN_STATE;
        float targetY = groundY + normalJumpHeight;

        transform.DOMoveY(targetY, 0.3f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // 滞空して次のマーカーを待つ

            });
        //もしかしてStartRespawnJumpいらない？！
    }

    //リスポーン後、次のジャンプマーカーに合わせて着地する
    void SyncToMarker()
    {
        float move = 0.2f;
        float total = beatInterval * 2f - 0.02f;
        float stay = total - move * 2;

        var seq = DOTween.Sequence();
        seq.AppendCallback(() => { canBackBeat = true; currentJumpPhase = JumpPhase.Stay; })
           .AppendInterval(stay)
           .AppendCallback(() => { canBackBeat = false; currentJumpPhase = JumpPhase.Falling; })
           .Append(transform.DOMoveY(groundY, move).SetEase(Ease.InQuad))
           .OnComplete(() =>
           {
               playerState = PlayerState.STAND_STATE;
               lastLandingTime = Time.time;
               isJumping = false;
           });

        currentSeq = seq;
        seq.Play();
    }
    //さっきいたドラムの上(取得しとく。)で、normalJumpHeightの高さにリスポーンして１秒待って、
    //DropToDrum()で、そのまま真下のドラムに着地するケースと、lastDrumPosに向かって弧を描いて着地するケースが欲しい。落下とダメージに対応したい。

    //startjump()のロジックのように、jumpマーカーに間に合うように着地したい。
    //リスポーン挙動はマーカー基準ではないのでどんなbpmでも、落下～リスポーン～ド
    //ラム上に降りる速さは同じ。＊リスポーン時の着地でspaceは無効にする。

}
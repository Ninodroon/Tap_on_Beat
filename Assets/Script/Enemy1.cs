using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class Enemy1 : EnemyBase
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int switchEveryNBeats = 2; // jumpマーカー何個で方向転換するか
    [SerializeField] private float turnDuration = 0.05f; // 振り向き時間


    [Header("奥行きスケール設定")]
    [SerializeField] private float zNear = 0f;   // 手前のZ座標
    [SerializeField] private float zFar = 8f;   // 奥のZ座標
    [SerializeField] private float scaleNear = 1f;   // 手前でのスケール
    [SerializeField] private float scaleFar = 0.7f; // 奥でのスケール

    private int direction = 1;//方向
    private bool isMoving = false;
    private int markerCount = 0;     // マーカーを数えるカウンター
    private CancellationTokenSource moveCts;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        AdxMarkerBroadcaster.OnMarkerReceived += OnMarkerReceived;
    }

    private void OnDisable()
    {
        AdxMarkerBroadcaster.OnMarkerReceived -= OnMarkerReceived;
        moveCts?.Cancel();
    }

    private void OnMarkerReceived(string tag)
    {
        if (tag != "JUMP") return;

        markerCount++;
        if (markerCount % switchEveryNBeats != 0) return;

        // 前の移動・回転を止める
        moveCts?.Cancel();
        moveCts = CancellationTokenSource.CreateLinkedTokenSource(
            this.GetCancellationTokenOnDestroy()
        );

        if (isMoving) direction *= -1;
        isMoving = true;

        TurnAndMove(moveCts.Token).Forget();
    }

    // ─────────────────────────────────
    // 振り向き（ease-out）→ そのまま移動
    // ─────────────────────────────────
    private async UniTaskVoid TurnAndMove(CancellationToken token)
    {
        // 回転アニメーション
        Quaternion from = transform.rotation;
        Quaternion to = Quaternion.LookRotation(Vector3.forward * direction);

        float elapsed = 0f;
        while (elapsed < turnDuration)
        {
            if (token.IsCancellationRequested) return;
            float p = Mathf.Clamp01(elapsed / turnDuration);
            //float s = 1f - (1f - p) * (1f - p); // ease-out：最初速く、最後ゆっくり
            float s = p * p * p; // ease-out：最初速く、最後ゆっくり
            transform.rotation = Quaternion.Slerp(from, to, s);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
            elapsed += Time.deltaTime;
        }
        transform.rotation = to;

        // 移動ループ
        while (!token.IsCancellationRequested)
        {
            transform.position += Vector3.forward * direction * moveSpeed * Time.deltaTime;
            float t = Mathf.InverseLerp(zNear, zFar, transform.position.z);
            float sc = Mathf.Lerp(scaleNear, scaleFar, t);
            transform.localScale = originalScale * sc;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }
}
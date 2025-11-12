using DG.Tweening;
using UnityEngine;

public class StartJumpMover : MonoBehaviour
{
    [Header("ジャンプポイント設定")]
    public Transform startPoint;   // スタート台の位置
    public Transform targetPoint;  // 1つ目ドラムの位置

    [Header("ジャンプ設定")]
    public float jumpHeight = 2.5f; // 放物線の高さ
    public float duration = 1.2f;   // ジャンプ時間
    public AudioSource startSE;     // ゲーム開始の笛（任意）

    private Sequence seq;

    void Start()
    {
        // 最初にスタート位置へ
        transform.position = startPoint.position;

        // ゲーム開始音を鳴らす
        if (startSE != null) startSE.Play();

        // 少しだけ遅延を入れてジャンプ開始
        Invoke(nameof(StartJump), 0.2f);
    }

    void StartJump()
    {
        seq?.Kill();

        // 最高点を計算（中間の高さを持ち上げる）
        Vector3 peak = Vector3.Lerp(startPoint.position, targetPoint.position, 0.5f);
        peak.y += jumpHeight;

        // 経路を指定
        Vector3[] path = new Vector3[] { peak, targetPoint.position };

        // 放物線のように補間
        seq = DOTween.Sequence();
        seq.Append(transform.DOPath(path, duration, PathType.CatmullRom)
            .SetEase(Ease.OutQuad)
            .SetLookAt(0.01f)); // 前を向かせたい場合

        // 着地後にプレイヤー制御を有効化するならここでイベント呼ぶ
        seq.OnComplete(() =>
        {
            Debug.Log("スタートジャンプ完了。");
            // 例：PlayerController.Instance.EnableControl();
        });
    }
}

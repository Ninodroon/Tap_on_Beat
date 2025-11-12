using UnityEngine;
using DG.Tweening;
using System;

public class Coin : MonoBehaviour
{
    [Header("コイン設定")]
    public int value = 100; // コインの価値
    public float rotateSpeed = 90f; // 回転速度（度/秒）
    public float bobHeight = 0.2f; // 浮遊の高さ
    public float bobSpeed = 2f; // 浮遊の速度

    [Header("エフェクト")]
    public GameObject collectEffect; // 取得時のエフェクト
  //  public AudioClip collectSound; // 取得時のサウンド

    private Vector3 startPos;
   // private AudioSource audioSource;

    public static event Action<int> OnCoinCollected; // コイン取得イベント

    void Start()
    {
        startPos = transform.position;
        //audioSource = GetComponent<AudioSource>();

        // 回転アニメーション
        transform.DORotate(new Vector3(0, 360, 0), 360f / rotateSpeed, RotateMode.FastBeyond360)
                 .SetLoops(-1, LoopType.Restart)
                 .SetEase(Ease.Linear);

        // 浮遊アニメーション
        transform.DOMoveY(startPos.y + bobHeight, 1f / bobSpeed)
                 .SetLoops(-1, LoopType.Yoyo)
                 .SetEase(Ease.InOutSine);
    }

    void OnTriggerEnter(Collider other)
    {
        // プレイヤーとの接触判定
        if (other.GetComponent<DededeJump>() != null)
        {
            CollectCoin();
            Debug.Log("コイン取得");
        }
    }

    void CollectCoin()
    {
        // イベント発火
        OnCoinCollected?.Invoke(value);

        // エフェクト生成
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        //// サウンド再生
        //if (collectSound != null && audioSource != null)
        //{
        //    audioSource.PlayOneShot(collectSound);
        //}

        // 取得アニメーション（スケールダウン + フェードアウト）
        transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);

        // コイン削除
        Destroy(gameObject, 0.3f);
        Debug.Log("コイン  削除");

    }

    void OnDestroy()
    {
        // DOTweenのクリーンアップ
        transform.DOKill();
    }
}

// コイン管理用のマネージャークラス
public class CoinManager : MonoBehaviour
{
    [Header("スコア管理")]
    public int totalScore = 0;
    public int coinCount = 0;

    [Header("UI")]
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI coinText;

    void OnEnable()
    {
        Coin.OnCoinCollected += OnCoinCollected;
    }

    void OnDisable()
    {
        Coin.OnCoinCollected -= OnCoinCollected;
    }

    void OnCoinCollected(int value)
    {
        totalScore += value;
        coinCount++;

        Debug.Log($"コイン取得！ +{value}点 (合計: {totalScore}点, {coinCount}枚)");

        // UI更新
        if (scoreText != null)
            scoreText.text = "Score: " + totalScore;
        if (coinText != null)
            coinText.text = "Coins: " + coinCount;
    }
}
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

//プレイヤーと当たると点滅して消える敵の基底クラス

public abstract class EnemyBase : MonoBehaviour
{
    [Header("挙動設定")]
    [SerializeField] protected bool doDestroy = true;

    [Header("点滅設定")]
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private float blinkDuration = 1f;

    protected Vector3 originalScale;
    protected Collider col;
    protected Renderer rend;
    private bool isHit = false;

    protected virtual void Awake()
    {
        col = GetComponent<Collider>();
        rend = GetComponentInChildren<Renderer>();
        originalScale = transform.localScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isHit) return;
        if (!other.CompareTag("Player")) return;

        isHit = true;
        if (col != null) col.enabled = false;

        if (doDestroy)
            BlinkAndDestroy(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid BlinkAndDestroy(CancellationToken token)
    {
        if (rend != null)
        {
            float elapsed = 0f;
            while (elapsed < blinkDuration)
            {
                if (token.IsCancellationRequested) return;
                rend.enabled = !rend.enabled;
                await UniTask.Delay(
                    (int)(blinkInterval * 1000),
                    cancellationToken: token
                );
                elapsed += blinkInterval;
            }
            rend.enabled = true;
        }
        Destroy(gameObject);
    }
}
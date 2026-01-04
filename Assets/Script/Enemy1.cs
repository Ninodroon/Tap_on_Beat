using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Enemy1 : MonoBehaviour
{
    [SerializeField] private int blinkCount = 6;
    [SerializeField] private float blinkInterval = 0.1f;

    // Bee flight parameters
    [SerializeField] private float beeDistance = 3f;     // local z方向の移動距離
    [SerializeField] private float beeDuration = 1f;     // 片方向に移動する所要時間
    [SerializeField] private bool beeLoop = true;        // 行き来を繰り返すか

    private bool isBlinking = false;

    // Start is called before the first frame update
    void Start()
    {
        BeeFly(beeDistance, beeDuration, beeLoop, this.GetCancellationTokenOnDestroy()).Forget();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private async void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // UniTask を返すメソッドを await して終了まで待つ（例: 2秒間点滅）
            await Blink(2f);
        }
    }

    async UniTask BeeFly(float distance, float duration, bool loop, CancellationToken token = default)
    {
        if (duration <= 0f) return;

        // 初期位置を基準に往復する
        Vector3 start = transform.position;
        Vector3 forward = transform.forward.normalized;
        Vector3 a = start;
        Vector3 b = start + forward * distance;

        do
        {
            // a -> b
            float t = 0f;
            while (t < duration)
            {
                if (token.IsCancellationRequested || this == null) return;
                float p = Mathf.Clamp01(t / duration);
                // 二次イージング（ease-in-out）
                float s = (p < 0.5f) ? (2f * p * p) : (1f - 2f * Mathf.Pow(1f - p, 2f));
                transform.position = Vector3.Lerp(a, b, s);
                await UniTask.Yield(token);
                t += Time.deltaTime;
            }
            transform.position = b;

            // b -> a
            t = 0f;
            while (t < duration)
            {
                if (token.IsCancellationRequested || this == null) return;
                float p = Mathf.Clamp01(t / duration);
                float s = (p < 0.5f) ? (2f * p * p) : (1f - 2f * Mathf.Pow(1f - p, 2f));
                transform.position = Vector3.Lerp(b, a, s);
                await UniTask.Yield(token);
                t += Time.deltaTime;
            }
            transform.position = a;

        } while (loop && !token.IsCancellationRequested && this != null);
    }

    async UniTask Blink(float duration)
    {
        float t = 0f;
        var rend = GetComponentInChildren<Renderer>();
        if (rend == null) return;

        while (t < duration)
        {
            if (rend == null) break;
            rend.enabled = !rend.enabled;
            await UniTask.Delay((int)(blinkInterval * 1000));
            t += blinkInterval;
        }

        if (rend != null) rend.enabled = true;
    }
}

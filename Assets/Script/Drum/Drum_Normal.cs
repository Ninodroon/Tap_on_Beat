using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using static UnityEngine.UI.Image;

/*
 ドラム
 プレイヤーが着地したら潰れる
 */

public class Drum_Normal : MonoBehaviour
{
    Vector3 originalScale;

    float timer = 999f;
    public ParticleSystem effectPrefab;


    [Header("へこみ設定 amountは基本0.25")]
    private float amount = 0.5f;     // A = 0.25 → 25%へこむ
    private float damping = 6f;       // k：減衰の強さ
    private float frequency = 20f;    // ω：振動の速さ
    private float maxTime = 0.5f;     // 揺れが続く時間

    private float delay = 0.05f; // 遅延秒数


    void Start()
    {
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        AdxMarkerBroadcaster.OnMarkerReceived += OnMarker;
    }

    void OnDisable()
    {
        AdxMarkerBroadcaster.OnMarkerReceived -= OnMarker;
    }

    void OnMarker(string tag)
    {
        if (tag == "JUMP")
        {
            timer = -delay;
            //timer =0;

        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > maxTime)
        {
            transform.localScale = originalScale;
            return;
        }

        float t = timer;
        float wave = Mathf.Exp(-damping * t) * Mathf.Abs(Mathf.Cos(frequency * t));//バウンス
        float yScale = 1f - amount * wave;
        float xzScale = 1f + amount * 0.5f * wave;

        transform.localScale = new Vector3(
            originalScale.x * xzScale,
            originalScale.y * yScale,
            originalScale.z * xzScale
        );

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        // 接触点（最初の1点）
        Vector3 contactPos = collision.contacts[0].point;

        // パーティクル生成
        ParticleSystem ps = Instantiate(effectPrefab, contactPos, Quaternion.identity);
        ps.Play();

        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }
}

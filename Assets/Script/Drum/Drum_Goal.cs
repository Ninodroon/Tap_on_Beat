using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drum_Goal : MonoBehaviour
{
    Vector3 originalScale;

    float timer = 999f;

    [Header("‚Ö‚±‚ÝÝ’è amount‚ÍŠî–{0.25")]
    private float amount = 0.5f;     // A = 0.25 ¨ 25%‚Ö‚±‚Þ
    private float damping = 6f;       // kFŒ¸Š‚Ì‹­‚³
    private float frequency = 20f;    // ƒÖFU“®‚Ì‘¬‚³
    private float maxTime = 0.5f;     // —h‚ê‚ª‘±‚­ŽžŠÔ
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
            timer = 0f;
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
        float wave = Mathf.Exp(-damping * t) * Mathf.Abs(Mathf.Cos(frequency * t));
        float yScale = 1f - amount * wave;

        transform.localScale = new Vector3(
            originalScale.x,
            originalScale.y * yScale,
            originalScale.z
        );
    }
}

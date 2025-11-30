using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drum_Break : MonoBehaviour
{
    Vector3 originalScale;
    public float squashAmount = 0.85f;
    int stepCount = 0;
    bool isBroken = false;

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

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isBroken) return;

        stepCount++;

        if (stepCount == 1)
        {
            // 少し縮む
            transform.localScale = new Vector3(originalScale.x, originalScale.y * squashAmount, originalScale.z);
        }
        else if (stepCount == 2)
        {
            // 壊れる（非表示）
            gameObject.SetActive(false);
            isBroken = true;
        }
    }

    void OnMarker(string tag)
    {
        if (isBroken && tag == "JUMP")
        {
            stepCount++;

            // 3回目のマーカーで復活
            if (stepCount >= 3)
            {
                stepCount = 0;
                isBroken = false;
                transform.localScale = originalScale;
                gameObject.SetActive(true);
            }
        }
    }
}
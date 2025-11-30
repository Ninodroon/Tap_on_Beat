using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 ƒhƒ‰ƒ€
 •\— ‚É‚Ð‚Á‚­‚è•Ô‚é
 */

public class Drum_FlipOn : MonoBehaviour
{
    float timer = 999f;
    float startZ;
    float endZ;

    [Header("Bounce Rotation Ý’è")]
    public float damping = 1f;      // Œ¸Š
    public float frequency = 18f;   // —h‚ê‘¬“x
    public float maxTime = 0.4f;    // —h‚ê‘±‚­ŽžŠÔ

    bool isFront = true;


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

            isFront = !isFront;

            startZ = transform.eulerAngles.z;

            endZ = isFront ? 0f : 180f;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > maxTime)
        {
            transform.rotation = Quaternion.Euler(0, 0, endZ);
            return;
        }

        float t = timer;

        float wave = 1f - Mathf.Exp(-damping * t) * Mathf.Cos(frequency * t);

        float z = Mathf.Lerp(startZ, endZ, wave);

        transform.rotation = Quaternion.Euler(0, 0, z);
    }
}


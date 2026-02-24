using UnityEngine;

/*
 ƒhƒ‰ƒ€
 •\— ‚É‚Ð‚Á‚­‚è•Ô‚é
 */
public class Drum_FlipOn : MonoBehaviour
{
    float timer = 999f;
    Quaternion startRot;
    Quaternion endRot;

    [Header("Bounce Rotation Ý’è")]
    public float damping = 1f;
    public float frequency = 18f;
    public float maxTime = 0.4f;

    bool waitMove = false;
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
            if (!waitMove)
            {
                timer = 0f;
                isFront = !isFront;
                startRot = transform.rotation;
                endRot = Quaternion.Euler(0, 0, isFront ? 0f : 180f);
                waitMove = true;
            }
            else
            {
                waitMove = false;
            }
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > maxTime)
        {
            transform.rotation = endRot;
            return;
        }

        float wave = 1f - Mathf.Exp(-damping * timer) * Mathf.Cos(frequency * timer);
        transform.rotation = Quaternion.Slerp(startRot, endRot, wave);
    }
}
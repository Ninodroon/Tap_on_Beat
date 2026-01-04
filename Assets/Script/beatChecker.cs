using UnityEngine;

public class BeatChecker : MonoBehaviour
{
    private Renderer rend;
    private Color originalColor;
    private Color beatColor = Color.red;

    private float flashTimer = 0f;
    private float flashDuration = 0.1f;

    private bool isMarker = false;

    long markerDspTime;
    long jumpDspTime;

    public bool beatChecker = false;
    public bool ontheDrumChecker = false;
    public bool isStayChecker = false;

    private void OnEnable()
    {
        AdxMarkerBroadcaster.OnMarkerReceived += OnSequencerCallback;
    }

    private void OnDisable()
    {
        AdxMarkerBroadcaster.OnMarkerReceived -= OnSequencerCallback;
    }

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    // ---- コールバック：ここでは絶対に色を変えない ----
    void OnSequencerCallback(string tag)
    {
        if (tag == "JUMP")
        {
            markerDspTime = DededeJump2.Instance.music.time;
            isMarker = true;
        }
    }

    // ---- 全ての描画は Update に固定 ----
    void Update()
    {
        //マーカーで光る
        if (beatChecker)
        {
            if (isMarker)
            {
                isMarker = false;

                rend.material.color = beatColor;
                flashTimer = flashDuration;

                jumpDspTime = DededeJump2.Instance.music.time;
                long delay = jumpDspTime - markerDspTime;
                //UnityEngine.Debug.Log($"マーカー：{markerDspTime},ひかる : {jumpDspTime},遅延: {delay} ");
            }

            // フラッシュ終了処理
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f)
                {
                    rend.material.color = originalColor;
                }
            }
        }

        if (ontheDrumChecker)
        {

            if (DededeJump2.Instance.ontheDrum)
            {
                rend.material.color = Color.red;
            }
            else
            {
                rend.material.color = originalColor;
            }
        }

        if (isStayChecker)
        {
            if (DededeJump2.Instance.currentJumpPhase == DededeJump2.JumpPhase.Stay)
            {
               // UnityEngine.Debug.Log($"Stayす");
                rend.material.color = Color.red;
            }
            else
            {
                //UnityEngine.Debug.Log($"Stayじゃないです");
                rend.material.color = originalColor;
            }
        }
    }
}

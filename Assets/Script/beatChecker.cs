using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Renderer))]
public class beatChecker : MonoBehaviour
{
    [Header("ビート設定")]
    public float bpm = 120f;           // BPM（1分あたりのビート数）
    private float beatInterval;        // 1ビートの秒数
    private float timer;
    private int beatCount = 0;


    [Header("色設定")]
    public Color beatColor = Color.red; // 一瞬変える色
    public float flashDuration = 0.1f;  // 色を維持する時間

    private Color originalColor;
    private Renderer rend;

    void Start()
    {
        beatInterval = 60f / bpm;
        timer = 0f;

        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    void Update()
    {
        timer += Time.deltaTime; // 前フレームからの経過時間を足す

        if (timer >= beatInterval)
        {
            FlashColor();
            OnBeat();

            timer -= beatInterval; // 次の拍のタイミングへ
        }
    }

    void FlashColor()
    {
        rend.material.DOColor(beatColor, flashDuration / 2f).OnComplete(() =>
        {
            rend.material.DOColor(originalColor, flashDuration / 2f);
        });
    }

    void OnBeat()
    {
        beatCount++;
        // 奇数拍が表拍、偶数拍が裏拍
        bool isBackBeat = beatCount % 2 == 0;
        if (isBackBeat)
        {
            // 裏打ちチャンス発生
        }
    }
}

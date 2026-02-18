using DG.Tweening;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class JudgeEffectManager : MonoBehaviour
{
    public static JudgeEffectManager Instance { get; private set; }

    public GameObject drumEffectPrefab;
    public Image judgeImage; // Inspector に割り当て
    public Sprite greatSprite;
    public Sprite goodSprite;
    //public Sprite missSprite;

    public float displayDuration = 0.5f;
    public Vector3 offsetFromPlayer = new Vector3(0, -1f, 0);

    private float hideTimer = 0f;
    private CanvasGroup _canvasGroup;

    void Awake()
    {
        Instance = this;
        if (judgeImage != null)
        {
            judgeImage.gameObject.SetActive(false);
            // フェード操作用に CanvasGroup を追加しておく（無ければ作成）
            _canvasGroup = judgeImage.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = judgeImage.gameObject.AddComponent<CanvasGroup>();
            }
            _canvasGroup.alpha = 1f;
        }
    }

    void Update()
    {
        if (hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
            {
                if (judgeImage != null) judgeImage.gameObject.SetActive(false);
            }
        }

        // プレイヤーの位置に追従（スクリーン座標へ）
        if (DededeJump2.Instance != null && judgeImage != null && judgeImage.gameObject.activeSelf)
        {
            Vector3 worldPos = DededeJump2.Instance.transform.position + offsetFromPlayer;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            judgeImage.rectTransform.position = screenPos;
        }
    }

    // 評価に応じてスプライト表示する既存呼び出しを維持
    public void ShowJudge(JudgeType judge)
    {
        if (judgeImage == null) return;

        // スプライト選択
        switch (judge)
        {
            case JudgeType.GREAT:
                judgeImage.sprite = greatSprite;
                break;
            case JudgeType.GOOD:
                judgeImage.sprite = goodSprite;
                break;
            default:
                //judgeImage.sprite = missSprite;
                break;
        }

        judgeImage.SetNativeSize();
        judgeImage.gameObject.SetActive(true);

        // 表示時間セット
        hideTimer = displayDuration;

        // フェードイン→保持→フェードアウト（DOTween使用）
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.DOFade(1f, 0.08f).OnComplete(() =>
            {
                // 表示終了時にフェードアウト
                DOVirtual.DelayedCall(displayDuration, () =>
                {
                    _canvasGroup.DOFade(0f, 0.18f).OnComplete(() =>
                    {
                        judgeImage.gameObject.SetActive(false);
                    });
                });
            });
        }
    }

    // 既存のドラムエフェクトはそのまま
    public void ShowDrumEffect(Vector3 drumPosition)
    {
        GameObject tempEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tempEffect.transform.position = drumPosition + Vector3.up * 0.2f;
        tempEffect.transform.localScale = Vector3.one * 0.2f;

        Renderer renderer = tempEffect.GetComponent<Renderer>();
        renderer.material.color = Color.white;

        Destroy(tempEffect.GetComponent<Collider>());

        Sequence seq = DOTween.Sequence();

        seq.Append(tempEffect.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutQuad))
           .Join(renderer.material.DOFade(0f, 0.25f))
           .OnComplete(() =>
           {
               Destroy(tempEffect);
           });
    }
}
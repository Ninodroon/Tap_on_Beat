// JudgeEffectManager.cs - 新規作成
using DG.Tweening;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;


public class JudgeEffectManager : MonoBehaviour
{
    public static JudgeEffectManager Instance { get; private set; }

    public GameObject drumEffectPrefab;
    public TextMeshProUGUI judgeText;
    public float displayDuration = 0.5f;
    public Vector3 offsetFromPlayer = new Vector3(0, -1f, 0);

    private float hideTimer = 0f;

    void Awake()
    {
        Instance = this;
        if (judgeText != null)
        {
            judgeText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
            {
                judgeText.gameObject.SetActive(false);
            }
        }

        // プレイヤーの位置に追従
        if (DededeJump2.Instance != null && judgeText.gameObject.activeSelf)
        {
            Vector3 worldPos = DededeJump2.Instance.transform.position + offsetFromPlayer;
            judgeText.transform.position = Camera.main.WorldToScreenPoint(worldPos);
        }
    }

    public void ShowJudge(JudgeType judge)
    {
        if (judgeText == null) return;

        judgeText.text = judge.ToString();

        // 色分け
        if (judge == JudgeType.GREAT)
            judgeText.color = Color.yellow;
        else if (judge == JudgeType.GOOD)
            judgeText.color = Color.green;

        judgeText.gameObject.SetActive(true);
        hideTimer = displayDuration;
    }

    public void ShowDrumEffect(Vector3 drumPosition)
    {
        // まずは白い点を作る
        GameObject tempEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tempEffect.transform.position = drumPosition + Vector3.up * 0.2f;
        tempEffect.transform.localScale = Vector3.one * 0.2f;

        Renderer renderer = tempEffect.GetComponent<Renderer>();
        renderer.material.color = Color.white;

        // コライダー不要
        Destroy(tempEffect.GetComponent<Collider>());

        // DOTweenで "広がり + フェードアウト"
        Sequence seq = DOTween.Sequence();

        seq.Append(tempEffect.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutQuad))
           .Join(renderer.material.DOFade(0f, 0.25f))
           .OnComplete(() =>
           {
               Destroy(tempEffect);
           });
    }




}
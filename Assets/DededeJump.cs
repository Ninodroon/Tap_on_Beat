using UnityEngine;
using DG.Tweening;
public enum JumpType
{
    Normal,
    High,
    Super,
    Ottoto
}
public class DededeJump : MonoBehaviour
{
    [Header("ビート設定")]
    public float bpm = 0f;
    private float beatInterval;  // 1拍の時間（秒）
    [Header("ジャンプ設定")]
    public float normalJumpHeight = 2f;
    public float highJumpHeight = 4f;
    public float superJumpHeight = 7f;
    public float jumpSpeedRatio = 0.25f; // 上昇・下降に使う割合（例：全体の25%）
    public float jumpHeight = 2.0f;
    [Header("ジャンプタイプ")]
    public JumpType currentJumpType = JumpType.Normal;
    private bool isJumping = false;
    //private bool isMoreJumping = false;
    private bool spacePressed = false; // 着地時のスペース判定用
    private float groundY = 0;
    [Header("移動")]
    public float moveSpeed = 2.0f;
    public int score = 0;
    void Start()
    {
        beatInterval = 60f / bpm;  // 1拍の秒数（例：120BPMなら0.5秒）
        groundY = transform.position.y;
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.position += Vector3.right * Time.deltaTime * moveSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position += Vector3.left * Time.deltaTime * moveSpeed;
        }
        // デバッグ用ジャンプ切り替え（キー入力）
        if (Input.GetKeyDown(KeyCode.A)) currentJumpType = JumpType.Normal;
        if (Input.GetKeyDown(KeyCode.S)) currentJumpType = JumpType.High;
        if (Input.GetKeyDown(KeyCode.D)) currentJumpType = JumpType.Super;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            spacePressed = true;
        }
        if (!isJumping)
        {
            JumpByTween();
        }
    }
    float GetJumpHeight()
    {
        switch (currentJumpType)
        {
            case JumpType.Normal: return normalJumpHeight;
            case JumpType.High: return highJumpHeight;
            case JumpType.Super: return superJumpHeight;
            case JumpType.Ottoto: return normalJumpHeight;//タカさは同じだけど裏打ちできない
            default: return normalJumpHeight;
        }
    }
    void JumpByTween()
    {
        isJumping = true;
        float baseY = groundY;
        float targetY = baseY + GetJumpHeight();
        float totalDuration = beatInterval * 2f; // 2拍分ジャンプに使う
        float moveTime = totalDuration * jumpSpeedRatio;
        float floatTime = totalDuration - moveTime * 2f;
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(targetY, moveTime).SetEase(Ease.OutQuad));
        seq.AppendInterval(floatTime); // 頂点で停止
        seq.Append(transform.DOMoveY(baseY, moveTime).SetEase(Ease.InQuad));
        Debug.Log($"着地 Y: {transform.position.y}");

        seq.OnComplete(() =>
        {
            if (spacePressed)
            {
                switch (currentJumpType)
                {
                    case JumpType.Normal:
                        currentJumpType = JumpType.High;
                        break;
                    case JumpType.High:
                        currentJumpType = JumpType.Super;
                        break;
                    case JumpType.Super:
                        // Superの場合はそのまま
                        break;
                }
            }
            else
            {
                if (currentJumpType == JumpType.High || currentJumpType == JumpType.Super)
                {
                    currentJumpType = JumpType.Normal;
                }
            }
            spacePressed = false;
            isJumping = false;
        });
        seq.Play();
    }
}
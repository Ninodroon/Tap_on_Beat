using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("追従設定")]
    public Transform target; // プレイヤーのTransform
    public float followSpeed = 5f; // 追従速度（滑らかさ）
    public bool smoothFollow = true; // 滑らか追従のON/OFF

    [Header("オフセット")]
    private Vector3 offset = new Vector3(0, 0, -20); // カメラのオフセット位置

    private Vector3 velocity = Vector3.zero; // SmoothDamp用

    void Start()
    {
        // targetが設定されていない場合、DededeJumpを持つオブジェクトを自動検索
        if (target == null)
        {
            DededeJump2 player = FindObjectOfType<DededeJump2>();
            if (player != null)
                target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // X軸のみ追従、Y/Z軸は固定
        Vector3 targetPos = new Vector3(
            target.position.x + offset.x,
            transform.position.y,   // Y固定したいならこれ
            offset.z                // ★固定Z（-10とか）
        );
        if (smoothFollow)
        {
            // 滑らかに追従
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, 1f / followSpeed);
        }
        else
        {
            // 即座に追従
            transform.position = targetPos;
        }
    }

    // デバッグ用：追従対象の変更
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
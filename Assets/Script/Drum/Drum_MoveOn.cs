using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 ドラム
 プレイヤーが触れたら「指定速度で Start → Target → 元位置」に戻る
 */

public class Drum_MoveOn : MonoBehaviour
{
    public Transform targetPos;
    Vector3 startPos;

    public float moveSpeed = 3f;
    bool isMoving = false;

    void Start()
    {
        startPos = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isMoving) return;

        if (other.CompareTag("Player"))
        {
            StartCoroutine(MoveSequence());
        }
    }

    IEnumerator MoveSequence()
    {
        isMoving = true;
        //Debug.Log("Drum_MoveOn: 動いてる", this);
        // 行く
        while (Vector3.Distance(transform.position, targetPos.position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 戻る
        while (Vector3.Distance(transform.position, startPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        isMoving = false;
    }
}

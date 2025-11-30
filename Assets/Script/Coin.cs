using UnityEngine;
using DG.Tweening;
using System;

public class Coin : MonoBehaviour
{
    [Header("コイン設定")]
    public int value = 100;
    public float bobHeight = 0.2f;
    public float bobSpeed = 1f;

    [Header("エフェクト")]
    public GameObject collectEffect;

    private Vector3 startPos;

    void Start() => startPos = transform.position;

    void OnEnable() => AdxMarkerBroadcaster.OnMarkerReceived += OnMarkerReceived;
    void OnDisable() => AdxMarkerBroadcaster.OnMarkerReceived -= OnMarkerReceived;

    void OnMarkerReceived(string tag)
    {
        if (tag == "JUMP")
            bobHeight = 0.2f;
        else if (tag == "BACK")
            bobHeight = 0.1f;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        transform.Rotate(0, 0, 1);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectEffect != null)
                Instantiate(collectEffect, transform.position, Quaternion.identity);

            transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
            Destroy(gameObject, 0.3f);
        }
    }

    void OnDestroy() => transform.DOKill();
}
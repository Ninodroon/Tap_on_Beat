using UnityEngine;
using CriWare;
using System;

public class AdxMarkerBroadcaster : MonoBehaviour
{
    // 「マーカーが来たら通知する」ためのイベント
    public static event Action<string> OnMarkerReceived;

    private void OnEnable()
    {
        CriAtomExSequencer.OnCallback += HandleMarker;
    }

    private void OnDisable()
    {
        // 無効化時に解除（安全）
        CriAtomExSequencer.OnCallback -= HandleMarker;
    }

    private void HandleMarker(ref CriAtomExSequencer.CriAtomExSequenceEventInfo info)
    {
        if (info.tag !=null)
        {
            string tag = info.tag; // AtomCraftでつけた名前
            //Debug.Log($"AdxMarkerBroadcaster マーカーr: {tag}");
            OnMarkerReceived?.Invoke(tag); // ← ここで全体に「マーカー来たよ！」と発信
        }
    }
}

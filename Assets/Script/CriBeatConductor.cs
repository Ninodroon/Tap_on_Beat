using UnityEngine;
using DG.Tweening;
using CriWare;
using System;

public class CriBeatConductor : MonoBehaviour
{
    [Header("再生する音源")]
    public CriAtomSource music; // ここにBGM用の CriAtomSource を割り当て（BeatSync入りのCue）

    public static event Action<CriAtomExBeatSync.Info> OnBeat; // 外部に拍通知

    void OnEnable()
    {
        // 推奨API
        CriAtomExBeatSync.OnCallback += HandleBeat;
        // （古いサンプル互換）CriAtomExBeatSync.SetCallback(HandleBeat);
    }

    void OnDisable()
    {
        CriAtomExBeatSync.OnCallback -= HandleBeat;
    }

    void Start()
    {
        if (music != null && music.status != CriAtomSource.Status.Playing)
        {
            music.Play();
        }


    }

    private void HandleBeat(ref CriAtomExBeatSync.Info info)
    {
        // info には bpm, beatCount, barCount, numBeats, beatProgress などが入る
        OnBeat?.Invoke(info);
        // ここで必要ならデバッグ:
         Debug.Log($"BEAT bar:{info.barCount} beat:{info.beatCount}/{info.numBeats} bpm:{info.bpm} prog:{info.beatProgress:0.00}");
    }
}

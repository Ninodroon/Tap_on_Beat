using UnityEngine;
using System;
using System.Collections.Concurrent;
using CriWare;

public class CriCueBus : MonoBehaviour
{
    [Header("この音だけ監視する")]
    public CriAtomSource music;

    public struct CueEvent
    {
        public uint playbackId;
        public string tag;   // "JUMP" / "BACK" / ほか
        public float time;   // 受信時の Unity 時刻
    }

    public static event Action<CueEvent> OnAnyCue;
    public static event Action<CueEvent> OnJumpCue;
    public static event Action<CueEvent> OnBackCue;

    uint _watchPlaybackId = 0xFFFFFFFF;
    readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();

    void Start()
    {
        // この再生だけを監視
        var pb = music.Play();
        _watchPlaybackId = pb.id;

        // 文字列版のイベントコールバックを使う（バージョン差の影響を受けない）
        CriAtomExSequencer.SetEventCallback(OnSequencerString);
    }

    void OnDestroy()
    {
        CriAtomExSequencer.SetEventCallback(null);
    }

    void Update()
    {
        while (_queue.TryDequeue(out var a)) a(); // メインスレッドで実行
    }

    // param 例:
    // "12345\t0\t314159265\tMarker\tJUMP"
    //  [0]=position(ms) [1]=callbackId [2]=playbackId [3]=type [4]=tag
    void OnSequencerString(string param)
    {
        if (string.IsNullOrEmpty(param)) return;
        var p = param.Split('\t');
        if (p.Length < 5) return;

        if (!uint.TryParse(p[2], out var pbId)) return;           // playbackId
        if (pbId != _watchPlaybackId) return;                      // 他の再生は無視

        var tag = (p[4] ?? "").Trim().ToUpperInvariant();         // "JUMP"/"BACK"/...
        var ev = new CueEvent { playbackId = pbId, tag = tag, time = Time.time };

        _queue.Enqueue(() => {
            OnAnyCue?.Invoke(ev);
            if (tag == "JUMP") OnJumpCue?.Invoke(ev);
            else if (tag == "BACK") OnBackCue?.Invoke(ev);
        });
    }
}

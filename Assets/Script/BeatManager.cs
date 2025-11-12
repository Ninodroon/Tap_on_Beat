using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using System.Threading;

public class BeatManager : MonoBehaviour
{
    public float bpm = 120f;
    private float beatInterval;

    // 拍が来たら通知するイベント
    public event Action OnBeat;

    private CancellationTokenSource cts; //シーン切り替えやオブジェクト破棄時に cts.Cancel() する

    void Start()
    {
        beatInterval = 60f / bpm;
        cts = new CancellationTokenSource();
        StartBeatLoop(cts.Token).Forget();
    }

    private async UniTaskVoid StartBeatLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            OnBeat?.Invoke(); // 拍の通知
            await UniTask.Delay(System.TimeSpan.FromSeconds(beatInterval), cancellationToken: token);
        }
    }

    private void OnDestroy()
    {
        cts?.Cancel();
    }
}

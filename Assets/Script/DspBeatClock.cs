using UnityEngine;
using System;

public sealed class DspBeatClock : MonoBehaviour
{
    [Range(40, 300)] public double bpm = 120.0;
    [SerializeField] AudioSource music;   // 任意。曲なしメトロノームならnullでOK
    public event Action<int, double> OnBeat;  // (beatIndex, beatDspTime)
    public event Action<int, int, double> OnSub; // (i, div, dspTime)

    double _secPerBeat;
    double _startDsp;      // 小節の基準（曲頭）
    int _emittedBeat = -1; // 最後に発火したビート番号

    void Start()
    {
        _secPerBeat = 60.0 / bpm;

        // 1) 曲あり：PlayScheduledで開始を予約→そのDSPを基準にする
        if (music && music.clip)
        {
            _startDsp = AudioSettings.dspTime + 0.10; // 100ms先に開始
            music.PlayScheduled(_startDsp);
        }
        else
        {
            // 2) 曲なし：今を基準に即開始
            _startDsp = AudioSettings.dspTime;
        }
    }

    void Update()
    {
        var dsp = AudioSettings.dspTime;

        // 現在“何拍目まで到達しているか”を絶対計算（積み上げではないのでドリフトしにくい）
        int curBeat = (int)Math.Floor((dsp - _startDsp) / _secPerBeat);
        while (_emittedBeat < curBeat)
        {
            _emittedBeat++;
            double beatTime = _startDsp + _emittedBeat * _secPerBeat;
            OnBeat?.Invoke(_emittedBeat, beatTime);

            // 必要ならサブディビジョン（例：1/4）
            int div = 4;
            double step = _secPerBeat / div;
            for (int i = 0; i < div; i++)
                OnSub?.Invoke(i, div, beatTime + i * step);
        }
    }

    // ===== ユーティリティ =====

    public double SecPerBeat => _secPerBeat;
    public double NowDsp => AudioSettings.dspTime;

    public double BeatIndexToDsp(int beatIndex) => _startDsp + beatIndex * _secPerBeat;

    public double NearestBeatDsp(double dspTime)
    {
        var k = Math.Round((dspTime - _startDsp) / _secPerBeat);
        return _startDsp + k * _secPerBeat;
    }

    // 再生中にBPM変更（直近の拍にスナップして継続）
    public void SetBpm(double newBpm)
    {
        var dsp = AudioSettings.dspTime;
        var nearest = NearestBeatDsp(dsp);
        bpm = newBpm;
        _secPerBeat = 60.0 / bpm;

        // 新BPMでも位相を維持するよう基準を再配置
        // 直近の拍が今dspと一致するよう_startDspをずらす
        // nearest = _startDsp' + n * secPerBeat' となるよう調整
        // nは現在の発火済みビート番号_emittedBeat
        _startDsp = nearest - _emittedBeat * _secPerBeat;
    }

    // 一時停止/再開やシークに強い基準再計算
    public void ReanchorByAudioSource()
    {
        if (!(music && music.clip)) return;
        int sr = music.clip.frequency;
        double playSec = (double)music.timeSamples / sr;
        // 「いまの曲の先頭=何Dspだったか」を再推定
        _startDsp = AudioSettings.dspTime - playSec;
        // 発火済みビート番号も追従
        _emittedBeat = (int)Math.Floor(playSec / _secPerBeat);
    }
}

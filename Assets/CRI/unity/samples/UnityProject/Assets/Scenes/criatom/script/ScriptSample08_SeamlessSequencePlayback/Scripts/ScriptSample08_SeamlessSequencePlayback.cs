/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

/**
 * 本サンプルは、連結再生を利用しています。
 *
 * 「Play Cue」ボタンを押下すると、指定されたCueを連結再生します。
 * 「Play Data」ボタンを押下するとStart関数内で生成したサイン波を連結再生します。
 * 本サンプルではそれぞれの再生フォーマットに対応したCriAtomExPlayerで再生しています。
 *
 */

#if !UNITY_WEBGL
#define SUPPORT_RAWPCM_VOICEPOOL
#endif

using UnityEngine;
using CriWare;
using System.Collections;
using System.Runtime.InteropServices;


public class ScriptSample08_SeamlessSequencePlayback : MonoBehaviour
{
#region Variables

	[SerializeField] private string cueName = string.Empty;

	private const int numEntries = 8;

	private CriAtomExPlayer player;

#if SUPPORT_RAWPCM_VOICEPOOL
	private CriAtomExPlayer playerForRawPcm;
	private CriAtomExVoicePool rawpcmVoicePool;

	/* 再生用データ */
	private float samplingRate = 48000.0f;
	private float[][] playbackData;
	private GCHandle[] gcHandle;
	private int lastEntriedIndex = 0;
#endif
	#endregion

	#region Functions

	void Start()
	{
		/* キュー再生用プレーヤを作成 */
		player = new CriAtomExPlayer();

		/* エントリプールの作成 */
		player.PrepareEntryPool(numEntries, true);

#if SUPPORT_RAWPCM_VOICEPOOL
		/* RawPCM 再生用プレーヤを作成 */
		playerForRawPcm = new CriAtomExPlayer();
		/* RawPcm ボイスプールを作成 */
		/* 注意：作成時に指定したフォーマットが再生時に適用される。 */
		rawpcmVoicePool = new CriAtomExRawPcmVoicePool(1, 2, (int)samplingRate, CriAtomExRawPcmVoicePool.RawPcmFormat.Float32, 1234u);
		/* ボイスプール識別子をプレーヤに登録 */
		playerForRawPcm.SetVoicePoolIdentifier(rawpcmVoicePool.identifier);

		/* エントリプールの作成 */
		playerForRawPcm.PrepareEntryPool(numEntries, true);

		/* 再生用データを用意 */
		playbackData = new float[numEntries][];
		gcHandle = new GCHandle[numEntries];

		/* 各データ長(ここでは1/30秒分) */
		int dataLength = (int)(samplingRate / 30);

		/* 末端から先頭に戻る際に途切れないようにするため周波数を調整 */
		float frequency = samplingRate / numEntries / 2;

		int t = 0;
		const float amplitude = 0.7080f;
		for (int i = 0; i < numEntries; i++) {
			playbackData[i] = new float[dataLength * 2];

			for (int j = 0; j < dataLength; j++) {
				/* 2ch の 3kHz サイン波 */
				float value = Mathf.Sin(frequency * t * Mathf.PI / samplingRate) * amplitude;
				playbackData[i][2 * j] = playbackData[i][2 * j + 1] = value;
				t++;
			}
			/* メモリ固定 */
			/* プレーヤに対して SetData を行ったあとに GC によるメモリ移動が行われるとアクセス違反が発生するため。 */
			gcHandle[i] = GCHandle.Alloc(playbackData[i], GCHandleType.Pinned);
		}
#endif
	}


	/* キューの連結再生を開始 */
	public void PlayCue() {
		/* 注意：Stop 呼び出しでエントリプールはクリアされる。 */
		player.Stop();

		/* 先頭キューをセット */
		player.SetCue(null, cueName);

		/* 最初の連結再生用キューを入力 */
		/* 注意：ダブルバッファで動作するため、Start 呼び出し後に即座に次のデータが要求されるので */
		/* 入力しておく必要がある。 */
		EntryCue();

		player.Start();
	}


	/* キューの入力 */
	private bool EntryCue() {
		if (player.EntryCue(null, cueName, false)) {
			return true;
		}

		/* 入力失敗(エントリプールが満杯) */
		return false;
	}


#if SUPPORT_RAWPCM_VOICEPOOL
	/* RawPCMデータの再生を開始 */
	public void PlayData() {
		/* 注意：Stop 呼び出しでエントリプールはクリアされる。 */
		playerForRawPcm.Stop();

		/* 再生するデータのフォーマットを RawPCM に指定 */
		playerForRawPcm.SetFormat(CriAtomEx.Format.RAW_PCM);

		/* 先頭データをセット */
		/* 注意：再生開始時の先頭データは通常の SetData 等の関数で設定する必要がある。 */
		playerForRawPcm.SetData(gcHandle[0].AddrOfPinnedObject(), playbackData[0].Length * sizeof(float));
		lastEntriedIndex = 0;

		/* 最初の連結再生用データを入力 */
		/* 注意：ダブルバッファで動作するため、Start 呼び出し後に即座に次のバッファが要求されるので */
		/* 入力しておく必要がある。 */
		/* 入力しない場合、先頭データの再生終了後に停止、または音途切れが発生する。 */
		/* 再生開始時は余裕をもってデータを入力しておくことで意図しない停止/音切れを回避することができる。 */
		while (playerForRawPcm.GetNumEntries() < 2) {
			EntryData();
		}

		playerForRawPcm.Start();
	}


	/* RawPCMデータの入力 */
	private bool EntryData()
	{
		int indexToEntry = (lastEntriedIndex + 1) % numEntries;
		if (playerForRawPcm.EntryData(gcHandle[indexToEntry].AddrOfPinnedObject(),
										playbackData[indexToEntry].Length * sizeof(float),
										false)) {
			lastEntriedIndex = indexToEntry;
			return true;
		}

		/* 入力失敗(エントリプールが満杯) */
		return false;
	}
#endif


	private void Update()
	{
		if (player.GetStatus() == CriAtomExPlayer.Status.Playing) {
			/* 注意：入力したキューの長さにもよるが、キュー再生の場合は */
			/* データ長に余裕があるので、エントリが空になったら追加すればよい。 */
			if (player.GetNumEntries() < 1)
				EntryCue();
		}

#if SUPPORT_RAWPCM_VOICEPOOL
		if (playerForRawPcm.GetStatus() == CriAtomExPlayer.Status.Playing) {
			/* 注意：本サンプルでは1/30秒分のデータを入力しているので、フレームレートが30に */
			/* 達していない場合は再生中の停止や音途切れの要因となる。 */
			/* サーバ処理の周期によっては一度に二つのバッファが消費される場合があるため、 */
			/* 余裕をもって入力しておく。 */
			while (playerForRawPcm.GetNumEntries() < 2)
				EntryData();
		}
#endif
	}


	void OnGUI()
	{
		if (Scene_00_SampleList.ShowList == true) {
			return;
		}

		if (Camera.main == null) {
			return;
		}

#if UNITY_WEBGL
		if (CriAtom.CueSheetsAreLoading) {
			return;
		}
#endif

		Scene_00_GUI.BeginGui("01/SampleMain2");

		/* Set UI skin. */
		GUI.skin = Scene_00_SampleList.uiSkin;

		var pos = Scene_00_GUI.ScreenPos2UIPos(Camera.main.WorldToScreenPoint(transform.position));
		pos.y = Scene_00_GUI.screenY - pos.y;

		CriAtomExPlayer.Status status;
		bool sw;

		status = player.GetStatus();
		sw = (status == CriAtomExPlayer.Status.Playing) ? false : true;
		if (Scene_00_GUI.Button(new Rect(pos.x - 300, pos.y, 150, 150), sw ? "Play Cue" : "Stop")) {
			if (sw) {
				PlayCue();
			} else {
				player.Stop();
			}
		}

#if SUPPORT_RAWPCM_VOICEPOOL
		status = playerForRawPcm.GetStatus();
		sw = (status == CriAtomExPlayer.Status.Playing) ? false : true;
		if (Scene_00_GUI.Button(new Rect(pos.x + 150, pos.y, 150, 150), sw ? "Play Data" : "Stop")) {
			if (sw) {
				PlayData();
			} else {
				playerForRawPcm.Stop();
			}
		}
#endif


		Scene_00_GUI.EndGui();
	}

#endregion
}

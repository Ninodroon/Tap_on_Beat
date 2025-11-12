/****************************************************************************
 *
 * Copyright (c) 2015 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

/**
 * 本サンプルは、このアプリを実行中のAndroid端末の音声再生遅延時間を推測するサンプルです。
 * 本サンプルは、以下のGUIボタンで操作できます。
 * "Initialize Estimator"：遅延推測を開始します。
 * "Finalize Estimator"：遅延推測を終了します
 * 遅延推測の処理状況は"Estimator Status"、遅延推測値は画面上の"Estimated Latency"として表示されます。
 * [注意] 遅延推測値はAtomライブラリの初期化設定(サーバ周波数、サウンドバッファリング時間)の値によって変化します。
 * 本サンプルは、Androidでの動作を想定しています。
 */
/**
 * This sample estimates the sound playback latency on the Android while an application is being executed.
 * It is controlled by the following GUI buttons:
 *   "Initialize Estimator" : to start the latency estimation
 *   "Finalize Estimator"   : to end the latency estimation
 * The processing status is displayed as "Estimator Status" on the screen.
 * [Note]
 * The estimation value of the latency changes depending on the initialization settings(server freq, sound buffering time).
 * This sample assumes the system is Android.
 */

using UnityEngine;
using CriWare;
using System.Collections;
using System.Runtime.InteropServices;

public class Scene_05_EstimateSoundLatency : MonoBehaviour
{

	#region Variables
	private CriAtomExLatencyEstimator.EstimatorInfo info;
    private bool is_initialized = false;
    #endregion

	#region Functions

	void Update()
	{
        info = CriAtomExLatencyEstimator.GetCurrentInfo();
	}

    void OnDestroy() {
        if (is_initialized) {
            is_initialized = false;
            // End the latency estimation.
            CriAtomExLatencyEstimator.FinalizeModule();

        }
    }

	void OnGUI()
	{
		if (Scene_00_SampleList.ShowList == true) {
			/* Disables GUI operations while displaying a list of samples. */
			return;
		}

		Scene_00_GUI.BeginGui("01/SampleMain");

		/* Set UI skin. */
		GUI.skin = Scene_00_SampleList.uiSkin;

		/* Display GUI, and control the sound playback. */
		GUILayoutOption[] option = new GUILayoutOption[]{ GUILayout.Height(28) };
		GUILayout.BeginArea(new Rect(42, 128, 480, 200));
		{
			GUILayout.Label("Estimator Status       : " + info.status);
			GUILayout.Space(16);
            GUILayout.Label("Estimated Latency[msec]: " + info.estimated_latency);
			GUILayout.Space(16);

            /* If a button is clicked, the corresponding sound is played. */
			if (Scene_00_GUI.Button("Initialize Estimator", option)) {
                if (!is_initialized) {
                    // Start the latency estimation.
                    CriAtomExLatencyEstimator.InitializeModule();
                    is_initialized = true;
                }
			}
			if (Scene_00_GUI.Button("Finalize Estimator", option)) {
                if (is_initialized) {
                    // End the latency estimation.
                    CriAtomExLatencyEstimator.FinalizeModule();
                    is_initialized = false;
                }
            }
        }
		GUILayout.EndArea();

		Scene_00_GUI.EndGui();
	}
	#endregion
}

/* end of file */

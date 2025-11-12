/****************************************************************************
 *
 * Copyright (c) 2018 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using CriWare;

public class InputCapture : MonoBehaviour
{
	#region Variables
	CriAtomExMic mic;
	bool isRecording;
	float peakLevel = 0;
	float[] micdata = new float[512];

	LineRenderer lineRenderer;
	Vector3[] linePositions = new Vector3[512];
	const float lineAreaWidth = 16.0f;
	const float lineAreaHeight = 6.0f;
	#endregion

	#region Functions
	void Start()
	{
		// Setup line renderer
		lineRenderer = gameObject.GetComponent<LineRenderer>();
		lineRenderer.startWidth = 0.05f;
		lineRenderer.startWidth = 0.05f;
		lineRenderer.positionCount = micdata.Length;

#if UNITY_2018_3_OR_NEWER && !UNITY_EDITOR && UNITY_ANDROID
		if (UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone) == false) {
			UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
		}
#endif

		// Initialize microphone module
		CriAtomExMic.InitializeModule();

		// Get default audio input device
		var defaultDevice = CriAtomExMic.GetDefaultDevice();
		if (!defaultDevice.HasValue) {
			Debug.Log("Audio input device not found.");
		}

		// Enumerate all audio input defices
		foreach (var device in CriAtomExMic.GetDevices()) {
			bool isDefault = (device.deviceId == defaultDevice.Value.deviceId);
			Debug.Log(device.deviceName + (isDefault ? " (default)" : ""));
		}
	}

	void OnDestroy()
	{
		if (mic != null) {
			mic.Dispose();
		}

		// Finalize microphone module
		CriAtomExMic.FinalizeModule();
	}

	void StartMic()
	{
		CriAtomExMic.Config config = CriAtomExMic.Config.Default;
		config.frameSize = (uint)micdata.Length;

		// Create microphone instance
		mic = CriAtomExMic.Create(config);
		if (mic != null) {
			mic.Start();
			isRecording = true;
		}
	}

	void StopMic()
	{
		// Destroy microphone instance
		if (mic != null) {
			mic.Stop();
			mic.Dispose();
			mic = null;
		}

		isRecording = false;
	}

	void Update()
	{
		if (mic == null) {
			return;
		}

		while (true) {
			// Get audio input data
			uint numSamples = mic.ReadData(micdata);
			if (numSamples == 0) {
				break;
			}

			// Analyze peak level
			float maxValue = 0;
			for (uint i = 0; i < numSamples; i++) {
				float s = micdata[i];
				float mag = Mathf.Sqrt(s * s);
				if (mag > maxValue) {
					maxValue = mag;
				}
			}
			peakLevel = maxValue;
		}

		// Set line vertices from sound wave.
		for (int i = 0; i < micdata.Length; i++) {
			linePositions[i] = new Vector3(
				(float)i / (float)micdata.Length * lineAreaWidth - lineAreaWidth / 2.0f,
				micdata[i] * lineAreaHeight / 2.0f, 0.0f);
		}
		lineRenderer.SetPositions(linePositions);
	}

	void OnGUI()
	{
		if (Scene_00_SampleList.ShowList == true) {
			return;
		}

		Scene_00_GUI.BeginGui("01/SampleMain");

		GUI.skin = Scene_00_SampleList.uiSkin;

		GUILayout.BeginArea(new Rect(12, 70, 320, 140), "", Scene_00_SampleList.TextStyle);

		if (isRecording) {
			if (Scene_00_GUI.Button("Stop Mic")) {
				StopMic();
			}
		} else {
			if (Scene_00_GUI.Button("Start Mic")) {
				StartMic();
			}
		}

		GUILayout.HorizontalSlider(peakLevel, 0.0f, 1.0f);

		GUILayout.EndArea();

		Scene_00_GUI.EndGui();
	}
	#endregion
}

/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using CriWare;
using System.Collections;

public class WaveMeter : MonoBehaviour {
	public CriAtomSource source;
	public int      monitoredChannelId = 0;

	private GameObject[] meterCubes;
	private Material cubeMaterial = null;
	private const int numCapturedSamples = 512;
	private CriAtomExOutputAnalyzer analyzer;
	private float [] pcmData;

	const float minX = -15.0f;
	const float maxX = 15.0f;
	const float maxY = 7.5f;


	IEnumerator Start() {
		// Wait for Loading ACB...
		while (CriAtom.CueSheetsAreLoading) {
			yield return null;
		}

		/* Initialize CriAtomExOutputAnalyzer for PCM capture. */
		CriAtomExOutputAnalyzer.Config config = new CriAtomExOutputAnalyzer.Config();
		config.enablePcmCapture = true;
		config.numCapturedPcmSamples = numCapturedSamples;
		analyzer = new CriAtomExOutputAnalyzer(config);

		/* Set Output Analyzer for CriAtomExPlayer.  */
		analyzer.AttachExPlayer(source.player);

		/* Initialize Cubes for displaying wave. */
		Vector3 position = new Vector3(0.0f, 0.0f, 0.0f);
		Vector3 scale = new Vector3((maxX - minX) / numCapturedSamples, 0.1f, 0.1f);
		meterCubes = new GameObject[numCapturedSamples];
		cubeMaterial = new Material(Shader.Find("Diffuse"));
		for (int i = 0; i < numCapturedSamples; i++) {
			meterCubes[i] = GameObject.CreatePrimitive(PrimitiveType.Quad);
			meterCubes[i].transform.parent = this.transform;
			meterCubes[i].transform.localScale = scale;
			position.x = minX + (maxX - minX) / numCapturedSamples * i;
			meterCubes[i].transform.position = position;
			meterCubes[i].GetComponent<Renderer>().material = cubeMaterial;
		}

		/* Initialize float array for receiving pcm data */
		pcmData = new float[numCapturedSamples];

		/* Start playback */
		source.Play();
	}

	void OnDestroy() {
		if (analyzer != null) {
			analyzer.DetachExPlayer();
			analyzer.Dispose();
		}
		DestroyImmediate(cubeMaterial);
	}

	void Update() {
		if (analyzer != null) {
			/* Get PCM output data from CriAtomExAnalyzer. */
			analyzer.GetPcmData(ref pcmData, monitoredChannelId);

			/* Update cubes based on PCM values. */
			Vector3 position;
			Color color = new Color(1.0f, 1.0f, 1.0f);
			for (int i = 0; i < numCapturedSamples; i++)
			{
				/* Set cube's Y position based on PCM value */
				position = meterCubes[i].transform.position;
				position.y = maxY * pcmData[i];
				meterCubes[i].transform.position = position;
				/* Set cube's color */
				color.g = Mathf.Max(0.0f, 1.0f - Mathf.Abs(pcmData[i]) * 5.0f);
				color.b = Mathf.Max(0.0f, Mathf.Abs(pcmData[i]) * 2.5f);
				meterCubes[i].GetComponent<Renderer>().material.color = color;
			}
		}
	}
}

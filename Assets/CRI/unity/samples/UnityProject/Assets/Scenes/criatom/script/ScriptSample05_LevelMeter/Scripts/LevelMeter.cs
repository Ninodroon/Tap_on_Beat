/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using CriWare;
using System.Collections;

public class LevelMeter : MonoBehaviour {

	public int      monitoredChannelId = 0;

	private float   objScaleBaseVal = 2.0f;

	/* This "Start()" method is called before "Update()".*/
	IEnumerator Start() {
		// Wait for Loading ACB...
		while (CriAtom.CueSheetsAreLoading) {
			yield return null;
		}

		CriAtom.AttachDspBusSetting("DspBusSetting_0");
		/* Set Bus Analayzer to use "BusAnalyzeInfo". */
		CriAtom.SetBusAnalyzer(true);
	}

	/* Update the local scale value of GameObject. */
	void Update() {
		/* Get BusAnalyzerInfo from a DSP Buss. */
		CriAtomExAsr.BusAnalyzerInfo lBusInfo = CriAtom.GetBusAnalyzerInfo("MasterOut");

		/* Calculate new value of GameObject scale. */
		//float lObjScaleNewVal = 0.1f + objScaleBaseVal * lBusInfo.rmsLevels[monitoredChannelId];
		float lObjScaleNewVal = 0.1f + objScaleBaseVal * lBusInfo.peakLevels[monitoredChannelId];
		Debug.Log("Channel_" + monitoredChannelId + " : " + lBusInfo.peakLevels[monitoredChannelId]);

		/* Update local scale of 'this' game object. */
		this.transform.localScale = new Vector3(lObjScaleNewVal, lObjScaleNewVal, lObjScaleNewVal);
	}
}

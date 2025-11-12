/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using CriWare;
using System.Collections.Generic;

public class SoundTest : MonoBehaviour {
	static private readonly string cueSheetName = "PinballMain";
	[SerializeField] private bool soundDebug = true;

	private CriAtomSource atomSourceSe;
	private CriAtomEx.CueInfo[] cueInfoList;
	private List<string> cueNameList = new List<string>();

	void InitCueInfoList()
	{
		atomSourceSe = gameObject.AddComponent<CriAtomSource>();
		atomSourceSe.cueSheet = cueSheetName;
		CriAtomExAcb acb = CriAtom.GetAcb(cueSheetName);
		cueInfoList = acb.GetCueInfoList();
		foreach (CriAtomEx.CueInfo cueInfo in cueInfoList) {
			cueNameList.Add(cueInfo.name);
		}
		CriAtomEx.AttachDspBusSetting("DspBusSetting_0");
		CriAtom.SetBusAnalyzer(true); // Enable the level meter
	}

	void Start ()
	{
#if !UNITY_WEBGL
		InitCueInfoList();
#endif
	}

	void OnGUI()
	{
		if (Scene_00_SampleList.ShowList == true) {
			return;
		}

#if UNITY_WEBGL
		if (CriAtom.CueSheetsAreLoading) {
			return;
		} else if (cueInfoList == null) {
			InitCueInfoList();
		}
#endif

		Scene_00_GUI.BeginGui("01/SampleMain");

		/* Set UI skin. */
		GUI.skin = Scene_00_SampleList.uiSkin;

		if (soundDebug) {
			var anchor = new Vector2(Scene_00_GUI.screenX - 400, 100);
			for (int i = 0; i < cueInfoList.Length; ++i) {
				var buttonRect = new Rect(anchor.x + 180 * (i % 2), anchor.y + 80 * (i / 2), 170, 70);
				if (Scene_00_GUI.Button(buttonRect, cueInfoList[i].name)){
					atomSourceSe.Stop();
					atomSourceSe.Play(cueInfoList[i].name);
				}
			}
		}

		Scene_00_GUI.EndGui();
	}
}

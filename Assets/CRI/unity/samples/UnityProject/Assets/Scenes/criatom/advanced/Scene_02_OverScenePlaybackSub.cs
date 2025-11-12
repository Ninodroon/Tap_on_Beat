/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

/**
 * 本サンプルは、複数のシーンを超えたACBファイルを扱い、
 * シーンを切り替えても途切れないBGM再生を実現しているサンプルです。
 * 表示されているオブジェクトをクリックすることで
 * 以下の２つのシーンを切り替えます。
 * ・Scene_02_OverScenePlayback     （メインシーン）
 * ・Scene_02_OverScenePlaybackSub  （サブシーン）
 */
/**
 * This sample switches scenes without interrupting the BGM by handling the ACB
 * file that has multiple scenes.
 * Click the object displayed to switch between the following scenes:
 * ・Scene_02_OverScenePlayback     (main scene)
 * ・Scene_02_OverScenePlaybackSub  (sub scene)
 */

using UnityEngine;
using CriWare;
using System.Collections;

public class Scene_02_OverScenePlaybackSub : MonoBehaviour
{
	#region Variables
	/* target object to click */
	private GameObject selectedGameObject;
	#endregion

	#region Functions
	void OnGUI()
	{
#if UNITY_WEBGL
		if (CriAtom.CueSheetsAreLoading) {
			return;
		}
#endif

		if (Scene_00_SampleList.ShowList == true) {
			/* Disable GUI operations while displaying a list of samples to be switched. */
			return;
		}

		Scene_00_GUI.BeginGui("01/SampleMain");

		/* Set UI skin. */
		GUI.skin = Scene_00_SampleList.uiSkin;

		GUILayout.BeginArea(new Rect(100, 100, 200, 200), "");
		if (Scene_00_GUI.Button("Push to\nChange Scene", GUILayout.Width(200), GUILayout.Height(200))) {
#if UNITY_5_3_OR_NEWER
			UnityEngine.SceneManagement.SceneManager.LoadScene("Scene_02_OverScenePlayback");
#else
			Application.LoadLevel("Scene_02_OverScenePlayback");
#endif
		}
		GUILayout.EndArea();

		Scene_00_GUI.EndGui();
	}
	#endregion
}
/* end of file */

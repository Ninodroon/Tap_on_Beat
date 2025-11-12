/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using CriWare;
using System.Collections;

public class ScriptSample03_GameScene : MonoBehaviour {

	private int     SceneMusicCueId = 2;
	private string  nextSceneName   = "ScriptSample03_TitleScene";


	/* Called before the first Update(). */
	void Start () {
		/* Play BGM */
#if !UNITY_WEBGL
		ScriptSample03_SoundManager.PlayCueId(SceneMusicCueId);
#else
		StartCoroutine(ScriptSample03_SoundManager.PlayCueIdAsync(SceneMusicCueId));
#endif
	}

	/* Show and control the scene-switching GUI. */
	void OnGUI(){
		if (Scene_00_SampleList.ShowList == true) {
			return;
		}

		/* Set UI skin. */
		GUI.skin = Scene_00_SampleList.uiSkin;

		Scene_00_GUI.BeginGui("01/SampleMain");
		if (Scene_00_GUI.Button(new Rect(Scene_00_GUI.screenX-250,200,150,150), "change\nscene")) {
#if UNITY_5_3_OR_NEWER
			UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
#else
			Application.LoadLevel(nextSceneName);
#endif
		}
		Scene_00_GUI.EndGui();
	}
}

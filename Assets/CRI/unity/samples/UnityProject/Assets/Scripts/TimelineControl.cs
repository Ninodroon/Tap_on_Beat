/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_2018_1_OR_NEWER

using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class TimelineControl : MonoBehaviour {
	private enum Anchor {
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
		Middle
	}

	#region Variables
	[SerializeField] private Rect areaRect;
	[SerializeField] private Anchor anchor = Anchor.TopLeft;
	[SerializeField] private float uiMarginOutter = 10f;
	[SerializeField] private float uiMarginInner = 5f;
	[SerializeField] private bool isHorizontal = false;

	private PlayableDirector director;

	private Rect anchoredRect;
	private Rect playButtonRect;
	private Rect pauseButtonRect;
	private Rect stopButtonRect;
	private float buttonHeight;
	private float buttonWidth;
	#endregion

	void Start () {
		director = GetComponent<PlayableDirector>();

		anchoredRect = areaRect;
		switch (anchor) {
			case Anchor.TopRight:
				anchoredRect.x = Scene_00_GUI.screenX - areaRect.x - areaRect.width;
				break;
			case Anchor.BottomLeft:
				anchoredRect.y = Scene_00_GUI.screenY - areaRect.y - areaRect.height;
				break;
			case Anchor.BottomRight:
				anchoredRect.x = Scene_00_GUI.screenX - areaRect.x - areaRect.width;
				anchoredRect.y = Scene_00_GUI.screenY - areaRect.y - areaRect.height;
				break;
			case Anchor.Middle:
				anchoredRect.x = (Scene_00_GUI.screenX - areaRect.width) / 2f + areaRect.x;
				anchoredRect.y = (Scene_00_GUI.screenY - areaRect.height) / 2f + areaRect.y;
				break;
			case Anchor.TopLeft:
			default:
				break;
		}

		if (isHorizontal) {
			buttonHeight = Mathf.Max(anchoredRect.height - uiMarginOutter * 2, 10f);
			buttonWidth = Mathf.Max((anchoredRect.width - uiMarginOutter * 2 - uiMarginInner * 2) / 3f, 10f);
			playButtonRect = new Rect(uiMarginOutter, uiMarginOutter, buttonWidth, buttonHeight);
			pauseButtonRect = new Rect(uiMarginOutter + buttonWidth + uiMarginInner, uiMarginOutter, buttonWidth, buttonHeight);
			stopButtonRect = new Rect(uiMarginOutter + buttonWidth * 2 + uiMarginInner * 2, uiMarginOutter, buttonWidth, buttonHeight);
		} else {
			buttonHeight = Mathf.Max((anchoredRect.height - uiMarginOutter * 2 - uiMarginInner * 2) / 3f, 10f);
			buttonWidth = Mathf.Max(anchoredRect.width - uiMarginOutter * 2, 10f);
			playButtonRect = new Rect(uiMarginOutter, uiMarginOutter, buttonWidth, buttonHeight);
			pauseButtonRect = new Rect(uiMarginOutter, uiMarginOutter + buttonHeight + uiMarginInner, buttonWidth, buttonHeight);
			stopButtonRect = new Rect(uiMarginOutter, uiMarginOutter + buttonHeight * 2 + uiMarginInner * 2, buttonWidth, buttonHeight);
		}
	}

	void OnGUI ()
	{
		if (Scene_00_SampleList.ShowList == true) {
			return;
		}

		Scene_00_GUI.BeginGui("01/SampleMain");

		GUI.skin = Scene_00_SampleList.uiSkin;

		GUILayout.BeginArea(anchoredRect, Scene_00_SampleList.TextStyle);
		{
			if (Scene_00_GUI.Button(playButtonRect, "Play")) {
				director.Play();
			}

			if (Scene_00_GUI.Button(pauseButtonRect, "Pause")) {
				director.Pause();
			}

			if (Scene_00_GUI.Button(stopButtonRect, "Stop")) {
				director.Stop();
			}
		}
		GUILayout.EndArea();

		Scene_00_GUI.EndGui();
	}
}

#endif
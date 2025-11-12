/****************************************************************************
 *
 * Copyright (c) 2011 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using CriWare;
using System.Collections;

public class HitTarget : MonoBehaviour {

	public int Score = 100;
	bool viewScoreFlag = false;

	SoundManager sm;

	// Use this for initialization
	void Start () {
		sm = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();

	}


	// Update is called once per frame
	void Update ()
	{

	}

	float lastPlaybackTargetTime = 0;

	void OnTriggerEnter ()
	{
		Debug.Log ("Hit ! " + Score.ToString());
		//  test

		GameObject gm = GameObject.FindGameObjectWithTag ("GameMain");
		gm.GetComponent<GameMain> ().AddScore (Score);

		viewScoreFlag = true;
		viewScoreOffset = 0.0f;
		lastChangedTime = Time.timeSinceLevelLoad;
		StartCoroutine (ViewScore ());

		if(lastPlaybackTargetTime+0.25 < Time.timeSinceLevelLoad){
			sm.PlaybackBumper(3);
			lastPlaybackTargetTime = Time.timeSinceLevelLoad;
		}
	}

	float viewScoreOffset = 0.0f;
	float lastChangedTime = 0.0f;
	void OnGUI()
	{
		Scene_00_GUI.BeginGui("01/SampleMain");

		if (viewScoreFlag){

			if(Camera.main){
				//float alphaValue =  10.0f - (Time.timeSinceLevelLoad - lastChangedTime);
				//if(alphaValue > 0)
				{
					GUI.color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
					Vector3 sp = Scene_00_GUI.ScreenPos2UIPos(Camera.main.WorldToScreenPoint(this.transform.position));

					GUI.Label(new Rect( sp.x-8,Scene_00_GUI.screenY-sp.y-10-viewScoreOffset,30,20),Score.ToString());
				}
			}
			viewScoreOffset = 15.0f * (Time.timeSinceLevelLoad - lastChangedTime);
		}

		Scene_00_GUI.EndGui();
	}


	IEnumerator ViewScore ()
	{
		yield return new WaitForSeconds (1);

		viewScoreFlag = false;
	}
}

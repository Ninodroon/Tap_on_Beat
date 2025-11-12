/****************************************************************************
 *
 * Copyright (c) 2020 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

/**
 * 本サンプルは、トランシーバー機能を利用した空間音響表現のサンプルです。
 * 本サンプルは、以下のGUIボタンで操作できます。
 * "Outside"：カメラ（リスナー付き）が部屋から外に移動します。
 * "Inside"：カメラが現在位置から初期位置に戻ります。
 * ボタンを押し、カメラが移動すると同時に、リスナーから聞こえる音の方向性が音源やトランシーバーとの位置関係によって変わります。
 */
/**
 * This sample shows how to implement spatial acoustics using the transceiver function.
 * It can be controlled by the following GUI buttons:
 *   "Outside" : to move the camera (along with the listener) outside of the room
 *   "Inside"  : to move the camera to its initial position inside the room
 * As the camera moves, the direction of the sound heard by the listener will change depending
 * on the positional relationship between the transceiver (or the sound source) and the listener.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CriWare;
using UnityEngine.UI;

public class Scene_07_Transceiver : MonoBehaviour
{
	public CriAtomRegion regionIn;
	public CriAtomRegion regionOut;
	public CriAtomTransceiver transceiver;
	public CriAtomListener listener;

	/* 3D regions of the sound sources are set in their corresponding inspectors. */

	public Camera overviewCamera;
	public RawImage overviewTarget;
	public Transform waypointInside;
	public Transform waypoinyOutside;

	private RenderTexture overviewTex;
	private Vector3 currentPos;
	private Vector3 nextPos;
	private float totalTime = 0;
	private float currentTime = 0;

	private void Start() {
		overviewTex = new RenderTexture((int)(overviewTarget.rectTransform.sizeDelta.x * 5), (int)(overviewTarget.rectTransform.sizeDelta.y * 5), 24);
		overviewCamera.targetTexture = overviewTex;
		overviewTarget.texture = overviewTex;
		overviewCamera.enabled = true;
	}

	private void OnDestroy() {
		if (this.overviewCamera != null) {
			this.overviewCamera.targetTexture = null;
		}
		if (this.overviewTarget != null) {
			this.overviewTarget.texture = null;
		}
		Destroy(this.overviewTex);
		this.overviewTex = null;
	}

	/* Entering the room */
	private void OnTriggerEnter(Collider other) {
		var listener = other.GetComponent<CriAtomListener>();
		if (listener != null) {
			/* the listener is inside the room */
			listener.region3d = regionIn;
			/* sound from outside will be transceived to the listener */
			transceiver.region3d = regionOut;
			Debug.Log("Inside");
		}
	}

	/* Exiting the room */
	private void OnTriggerExit(Collider other) {
		var listener = other.GetComponent<CriAtomListener>();
		if (listener != null) {
			/* the listener is outside the room */
			listener.region3d = regionOut;
			/* sound from inside will be transceived to the listener */
			transceiver.region3d = regionIn;
			Debug.Log("Outside");
		}
	}

	void OnGUI() {
		if (Scene_00_SampleList.ShowList == true) {
			return;
		}

		Scene_00_GUI.BeginGui("01/SampleMain");
		GUI.skin = Scene_00_SampleList.uiSkin;

		if (Scene_00_GUI.Button(new Rect(Scene_00_GUI.screenX - 200, 60, 160, 100), "Inside")) {
			MoveToWayPoint(waypointInside, 5f);
		}

		if (Scene_00_GUI.Button(new Rect(Scene_00_GUI.screenX - 200, 170, 160, 100), "Outside")) {
			MoveToWayPoint(waypoinyOutside, 5f);
		}

		Scene_00_GUI.EndGui();
	}

	/* Move the listener to the way point */
	private void MoveToWayPoint(Transform waypoint, float sec) {
		this.currentPos = listener.gameObject.transform.position;
		this.nextPos = waypoint.position;
		this.currentTime = this.totalTime = sec;
	}

	private void Update() {
		if (this.currentTime > 0) {
			listener.gameObject.transform.position = Vector3.Lerp(this.currentPos, this.nextPos, 1f - currentTime / totalTime);
			this.currentTime -= Time.deltaTime;
		}
	}
}

/* end of file */
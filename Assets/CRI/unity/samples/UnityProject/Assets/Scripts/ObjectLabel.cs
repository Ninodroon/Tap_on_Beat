/****************************************************************************
 *
 * Copyright (c) 2014 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;
using System.Collections;

public class ObjectLabel : MonoBehaviour {
	public GameObject target;
	public string text;
	public Vector2 offset;

	void Start ()
	{
		UnityEngine.UI.Text guiText = gameObject.AddComponent<UnityEngine.UI.Text>();
		guiText.text = text;

	}

	void Update ()
	{
		var pos = Camera.main.WorldToViewportPoint(target.transform.position);
		pos.x += offset.x;
		pos.y += offset.y;
		transform.position = pos;
	}
}
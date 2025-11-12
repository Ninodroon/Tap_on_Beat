/****************************************************************************
 *
 * Copyright (c) 2019 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_2018_1_OR_NEWER

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

[RequireComponent(typeof(Slider))]
public class GetTimelineProgress : MonoBehaviour {

	public PlayableDirector director;

	private Slider slider;

	// Use this for initialization
	void Start () {
		slider = GetComponent<Slider>();
	}

	// Update is called once per frame
	void Update () {
		slider.value = (float)(director.time / director.duration);
	}
}

#endif
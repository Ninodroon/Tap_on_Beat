using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene_04_Timeline : MonoBehaviour
{
	[SerializeField]
	UnityEngine.Playables.PlayableDirector director = null;

	[SerializeField]
	UnityEngine.UI.Slider slider = null;

	private void Awake()
	{
		slider.minValue = 0;
		slider.maxValue = (float)director.duration;
	}

	private void Update()
	{
		slider.value = (float)director.time;
	}
}

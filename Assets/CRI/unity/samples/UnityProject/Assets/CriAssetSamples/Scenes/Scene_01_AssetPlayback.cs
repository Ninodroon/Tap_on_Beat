using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CriWare;

public class Scene_01_AssetPlayback : MonoBehaviour
{
	[SerializeField]
	CriAtomSourceBase criAtomSource = null;

	public void PlayDefault()
	{
		StopAll();

		// CriAtomSourceBase のインスペクタで設定されているキューを再生
		criAtomSource.Play();
	}

	public void PlayCue(string name)
	{
		StopAll();

		// キューをキュー名で指定して再生
		criAtomSource.Play(name);
	}

	public void StopAll()
	{
		// 再生を停止
		criAtomSource.Stop();
	}
}

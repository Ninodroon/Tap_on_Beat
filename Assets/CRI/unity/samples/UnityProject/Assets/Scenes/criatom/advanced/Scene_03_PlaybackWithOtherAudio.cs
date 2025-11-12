/****************************************************************************
 *
 * Copyright (c) 2014 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

/**
 * 本サンプルは、他のアプリで音を出している場合にはBGMをミュートさせるサンプルです。
 * シーンの開始時、アプリのポーズ解除時に他のアプリが音を出しているかを判定して、
 * BGMカテゴリをミュート/ミュート解除します。
 * 本サンプルは、iOSでの動作を想定しています。
 */
/**
 * This sample mutes the BGM when other application plays a sound.
 * At the start of scene, the BGM category is muted/unmuted by determining whether
 * other application plays a sound when it releases the pause.
 * This sample assumes the system is iOS.
 */

#if !UNITY_EDITOR && UNITY_IOS
	#define PLAYBACK_WITH_OTHER_AUDIO
#endif

using UnityEngine;
using CriWare;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using AOT;

public class Scene_03_PlaybackWithOtherAudio : MonoBehaviour
{
	#region Constants
	/* category name to mute */
	private const string muteCategoryName = "BGM";
	#endregion

	#region Variables
#if PLAYBACK_WITH_OTHER_AUDIO
	private bool appPaused = false;
	private bool isMuted = false;
#endif
	#endregion

	#region Functions
	void Awake()
	{
#if PLAYBACK_WITH_OTHER_AUDIO
		/* Mute/unmute the BGM category by determining whether other application plays a sound at the start of a scene. */
		StopBgmIfOtherAudioIsPlaying();

		// add event based system to ensure that no other audio are playing. (iOS 9.0)
		CallbackDelegate del = new CallbackDelegate(StopBgmIfOtherAudioIsPlayingCallback);
		register_other_audio_is_playing_function(del, (IntPtr)GCHandle.Alloc(this));

		isMuted = CriAtomExCategory.IsMuted(muteCategoryName);

		CriAtomServer.instance.onApplicationPausePostProcess += onApplicationPausePostProcess;
#endif
	}

	void Update()
	{
	}

#if PLAYBACK_WITH_OTHER_AUDIO
	void OnApplicationPause(bool pause)
	{
		appPaused = pause;
	}

	void StopBgmIfOtherAudioIsPlaying()
	{
		if (other_audio_is_playing() == 1) {
			/* Mute the BGM when other application plays a sound. */
			CriAtomExCategory.Mute(muteCategoryName, true);
		} else if (CriAtomExCategory.IsMuted(muteCategoryName)) {
			/* Unmute the BGM category when other application does not play a sound */
			/* and the BGM category is muted. */
			CriAtomExCategory.Mute(muteCategoryName, false);
		}
	}

	[MonoPInvokeCallback (typeof (CallbackDelegate))]
	private static void StopBgmIfOtherAudioIsPlayingCallback( int isOtherAudioPlaying, IntPtr voidptr )
	{
		Scene_03_PlaybackWithOtherAudio self = ((GCHandle)voidptr).Target as Scene_03_PlaybackWithOtherAudio;
		if (isOtherAudioPlaying == 1) {
			/* Mute the BGM when other application plays a sound. */
			if (self.appPaused == false) {
				CriAtomExCategory.Mute(muteCategoryName, true);
			}
			self.isMuted = true;
		} else if (self.isMuted) {
			/* Unmute the BGM category when other application does not play a sound */
			/* and the BGM category is muted. */
			if (self.appPaused == false) {
				CriAtomExCategory.Mute(muteCategoryName, false);
			}
			self.isMuted = false;
		}
	}

	public void onApplicationPausePostProcess(bool pause) {
		if (!pause) {
			StopBgmIfOtherAudioIsPlaying ();
			isMuted = CriAtomExCategory.IsMuted(muteCategoryName);
		}
	}

	private delegate void CallbackDelegate( int isOtherAudioPlaying, IntPtr voidptr );

	/* Import the functions defined in Assets/Plugins/iOS/other_audio_is_playing.c */
	[DllImport("__Internal")]
	private static extern int other_audio_is_playing();

	[DllImport("__Internal")]
	private static extern void register_other_audio_is_playing_function(CallbackDelegate OnOtherAudioIsPlayingFunc, IntPtr userData);
#endif

	void OnGUI()
	{
#if UNITY_WEBGL
		if (CriAtom.CueSheetsAreLoading) {
			return;
		}
#endif

		if (Scene_00_SampleList.ShowList == true) {
			/* Disable GUI operations while displaying a list of samples. */
			return;
		}

		Scene_00_GUI.BeginGui("01/SampleMain");

		/* Set UI skin. */
		GUI.skin = Scene_00_SampleList.uiSkin;

		/* Get the CriAtomSource component. */
		CriAtomSource atomSource = this.GetComponent<CriAtomSource>();

		/* Display GUI, and control the playback. */
		GUILayoutOption[] option = new GUILayoutOption[]{ GUILayout.Height(28) };
		GUILayout.BeginArea(new Rect(42, 128, 480, 200));
		{
			GUILayout.Label("BGM Category Status: " + (CriAtomExCategory.IsMuted(muteCategoryName) ? "Mute" : "Playing"));
			GUILayout.Space(16);
			/* If a button is clicked, play the corresponding sound. */
			if (Scene_00_GUI.Button("Play SE1", option)) {
				/* Play the Cue of SE category. */
				atomSource.Play("gun1_High");
			}
			if (Scene_00_GUI.Button("Play SE2", option)) {
				/* Play the Cue of SE category. */
				atomSource.Play("bomb2");
			}
			if (Scene_00_GUI.Button("Play Voice", option)) {
				atomSource.Play("cri_middleware_sdk");
			}
		}
		GUILayout.EndArea();

		Scene_00_GUI.EndGui();
	}
	#endregion
}

/* end of file */

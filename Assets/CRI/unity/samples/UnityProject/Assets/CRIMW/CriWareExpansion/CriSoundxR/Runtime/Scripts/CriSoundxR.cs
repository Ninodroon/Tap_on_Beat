/****************************************************************************
 *
 * Copyright (c) 2024 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_IOS || UNITY_ANDROID || UNITY_PS4 || UNITY_PS5 || UNITY_GAMECORE_XBOXONE || UNITY_GAMECORE_SCARLETT || UNITY_SWITCH
#define CRI_SUPPORTS_SOUNDXR
#endif

using System;
using System.Runtime.InteropServices;

#if !UNITY_EDITOR && UNITY_ANDROID
using System.Globalization;
using UnityEngine;
#endif

/**
 * \addtogroup CriSoundxR
 * @{
 */

namespace CriWare {
	/**
	 * <summary>Sound xRの利用に必要な機能を提供するクラス</summary>
	 */
    public class CriSoundxR
    {
		private const string scriptVersionString = "1.00.04";

		const string pluginName =
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			"criafx_soundxr_macosx";
#elif UNITY_EDITOR || UNITY_ANDROID || UNITY_STANDALONE || UNITY_GAMECORE_XBOXONE || UNITY_GAMECORE_SCARLETT
			"criafx_soundxr";
#else
			"__Internal";
#endif

		/**
		 * <summary>Sound xRライブラリのADXへの登録</summary>
		 * <remarks>
		 * <para header='説明'>Sound xR利用時に必要なADXへの登録処理を行います。<br/>
		 * 利用時は必ずADXの初期化より前に呼び出してください。<br/>
		 * Sound xR初期化向けコンポーネントを利用する場合、本APIを呼び出す必要はありません。<br/></para>
		 * </remarks>
		 * <seealso cref='CriWare.CriSoundxRInitializer'/>
		 */
		public static void RegisterInterface()
		{
#if !UNITY_EDITOR && UNITY_ANDROID
			if(CultureInfo.InvariantCulture.CompareInfo.IndexOf(SystemInfo.processorType, "ARM", CompareOptions.IgnoreCase) < 0){ 
				Debug.LogWarning("[CRIWARE] CRI Afx Sound xR Expansion currently does not support x86_64/x86 version of Android.");
				return;
			}
#endif
			NativeMethods.criAtomExAsr_RegisterSoundxRInterface(NativeMethods.criSoundxR_GetInterface());
		}

		/**
		 * <summary>Sound xRライブラリのADXからの登録解除</summary>
		 * <remarks>
		 * <para header='説明'>Sound xRのADXへの登録を解除します。<br/>
		 * ゲーム中にライブラリの終了と再初期化を行わない限り、本APIを呼び出す必要はありません。<br/></para>
		 * </remarks>
		 */
		public static void ResetInterface()
		{
#if !UNITY_EDITOR && UNITY_ANDROID
			if(CultureInfo.InvariantCulture.CompareInfo.IndexOf(SystemInfo.processorType, "ARM", CompareOptions.IgnoreCase) < 0){ 
				Debug.LogWarning("[CRIWARE] CRI Afx Sound xR Expansion currently does not support x86_64/x86 version of Android.");
				return;
			}
#endif
			NativeMethods.criAtomExAsr_RegisterSoundxRInterface(IntPtr.Zero);
		}

		class NativeMethods
		{
#if !CRIWARE_ENABLE_HEADLESS_MODE && CRI_SUPPORTS_SOUNDXR
			[DllImport(pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
			public static extern IntPtr criSoundxR_GetInterface();
			[DllImport(CriWare.Common.pluginName, CallingConvention = CriWare.Common.pluginCallingConvention)]
			public static extern void criAtomExAsr_RegisterSoundxRInterface(IntPtr interfacePointer);
#else
			public static IntPtr criSoundxR_GetInterface(){return default;}
			public static void criAtomExAsr_RegisterSoundxRInterface(IntPtr interfacePointer) {}
#endif
		}
	}
} // namespace CriWare
/** @} */

/****************************************************************************
 *
 * Copyright (c) 2024 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

using UnityEngine;

/**
 * \addtogroup CriSoundxR
 * @{
 */

namespace CriWare
{
	/**
	 * <summary>Sound xR向け初期化前処理を行うコンポーネント</summary>
	 * <remarks>
	 * <para header='説明'>シーン内に本コンポーネントが配置されている場合、<br/>
	 * Sound xRの初期化に必要な前処理を行います。<br/>
	 * プログラムから<see cref='CriWare.CriSoundxR.RegisterInterface'/>を呼び出す場合、<br/>
	 * 本コンポーネントを配置する必要はありません。</para>
	 * </remarks>
	 */
	[DefaultExecutionOrder(-20)]
	public class CriSoundxRInitializer : MonoBehaviour
	{
		private void Awake()
		{
			CriSoundxR.RegisterInterface();
			CriAtomPlugin.OnFinalized += CriSoundxR.ResetInterface;
		}
	}
}
/** @} */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CriWare.Assets;

public class Scene_03_Multilingual : MonoBehaviour
{
	[SerializeField]
	CriAtomAcbAsset acbAsset = null;

	[SerializeField]
	UnityEngine.UI.Dropdown dropdown = null;

	public void SetLanguageByIndex(int index)
	{
		SetLanguage(dropdown.options[index].text);
	}

    public void SetLanguage(string language)
	{
		// アセットに指定されている言語名を指定して切り替え
		CriAssetsLocalization.ChangeLanguage(language);
		// 言語設定を反映するため再ロード
		CriAtomAssetsLoader.ReleaseCueSheet(acbAsset);
		CriAtomAssetsLoader.AddCueSheet(acbAsset);
	}
}

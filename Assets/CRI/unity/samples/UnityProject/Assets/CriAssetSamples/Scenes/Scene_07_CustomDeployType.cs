using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CriWare.Assets;
using System.IO;
using System;

#if UNITY_EDITOR

#if UNITY_2020_3_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using System.Security.Cryptography;

[System.Serializable, CriDisplayName("CustomDeploySample")]
public class Scene_07_CustomDeployTypeCreator : ICriAssetImplCreator
{
	public string Description =>
@"ICriAssetImplCreator インターフェイスを継承したクラスを実装することで独自のデプロイ先を実装することができます。
本サンプルではアセット内のデータを一次キャッシュに書き出してファイルロード可能にします。
AWB 等を Addressables に依存せずに AssetBundle に含めることが可能ですが、
アセット内とキャッシュ内に2重にデータを持ってしまうため、可能な限り CRI Addressables の利用をお勧めします。";

	public ICriAssetImpl CreateAssetImpl(AssetImportContext ctx)
	{
		var data = File.ReadAllBytes(ctx.assetPath);

		var md5 = MD5.Create();
		var hash = md5.ComputeHash(data);
		md5.Clear();

		return new Scene_07_CustomDeployType(data, BitConverter.ToString(hash).ToLower().Replace("-", ""));
	}
}

#endif

[System.Serializable]
public class Scene_07_CustomDeployType : ICriFileAssetImpl
{
	[SerializeField, HideInInspector]
	byte[] data;
	[SerializeField]
	string fileName;

	public Scene_07_CustomDeployType(byte[] data, string fileName)
	{
		this.data = data;
		this.fileName = fileName;
	}

	public string Path => System.IO.Path.Combine(CriWare.Common.installCachePath, fileName);
	public ulong Offset => 0;
	public long Size { get; }
	public bool IsReady { get; private set; }
	public void OnEnable() {
		if (!Directory.Exists(System.IO.Path.GetDirectoryName(Path)))
			Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
		File.WriteAllBytes(Path, data);
		IsReady = true;
#if !UNITY_EDITOR
		data = null;
#endif
	}
	public void OnDisable() => File.Delete(Path);
}

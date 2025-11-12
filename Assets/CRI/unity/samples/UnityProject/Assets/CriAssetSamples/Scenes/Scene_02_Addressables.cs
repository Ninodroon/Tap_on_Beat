using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Scene_02_Addressables : MonoBehaviour
{
	[SerializeField]
	Transform buttonsParent = null;
	[SerializeField]
	GameObject buttonTemplate = null;

	readonly string PrefabListAddress = "PrefabList";

	Scene_02_Addressables_PrefabList prefabList = null;
	Scene_02_Addressables_PrefabList.Prefab currentInfo = null;
	GameObject currentLoaded = null;

	UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle currentOp;
	private async void Awake()
	{
#if ENABLE_CACHING
		Caching.ClearCache();
#endif

		// リモートカタログの有無を確認して Addressables を初期化
		var catalogs = await Addressables.CheckForCatalogUpdates().Task;
		if (catalogs.Count > 0)
			await Addressables.UpdateCatalogs(catalogs).Task;
		else
			await Addressables.InitializeAsync().Task;

		// プレハブ情報を持ったアセットを取得してプレハブ一覧を UI 上に表示
		prefabList = await Addressables.LoadAssetAsync<Scene_02_Addressables_PrefabList>(PrefabListAddress).Task;
		UpdateView();
	}

	async void UpdateView()
	{
		// プレハブ一覧の表示要素を一度すべて削除
		foreach(Transform t in buttonsParent)
		{
			if (t == transform) continue;
			Destroy(t.gameObject);
		}

		// プレハブ情報がなければ何も表示しない
		if (prefabList == null) return;

		// プレハブ情報から各インスタンス化ボタンを生成
		foreach(var info in prefabList.prefabs)
		{
			var obj = Instantiate(buttonTemplate, buttonsParent);
			// 表示名を設定
			obj.GetComponentInChildren<UnityEngine.UI.Text>().text = $"{info.name} : {await Addressables.GetDownloadSizeAsync(info.prefab).Task} Bytes";
			// クリックされた際にインスタンス化する処理を設定
			obj.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(() => {
				// 他のプレハブインスタンスがあれば破棄
				Unload();
				currentInfo = info;

				// CRIWARE のアセットを含むかに関わらず、通常の Addressable Asset としてロード可能
				info.prefab.InstantiateAsync().Completed += e => {
					currentLoaded = e.Result;
					UpdateView();
				};

			});
			UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(obj);
		}
	}

	// ロードされているプレハブを破棄
	public void Unload()
	{
		if (currentInfo != null)
			currentInfo.prefab.ReleaseInstance(currentLoaded);
		currentInfo = null;
		currentLoaded = null;
	}

	// ダウンロード済みのキャッシュを破棄
	public void ClearCache()
	{
		// プレハブがロードされている状態では何もしない
		if (currentLoaded != null) return;
		if (prefabList == null) return;

		foreach (var info in prefabList.prefabs)
			Addressables.ClearDependencyCacheAsync(info.prefab);

		UpdateView();
	}
}

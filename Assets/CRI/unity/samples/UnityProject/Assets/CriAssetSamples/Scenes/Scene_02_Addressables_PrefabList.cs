using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Scene_02_Addressables_PrefabList : ScriptableObject
{
	[System.Serializable]
	public class Prefab
	{
		public string name;
		public AssetReference prefab;
	}

	[SerializeField]
	public Prefab[] prefabs;
}

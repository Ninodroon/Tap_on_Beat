#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

//ツールタブにStageDataAssetを作成するメニューを追加

public static class StageDataAssetCreator
{
    [MenuItem("Tools/Stage/Create StageDataAsset")]
    public static void Create()
    {
        var asset = ScriptableObject.CreateInstance<StageDataAsset>();

        string path = EditorUtility.SaveFilePanelInProject(
            "Create StageDataAsset",
            "StageData",
            "asset",
            "保存先を選んで"
        );

        if (string.IsNullOrEmpty(path)) return;

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(asset);
        Selection.activeObject = asset;
    }
}
#endif

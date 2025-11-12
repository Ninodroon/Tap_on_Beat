using UnityEngine;
using UnityEditor;

public class ObjectPlacerWindow : EditorWindow
{
    public GameObject prefab;        // 配置するPrefab
    public int count = 5;            // 配置個数
    public Vector3 startPos = Vector3.zero; // 配置開始位置
    public Vector3 spacing = Vector3.zero;   // オブジェクト間の間隔

    [MenuItem("Tools/Object Placer")]
    public static void ShowWindow()
    {
        GetWindow<ObjectPlacerWindow>("Object Placer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Placement Settings", EditorStyles.boldLabel);

        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
        count = EditorGUILayout.IntField("Count", count);
        startPos = EditorGUILayout.Vector3Field("Start Position", startPos);
        spacing = EditorGUILayout.Vector3Field("Spacing", spacing);

        if (GUILayout.Button("Place Objects"))
        {
            PlaceObjects();
        }
    }

    private void PlaceObjects()
    {
        if (prefab == null)
        {
            Debug.LogWarning("Prefabが指定されていません！");
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Place Objects");
        int group = Undo.GetCurrentGroup();

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = startPos + i * spacing;
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.position = pos;
            Undo.RegisterCreatedObjectUndo(obj, "Create Object");
        }

        Undo.CollapseUndoOperations(group);
    }
}

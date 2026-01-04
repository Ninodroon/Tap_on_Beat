using UnityEngine;

// ステージエディタの状態管理
public static class StageEditorState
{
    public static bool IsEditing;
    public static StageObjectDefinition currentDef;

    public static void PlaceCurrent(Vector3 pos)
    {
        if (currentDef == null) return;

        GameObject go = Object.Instantiate(
            currentDef.prefab,
            pos,
            Quaternion.identity
        );

        go.name = currentDef.type.ToString();
    }
}

using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public static class StageEditorState
{
    public static bool IsEditing;
    public static StageObjectDefinition currentDef;

    private static HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

    public static bool CanPlaceAt(Vector2Int baseCell, Vector2Int size)
    {
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                var cell = new Vector2Int(baseCell.x + x, baseCell.y + y);
                if (occupied.Contains(cell)) return false;
            }
        return true;
    }

    // 指定範囲のセルを占有状態に登録
    public static void RegisterOccupied(Vector2Int baseCell, Vector2Int size)
    {
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                occupied.Add(new Vector2Int(baseCell.x + x, baseCell.y + y));
    }

    // 指定範囲のセルを占有状態から解除
    public static void UnregisterOccupied(Vector2Int baseCell, Vector2Int size)
    {
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                occupied.Remove(new Vector2Int(baseCell.x + x, baseCell.y + y));
    }

    // 占有範囲の中心ワールド座標を返す
    public static Vector3 GetFootprintCenterWorld(Vector2Int baseCell, Vector2Int size)
    {
        Vector3 baseCenter = StageGridUtil.GridToWorld(baseCell);

        float dx = (size.x - 1) * StageGridUtil.GRID_X * 0.5f;
        float dy = (size.y - 1) * StageGridUtil.GRID_Y * 0.5f;

        return baseCenter + new Vector3(dx, dy, 0f);
    }

    //置く
    public static void PlaceCurrent(Vector2Int baseCell)
    {
        if (currentDef == null || currentDef.prefab == null) return;

        Vector2Int size = currentDef.size;
        if (!CanPlaceAt(baseCell, size)) return;

        Vector3 placeWorld = GetFootprintCenterWorld(baseCell, size);

#if UNITY_EDITOR
        var go = (GameObject)PrefabUtility.InstantiatePrefab(currentDef.prefab);
#else
        var go = Object.Instantiate(currentDef.prefab);
#endif
        go.transform.position = placeWorld;

        var placed = go.GetComponent<StagePlacedObject>();
        if (placed == null) placed = go.AddComponent<StagePlacedObject>();
        placed.baseCell = baseCell;
        placed.size = size;
        placed.definition = currentDef;

        RegisterOccupied(baseCell, size);
    }

    // 互換用（後で削除予定）
    public static void PlaceCurrent(Vector3 snappedWorld)
    {
        PlaceCurrent(StageGridUtil.WorldToGrid(snappedWorld));
    }

    public static void DeletePlaced(GameObject go)
    {
        if (go == null) return;

        var placed = go.GetComponent<StagePlacedObject>();
        if (placed != null)
            UnregisterOccupied(placed.baseCell, placed.size);

        Object.DestroyImmediate(go);
    }

    // シーン上の全配置物から占有情報を再構築
    public static void RebuildOccupiedFromScene()
    {
        occupied.Clear();
        var all = Object.FindObjectsOfType<StagePlacedObject>();
        foreach (var p in all)
            RegisterOccupied(p.baseCell, p.size);
    }

    // 全配置物をPrefabで置き換え（Definition更新時用）
    public static void RefreshAllPlacedObjects()
    {
#if UNITY_EDITOR
        var all = Object.FindObjectsOfType<StagePlacedObject>();

        foreach (var p in all)
        {
            if (p.definition == null || p.definition.prefab == null) continue;

            Vector3 pos = GetFootprintCenterWorld(p.baseCell, p.size);

            var go = (GameObject)PrefabUtility.InstantiatePrefab(p.definition.prefab);
            go.transform.position = pos;

            var np = go.GetComponent<StagePlacedObject>();
            if (np == null) np = go.AddComponent<StagePlacedObject>();
            np.baseCell = p.baseCell;
            np.size = p.size;
            np.definition = p.definition;

            Object.DestroyImmediate(p.gameObject);
        }

        RebuildOccupiedFromScene();
#endif
    }

    // 手動移動したオブジェクトの位置をグリッドに合わせる
    public static void BakeTransformsToPlacedData(bool snapToGrid)
    {
#if UNITY_EDITOR
        var all = Object.FindObjectsOfType<StagePlacedObject>();

        foreach (var p in all)
        {
            Undo.RecordObject(p, "Bake Placed Object");
            Undo.RecordObject(p.transform, "Bake Placed Transform");

            Vector2Int newCell = StageGridUtil.WorldToGrid(p.transform.position);
            p.baseCell = newCell;

            if (p.definition != null)
                p.size = p.definition.size;

            if (snapToGrid)
                p.transform.position = GetFootprintCenterWorld(newCell, p.size);

            EditorUtility.SetDirty(p);
        }

        RebuildOccupiedFromScene();
#endif
    }
}
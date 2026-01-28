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
    private static List<StagePlacedObject> drumCache = new List<StagePlacedObject>();

    public static List<StagePlacedObject> GetDrumObjects() => drumCache;

    // 指定マスに配置可能か判定
    public static bool CanPlaceAt(Vector2Int baseCell, Vector2Int size)
    {
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                int cellX = baseCell.x + x;
                int cellY = baseCell.y + y;
                if (occupied.Contains(new Vector2Int(cellX, cellY)))
                    return false;
            }
        return true;
    }

    // 占有マスを登録
    public static void RegisterOccupied(Vector2Int baseCell, Vector2Int size)
    {
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                occupied.Add(new Vector2Int(baseCell.x + x, baseCell.y + y));
    }

    // 占有マスを解除
    public static void UnregisterOccupied(Vector2Int baseCell, Vector2Int size)
    {
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                occupied.Remove(new Vector2Int(baseCell.x + x, baseCell.y + y));
    }

    // プレビューマスの中心のワールド座標を計算
    public static Vector3 GetFootprintCenterWorld(Vector2Int baseCell, Vector2Int size)
    {
        Vector3 baseCenter = StageGridUtil.GridToWorld(baseCell);
        float dx = (size.x - 1) * StageGridUtil.GRID_X * 0.5f;
        float dy = (size.y - 1) * StageGridUtil.GRID_Y * 0.5f;
        return baseCenter + new Vector3(dx, dy, 0f);
    }

    // 現在選択中のオブジェクトを配置
    // 現在選択中のオブジェクトを配置
    public static void PlaceCurrent(Vector2Int baseCell)
    {
        if (currentDef == null || currentDef.prefab == null) return;

        Vector2Int size = currentDef.size;
        if (!CanPlaceAt(baseCell, size)) return;

        Vector3 pos = GetFootprintCenterWorld(baseCell, size);

#if UNITY_EDITOR
        // 親（StageObjects）を用意
        const string ROOT_NAME = "StageObjects";
        GameObject root = GameObject.Find(ROOT_NAME);
        if (root == null)
        {
            root = new GameObject(ROOT_NAME);
            Undo.RegisterCreatedObjectUndo(root, "Create StageObjects Root");
        }

        var go = (GameObject)PrefabUtility.InstantiatePrefab(currentDef.prefab);
        Undo.RegisterCreatedObjectUndo(go, "Place Stage Object");
#else
    GameObject root = GameObject.Find("StageObjects");
    if (root == null) root = new GameObject("StageObjects");

    var go = Object.Instantiate(currentDef.prefab);
#endif

        // 親に入れる（ワールド座標維持）
        go.transform.SetParent(root.transform, true);

        // 位置合わせ
        go.transform.position = pos;

        var placed = go.GetComponent<StagePlacedObject>() ?? go.AddComponent<StagePlacedObject>();
        placed.baseCell = baseCell;
        placed.size = size;
        placed.definition = currentDef;

        RegisterOccupied(baseCell, size);

        if (currentDef.isDrum)
            drumCache.Add(placed);
    }


    // 配置済みオブジェクトを削除
    public static void DeletePlaced(GameObject go)
    {
        if (go == null) return;

        var p = go.GetComponent<StagePlacedObject>();
        if (p != null)
        {
            UnregisterOccupied(p.baseCell, p.size);
            drumCache.Remove(p);
        }

        Object.DestroyImmediate(go);
    }

    // シーン内の全配置オブジェクトから占有情報とドラムキャッシュを再構築
    public static void RebuildOccupiedFromScene()
    {
        occupied.Clear();
        drumCache.Clear();

        foreach (var p in Object.FindObjectsOfType<StagePlacedObject>())
        {
            RegisterOccupied(p.baseCell, p.size);

            if (p.definition != null && p.definition.isDrum)
                drumCache.Add(p);
        }
    }

    // 全配置オブジェクトをプレハブから再生成
    public static void RefreshAllPlacedObjects()
    {
#if UNITY_EDITOR
        var all = Object.FindObjectsOfType<StagePlacedObject>();
        foreach (var p in all)
        {
            if (p.definition == null || p.definition.prefab == null) continue;

            Vector2Int size = p.definition.size;
            Vector3 pos = GetFootprintCenterWorld(p.baseCell, size);

            var go = (GameObject)PrefabUtility.InstantiatePrefab(p.definition.prefab);
            go.transform.position = pos;

            var np = go.GetComponent<StagePlacedObject>() ?? go.AddComponent<StagePlacedObject>();
            np.baseCell = p.baseCell;
            np.size = size;
            np.definition = p.definition;

            Object.DestroyImmediate(p.gameObject);
        }
        RebuildOccupiedFromScene();
#endif
    }

    // フットプリント中心座標から基準セルを逆算
    static Vector2Int FootprintCenterWorldToBaseCell(Vector3 centerWorld, Vector2Int size)
    {
        float dx = (size.x - 1) * StageGridUtil.GRID_X * 0.5f;
        float dy = (size.y - 1) * StageGridUtil.GRID_Y * 0.5f;
        Vector3 baseCenter = centerWorld - new Vector3(dx, dy, 0f);
        return StageGridUtil.WorldToGrid(baseCenter);
    }

    // Transform座標を配置データに反映（オプションでグリッドスナップ）
    public static void BakeTransformsToPlacedData(bool snapToGrid)
    {
#if UNITY_EDITOR
        var all = Object.FindObjectsOfType<StagePlacedObject>();

        foreach (var p in all)
        {
            Undo.RecordObject(p, "Bake Placed Object");
            Undo.RecordObject(p.transform, "Bake Placed Transform");

            Vector2Int size = p.definition != null ? p.definition.size : p.size;
            Vector2Int baseCell = FootprintCenterWorldToBaseCell(p.transform.position, size);

            p.size = size;
            p.baseCell = baseCell;

            if (snapToGrid)
                p.transform.position = GetFootprintCenterWorld(baseCell, size);

            EditorUtility.SetDirty(p);
        }
        RebuildOccupiedFromScene();
#endif
    }

    //1番左にあるオブジェクトをPlayerのFirstDrumにする
    public static StagePlacedObject GetFirstDrumByMinX()
    {
        StagePlacedObject best = null;

        foreach (var p in Object.FindObjectsOfType<StagePlacedObject>())
        {
            if (p == null || p.definition == null) continue;
            if (!p.definition.isDrum) continue;

            if (best == null)
            {
                best = p;
                continue;
            }

            // x が小さい方が優先、同じなら y が小さい方
            if (p.baseCell.x < best.baseCell.x ||
                (p.baseCell.x == best.baseCell.x && p.baseCell.y < best.baseCell.y))
            {
                best = p;
            }
        }

        return best;
    }


}
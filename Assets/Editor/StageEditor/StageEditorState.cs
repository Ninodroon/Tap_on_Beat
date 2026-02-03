using System;
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

    public static bool CanPlaceAt(Vector2Int baseCell, Vector2Int size)
    {
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                if (occupied.Contains(baseCell + new Vector2Int(x, y)))
                    return false;
        return true;
    }

    public static void RegisterOccupied(Vector2Int baseCell, Vector2Int size)
    {
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                occupied.Add(baseCell + new Vector2Int(x, y));
    }

    public static void UnregisterOccupied(Vector2Int baseCell, Vector2Int size)
    {
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                occupied.Remove(baseCell + new Vector2Int(x, y));
    }

    public static Vector3 GetFootprintCenterWorld(Vector2Int baseCell, Vector2Int size)
    {
        Vector3 baseCenter = StageGridUtil.GridToWorld(baseCell);
        float dx = (size.x - 1) * StageGridUtil.GRID_X * 0.5f;
        float dy = (size.y - 1) * StageGridUtil.GRID_Y * 0.5f;
        return baseCenter + new Vector3(dx, dy, 0f);
    }

    public static void PlaceCurrent(Vector2Int baseCell)
    {
        if (currentDef == null || currentDef.prefab == null) return;
        if (!CanPlaceAt(baseCell, currentDef.size)) return;

        Vector3 pos = GetFootprintCenterWorld(baseCell, currentDef.size);

#if UNITY_EDITOR
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

        go.transform.SetParent(root.transform, true);
        go.transform.position = pos;

        var placed = go.GetComponent<StagePlacedObject>() ?? go.AddComponent<StagePlacedObject>();
        placed.baseCell = baseCell;
        placed.size = currentDef.size;
        placed.definition = currentDef;

        RegisterOccupied(baseCell, currentDef.size);

        if (currentDef.isDrum)
            drumCache.Add(placed);
    }

    public static void DeletePlaced(GameObject go)
    {
        if (go == null) return;

        var p = go.GetComponent<StagePlacedObject>();
        if (p != null)
        {
            UnregisterOccupied(p.baseCell, p.size);
            drumCache.Remove(p);
        }

        UnityEngine.Object.DestroyImmediate(go);
    }

    public static void RebuildOccupiedFromScene()
    {
        occupied.Clear();
        drumCache.Clear();

        foreach (var p in UnityEngine.Object.FindObjectsOfType<StagePlacedObject>())
        {
            RegisterOccupied(p.baseCell, p.size);

            if (p.definition != null && p.definition.isDrum)
                drumCache.Add(p);
        }
    }

    public static void RefreshAllPlacedObjects()
    {
#if UNITY_EDITOR
        var all = UnityEngine.Object.FindObjectsOfType<StagePlacedObject>();
        foreach (var p in all)
        {
            if (p.definition == null || p.definition.prefab == null) continue;

            Vector3 pos = GetFootprintCenterWorld(p.baseCell, p.definition.size);

            var go = (GameObject)PrefabUtility.InstantiatePrefab(p.definition.prefab);
            go.transform.position = pos;

            var np = go.GetComponent<StagePlacedObject>() ?? go.AddComponent<StagePlacedObject>();
            np.baseCell = p.baseCell;
            np.size = p.definition.size;
            np.definition = p.definition;

            UnityEngine.Object.DestroyImmediate(p.gameObject);
        }
        RebuildOccupiedFromScene();
#endif
    }

    static Vector2Int FootprintCenterWorldToBaseCell(Vector3 centerWorld, Vector2Int size)
    {
        float dx = (size.x - 1) * StageGridUtil.GRID_X * 0.5f;
        float dy = (size.y - 1) * StageGridUtil.GRID_Y * 0.5f;
        return StageGridUtil.WorldToGrid(centerWorld - new Vector3(dx, dy, 0f));
    }

    public static void BakeTransformsToPlacedData(bool snapToGrid)
    {
#if UNITY_EDITOR
        var all = UnityEngine.Object.FindObjectsOfType<StagePlacedObject>();

        foreach (var p in all)
        {
            Undo.RecordObject(p, "Bake Placed Object");
            Undo.RecordObject(p.transform, "Bake Placed Transform");

            Vector2Int size = p.definition != null ? p.definition.size : p.size;
            p.baseCell = FootprintCenterWorldToBaseCell(p.transform.position, size);
            p.size = size;

            if (snapToGrid)
                p.transform.position = GetFootprintCenterWorld(p.baseCell, size);

            EditorUtility.SetDirty(p);
        }
        RebuildOccupiedFromScene();
#endif
    }

    public static StagePlacedObject GetFirstDrumByMinX()
    {
        StagePlacedObject best = null;

        foreach (var p in UnityEngine.Object.FindObjectsOfType<StagePlacedObject>())
        {
            if (p.definition == null || !p.definition.isDrum) continue;

            if (best == null || p.baseCell.x < best.baseCell.x ||
                (p.baseCell.x == best.baseCell.x && p.baseCell.y < best.baseCell.y))
                best = p;
        }

        return best;
    }

    public static void SaveToStageData(StageDataAsset data)
    {
#if UNITY_EDITOR
        if (data == null) return;

        var list = new List<StageDataAsset.Item>();

        foreach (var p in UnityEngine.Object.FindObjectsOfType<StagePlacedObject>())
        {
            if (p.definition == null) continue;

            list.Add(new StageDataAsset.Item
            {
                definition = p.definition,
                x = p.baseCell.x,
                y = p.baseCell.y,
                z = 0
            });
        }

        data.SetItems(list);
#endif
    }

    public static void LoadFromStageData(StageDataAsset data)
    {
#if UNITY_EDITOR
        if (data == null) return;

        var all = UnityEngine.Object.FindObjectsOfType<StagePlacedObject>();
        foreach (var p in all)
            DeletePlaced(p.gameObject);

        foreach (var it in data.Items)
        {
            if (it == null || it.definition == null) continue;

            currentDef = it.definition;
            PlaceCurrent(new Vector2Int(it.x, it.y));
        }

        currentDef = null;
        RebuildOccupiedFromScene();
#endif
    }
}
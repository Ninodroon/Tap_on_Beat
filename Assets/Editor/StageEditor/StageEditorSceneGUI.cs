using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//シーンビューでのステージエディタのGUI制御。マウス位置取得、オブジェクト配置など。

/*
1.SceneGUI: マウス位置→ワールド座標→グリッドセル番号に変換
2.SceneGUI: セル番号から配置プレビュー表示（青/赤）
3.クリック時: セル番号をPlaceCurrent(baseCell)に渡す
4.State: セル+サイズから占有範囲の中心座標を計算
5.State: その座標にPrefab配置、占有情報登録 
 */

[InitializeOnLoad]
public static class StageEditorSceneGUI
{
    static StageEditorSceneGUI()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView view)
    {
        if (!StageEditorState.IsEditing) return;

        Event e = Event.current;
        if (e == null) return;

        // 右クリック削除
        if (e.type == EventType.MouseDown && e.button == 1 && !e.alt)
        {
            TryDeleteUnderMouse(e);
            e.Use();
            SceneView.RepaintAll();
            return;
        }

        // マウスのワールド位置
        Vector3 world = GetMouseWorldPosition(e);

        // ★ セル番号は world から取る（ここが重要）
        Vector2Int baseCell = StageGridUtil.WorldToGrid(world);
        //Vector3 baseCell = StageGridUtil.WorldToGrid(world);
        
        // ★ セル中心を snapped とする（ここが重要）
        Vector3 snapped = StageGridUtil.GridToWorld(baseCell);

        // size対応・赤/青プレビュー（あなたのDrawPlacementPreviewがセル塗り版ならOK）
        DrawPlacementPreview(snapped);

        // 左クリック配置（置けるときだけ）
        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            if (StageEditorState.currentDef != null &&
                StageEditorState.CanPlaceAt(baseCell, StageEditorState.currentDef.size))
            {
                StageEditorState.PlaceCurrent(snapped);
                //StageEditorState.PlaceCurrent(baseCell);//エラー
            }

            e.Use();
        }

        SceneView.RepaintAll();
    }

    static void TryDeleteUnderMouse(Event e)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 9999f))
        {
            // 配置物（StagePlacedObject）を持ってるやつだけ消す
            var placed = hit.collider.GetComponentInParent<StagePlacedObject>();
            if (placed != null)
                StageEditorState.DeletePlaced(placed.gameObject);
        }
    }

    static Vector3 GetMouseWorldPosition(Event e)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);

        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return Vector3.zero;
    }

    static void DrawSnapPreview(Vector3 pos)
    {
        Handles.color = Color.cyan;
        Handles.DrawWireCube(
            pos,
            new Vector3(
                StageGridUtil.GRID_X,
                StageGridUtil.GRID_Y,
                0.01f
            )
        );
    }

    //おけるマスを表示する
    static void DrawPlacementPreview(Vector3 snappedWorld)
    {
        if (StageEditorState.currentDef == null) return;

        Vector2Int baseCell = StageGridUtil.WorldToGrid(snappedWorld);
        Vector2Int size = StageEditorState.currentDef.size;

        bool canPlaceAll = StageEditorState.CanPlaceAt(baseCell, size);

        // 視認性：置ける=青、置けない=赤
        Color outline = canPlaceAll
            ? new Color(0.2f, 0.8f, 1.0f, 0.95f)   // 青
            : new Color(1.0f, 0.2f, 0.2f, 0.95f);  // 赤

        Color fill = canPlaceAll
            ? new Color(0.2f, 0.8f, 1.0f, 0.12f)
            : new Color(1.0f, 0.2f, 0.2f, 0.12f);

        // セル単位で塗る
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                Vector2Int cell = new Vector2Int(baseCell.x + x, baseCell.y + y);

                Vector3 cellWorld = StageGridUtil.GridToWorld(cell);

                // 1セルの中心
                //Vector3 center = cellWorld + new Vector3(StageGridUtil.GRID_X * 0.5f, StageGridUtil.GRID_Y * 0.5f, 0f);
                Vector3 center = cellWorld;

                float w = StageGridUtil.GRID_X;
                float h = StageGridUtil.GRID_Y;

                Vector3[] rect =
                {
                    center + new Vector3(-w/2, -h/2, 0),
                    center + new Vector3(-w/2,  h/2, 0),
                    center + new Vector3( w/2,  h/2, 0),
                    center + new Vector3( w/2, -h/2, 0),
                };

                Handles.DrawSolidRectangleWithOutline(rect, fill, outline);
            }
    }

}

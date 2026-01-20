using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class StageEditorSceneGUI
{
    static readonly Color GRID_COLOR = new Color(0.4f, 0.8f, 1.0f, 0.15f);

    static StageEditorSceneGUI()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView view)
    {
        if (!StageEditorState.IsEditing) return;

        Event e = Event.current;
        if (e == null) return;

        DrawBackgroundGrid(view);

        // 右クリック: 削除
        if (e.type == EventType.MouseDown && e.button == 1 && !e.alt)
        {
            TryDeleteUnderMouse(e);
            e.Use();
            SceneView.RepaintAll();
            return;
        }

        Vector3 world = GetMouseWorldPosition(e);
        Vector2Int baseCell = StageGridUtil.WorldToGrid(world);
        Vector3 snapped = StageGridUtil.GridToWorld(baseCell);

        DrawPlacementPreview(snapped);

        // 左クリック: 配置
        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            if (StageEditorState.currentDef != null &&
                StageEditorState.CanPlaceAt(baseCell, StageEditorState.currentDef.size))
            {
                StageEditorState.PlaceCurrent(baseCell);
            }
            e.Use();
        }

        SceneView.RepaintAll();
    }

    // カメラビュー全体にグリッドを描画
    static void DrawBackgroundGrid(SceneView view)
    {
        Camera cam = view.camera;
        if (cam == null) return;

        Color prevColor = Handles.color;
        var prevZTest = Handles.zTest;

        // 画面四隅のワールド座標を取得
        Vector3 w0 = ScreenToWorldOnZ0(cam, new Vector2(0, 0));
        Vector3 w1 = ScreenToWorldOnZ0(cam, new Vector2(cam.pixelWidth, 0));
        Vector3 w2 = ScreenToWorldOnZ0(cam, new Vector2(cam.pixelWidth, cam.pixelHeight));
        Vector3 w3 = ScreenToWorldOnZ0(cam, new Vector2(0, cam.pixelHeight));

        float minX = Mathf.Min(w0.x, w1.x, w2.x, w3.x);
        float maxX = Mathf.Max(w0.x, w1.x, w2.x, w3.x);
        float minY = Mathf.Min(w0.y, w1.y, w2.y, w3.y);
        float maxY = Mathf.Max(w0.y, w1.y, w2.y, w3.y);

        float stepX = StageGridUtil.GRID_X;
        float stepY = StageGridUtil.GRID_Y;

        // 余白追加
        float padX = stepX * 2f;
        float padY = stepY * 2f;
        minX -= padX; maxX += padX;
        minY -= padY; maxY += padY;

        // グリッド開始・終了座標
        float startX = Mathf.Floor(minX / stepX) * stepX;
        float endX = Mathf.Ceil(maxX / stepX) * stepX;
        float startY = Mathf.Floor(minY / stepY) * stepY;
        float endY = Mathf.Ceil(maxY / stepY) * stepY;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        Handles.color = GRID_COLOR;

        // 縦線
        for (float x = startX; x <= endX; x += stepX)
            Handles.DrawLine(
                new Vector3(x, startY, StageGridUtil.FIXED_Z),
                new Vector3(x, endY, StageGridUtil.FIXED_Z));

        // 横線
        for (float y = startY; y <= endY; y += stepY)
            Handles.DrawLine(
                new Vector3(startX, y, StageGridUtil.FIXED_Z),
                new Vector3(endX, y, StageGridUtil.FIXED_Z));

        Handles.color = prevColor;
        Handles.zTest = prevZTest;
    }

    // スクリーン座標からZ=0平面上のワールド座標を取得
    static Vector3 ScreenToWorldOnZ0(Camera cam, Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : Vector3.zero;
    }

    // マウス位置のワールド座標を取得
    static Vector3 GetMouseWorldPosition(Event e)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : Vector3.zero;
    }

    // マウス下のオブジェクトを削除
    static void TryDeleteUnderMouse(Event e)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 9999f))
        {
            var placed = hit.collider.GetComponentInParent<StagePlacedObject>();
            if (placed != null)
                StageEditorState.DeletePlaced(placed.gameObject);
        }
    }

    // 配置プレビューを描画
    static void DrawPlacementPreview(Vector3 snappedWorld)
    {
        if (StageEditorState.currentDef == null) return;

        Vector2Int baseCell = StageGridUtil.WorldToGrid(snappedWorld);
        Vector2Int size = StageEditorState.currentDef.size;

        bool canPlace = StageEditorState.CanPlaceAt(baseCell, size);

        Color outline = canPlace
            ? new Color(0.2f, 0.8f, 1f, 0.95f)
            : new Color(1f, 0.2f, 0.2f, 0.95f);

        Color fill = canPlace
            ? new Color(0.2f, 0.8f, 1f, 0.15f)
            : new Color(1f, 0.2f, 0.2f, 0.15f);

        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                Vector2Int cell = new Vector2Int(baseCell.x + x, baseCell.y + y);
                Vector3 center = StageGridUtil.GridToWorld(cell);

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
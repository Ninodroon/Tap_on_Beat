using DG.Tweening.Core.Easing;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class StageEditorSceneGUI
{    
    static readonly Color GRID_COLOR = new Color(0.4f, 0.8f, 1.0f, 0.15f);
    static readonly Color DRUM_COLUMN_COLOR = new Color(1.0f, 0.95f, 0.2f, 0.5f);

    static readonly float JUMP_H1 = 2.0f;
    static readonly float JUMP_H2 = 4.0f;
    static readonly float JUMP_H3 = 7.0f;

    static readonly Color JUMP_LINE_1 = new Color(0.2f, 1.0f, 0.6f, 0.5f);
    static readonly Color JUMP_LINE_2 = new Color(0.2f, 0.8f, 1.0f, 0.5f);
    static readonly Color JUMP_LINE_3 = new Color(1.0f, 0.2f, 0.9f, 0.5f);

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
        DrawJumpHeightRowsFromPlayer(view);//‚³‚ê‚È‚¢

        Vector3 world = GetMouseWorldPosition(e);
        Vector2Int mouseCell = StageGridUtil.WorldToGrid(world);

        DrawDrumColumnIfNeeded(view, mouseCell);

        if (e.type == EventType.MouseDown && e.button == 1 && !e.alt)
        {
            TryDeleteUnderMouse(e);
            e.Use();
            SceneView.RepaintAll();
            return;
        }

        DrawPlacementPreview(mouseCell);

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            if (StageEditorState.currentDef != null &&
                StageEditorState.CanPlaceAt(mouseCell, StageEditorState.currentDef.size))
            {
                StageEditorState.PlaceCurrent(mouseCell);
            }
            e.Use();
        }

        SceneView.RepaintAll();
    }

    static void DrawJumpHeightRows(SceneView view)
    {
        DrawHorizontalRow(view, WorldHeightToCellY(JUMP_H1), JUMP_LINE_1);
        DrawHorizontalRow(view, WorldHeightToCellY(JUMP_H2), JUMP_LINE_2);
        DrawHorizontalRow(view, WorldHeightToCellY(JUMP_H3), JUMP_LINE_3);
    }
    static void DrawJumpHeightRowsFromPlayer(SceneView view)
    {
        //var pManager = Object.FindObjectOfType<Manager>();
        //if (pManager == null) return;

        //DededeJump2 pPlayer = pManager.Player;
        //if (pPlayer == null) return;

        var pManager = Object.FindObjectOfType<Manager>();
        if (pManager == null) return;

        DededeJump2 playerGO = pManager.Player;
        if (playerGO == null) return;

        DededeJump2 pPlayer = playerGO.GetComponent<DededeJump2>();
        if (pPlayer == null) return;

        float adjust = 0.5f;

        float h1 = pPlayer.normalJumpHeight + adjust;
        float h2 = pPlayer.highJumpHeight + adjust;
        float h3 = pPlayer.superJumpHeight + adjust;

        int y1 = WorldHeightToCellY(h1);
        int y2 = WorldHeightToCellY(h2);
        int y3 = WorldHeightToCellY(h3);

        //DrawHorizontalRow(view, WorldHeightToCellY(y1), JUMP_LINE_1);
        //DrawHorizontalRow(view, WorldHeightToCellY(y2), JUMP_LINE_2);
        //DrawHorizontalRow(view, WorldHeightToCellY(y3), JUMP_LINE_3);

        DrawHorizontalRow(view, y1, JUMP_LINE_1);
        DrawHorizontalRow(view, y2, JUMP_LINE_2);
        DrawHorizontalRow(view, y3, JUMP_LINE_3);
    }

    static int WorldHeightToCellY(float worldY)
    {
        return StageGridUtil.WorldToGrid(new Vector3(0f, worldY, 0f)).y;
    }

    static void DrawHorizontalRow(SceneView view, int cellY, Color fill)
    {
        Camera cam = view.camera;
        if (cam == null) return;

        Color prevColor = Handles.color;
        var prevZTest = Handles.zTest;

        Vector3 w0 = ScreenToWorldOnZ0(cam, Vector2.zero);
        Vector3 w1 = ScreenToWorldOnZ0(cam, new Vector2(cam.pixelWidth, 0));

        float minX = Mathf.Min(w0.x, w1.x);
        float maxX = Mathf.Max(w0.x, w1.x);
        float stepX = StageGridUtil.GRID_X;

        int xStart = Mathf.FloorToInt(minX / stepX) - 2;
        int xEnd = Mathf.CeilToInt(maxX / stepX) + 2;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        Handles.color = fill;

        float w = StageGridUtil.GRID_X;
        float h = StageGridUtil.GRID_Y;

        for (int x = xStart; x <= xEnd; x++)
        {
            Vector3 center = StageGridUtil.GridToWorld(new Vector2Int(x, cellY));

            Vector3[] rect =
            {
                center + new Vector3(-w/2, -h/2, 0),
                center + new Vector3(-w/2,  h/2, 0),
                center + new Vector3( w/2,  h/2, 0),
                center + new Vector3( w/2, -h/2, 0),
            };

            Handles.DrawSolidRectangleWithOutline(rect, fill, Color.clear);
        }

        Handles.color = prevColor;
        Handles.zTest = prevZTest;
    }

    static void DrawDrumColumnIfNeeded(SceneView view, Vector2Int mouseCell)
    {
        foreach (var p in StageEditorState.GetDrumObjects())
        {
            int xMin = p.baseCell.x;
            int xMax = p.baseCell.x + p.size.x - 1;

            if (mouseCell.x >= xMin && mouseCell.x <= xMax)
            {
                DrawVerticalColumn(view, mouseCell.x);
                return;
            }
        }
    }

    static void DrawVerticalColumn(SceneView view, int cellX)
    {
        Camera cam = view.camera;
        if (cam == null) return;

        Color prevColor = Handles.color;
        var prevZTest = Handles.zTest;

        Vector3 w0 = ScreenToWorldOnZ0(cam, Vector2.zero);
        Vector3 w1 = ScreenToWorldOnZ0(cam, new Vector2(0, cam.pixelHeight));

        float minY = Mathf.Min(w0.y, w1.y);
        float maxY = Mathf.Max(w0.y, w1.y);
        float stepY = StageGridUtil.GRID_Y;

        int yStart = Mathf.FloorToInt(minY / stepY) - 2;
        int yEnd = Mathf.CeilToInt(maxY / stepY) + 2;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        Handles.color = DRUM_COLUMN_COLOR;

        float w = StageGridUtil.GRID_X;
        float h = StageGridUtil.GRID_Y;

        for (int y = yStart; y <= yEnd; y++)
        {
            Vector3 center = StageGridUtil.GridToWorld(new Vector2Int(cellX, y));

            Vector3[] rect =
            {
                center + new Vector3(-w/2, -h/2, 0),
                center + new Vector3(-w/2,  h/2, 0),
                center + new Vector3( w/2,  h/2, 0),
                center + new Vector3( w/2, -h/2, 0),
            };

            Handles.DrawSolidRectangleWithOutline(rect, DRUM_COLUMN_COLOR, Color.clear);
        }

        Handles.color = prevColor;
        Handles.zTest = prevZTest;
    }

    static void DrawBackgroundGrid(SceneView view)
    {
        Camera cam = view.camera;
        if (cam == null) return;

        Color prevColor = Handles.color;
        var prevZTest = Handles.zTest;

        Vector3 w0 = ScreenToWorldOnZ0(cam, Vector2.zero);
        Vector3 w1 = ScreenToWorldOnZ0(cam, new Vector2(cam.pixelWidth, 0));
        Vector3 w2 = ScreenToWorldOnZ0(cam, new Vector2(cam.pixelWidth, cam.pixelHeight));
        Vector3 w3 = ScreenToWorldOnZ0(cam, new Vector2(0, cam.pixelHeight));

        float minX = Mathf.Min(w0.x, w1.x, w2.x, w3.x);
        float maxX = Mathf.Max(w0.x, w1.x, w2.x, w3.x);
        float minY = Mathf.Min(w0.y, w1.y, w2.y, w3.y);
        float maxY = Mathf.Max(w0.y, w1.y, w2.y, w3.y);

        float stepX = StageGridUtil.GRID_X;
        float stepY = StageGridUtil.GRID_Y;

        minX -= stepX * 2f;
        maxX += stepX * 2f;
        minY -= stepY * 2f;
        maxY += stepY * 2f;

        float startX = Mathf.Floor(minX / stepX) * stepX;
        float endX = Mathf.Ceil(maxX / stepX) * stepX;
        float startY = Mathf.Floor(minY / stepY) * stepY;
        float endY = Mathf.Ceil(maxY / stepY) * stepY;

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        Handles.color = GRID_COLOR;

        for (float x = startX; x <= endX; x += stepX)
            Handles.DrawLine(new Vector3(x, startY, StageGridUtil.FIXED_Z),
                             new Vector3(x, endY, StageGridUtil.FIXED_Z));

        for (float y = startY; y <= endY; y += stepY)
            Handles.DrawLine(new Vector3(startX, y, StageGridUtil.FIXED_Z),
                             new Vector3(endX, y, StageGridUtil.FIXED_Z));

        Handles.color = prevColor;
        Handles.zTest = prevZTest;
    }

    static Vector3 ScreenToWorldOnZ0(Camera cam, Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : Vector3.zero;
    }

    static Vector3 GetMouseWorldPosition(Event e)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : Vector3.zero;
    }

    static void TryDeleteUnderMouse(Event e)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        //if (Physics.Raycast(ray, out RaycastHit hit, 9999f))
        if (Physics.Raycast(ray, out RaycastHit hit, 9999f, ~0, QueryTriggerInteraction.Collide))
        {
            var placed = hit.collider.GetComponentInParent<StagePlacedObject>();
            if (placed != null)
                StageEditorState.DeletePlaced(placed.gameObject);

            Debug.Log("Hit: " + hit.collider.name);
        }
        else {
            Debug.Log("Raycast hit nothing");
        }
    }

    static void DrawPlacementPreview(Vector2Int baseCell)
    {
        if (StageEditorState.currentDef == null) return;

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
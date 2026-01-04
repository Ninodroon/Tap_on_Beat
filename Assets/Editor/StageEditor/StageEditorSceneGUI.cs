using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//シーンビューでのステージエディタのGUI制御。マウス位置取得、オブジェクト配置など。

//public class StageEditorSceneGUI : MonoBehaviour

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

        Vector3 world = GetMouseWorldPosition(e);
        Vector3 snapped = StageGridUtil.SnapToGrid(world);

        DrawSnapPreview(snapped);

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            StageEditorState.PlaceCurrent(snapped);
            e.Use();
        }

        SceneView.RepaintAll();
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
}

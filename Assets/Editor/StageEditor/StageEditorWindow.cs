using UnityEditor;
using UnityEngine;

// ステージエディタのウィンドウ
public class StageEditorWindow : EditorWindow
{
    StageObjectDefinition[] defs;
    void OnEnable()
    {
        defs = Resources.LoadAll<StageObjectDefinition>("StageObjects");
    }

    [MenuItem("Tools/Stage Editor")]
    static void Open()
    {
        GetWindow<StageEditorWindow>("Stage Editor");
    }

    void OnGUI()
    {
        GUILayout.Label("Stage Editor", EditorStyles.boldLabel);

        //StageEditorState.IsEditing =
        //    GUILayout.Toggle(StageEditorState.IsEditing, "Edit Mode");

        bool newEditing =
        GUILayout.Toggle(StageEditorState.IsEditing, "Edit Mode");

        //if (GUILayout.Button("Refresh Placed Objects (Rebuild From Definitions)"))
        if (GUILayout.Button("Update Objects(from Definitions)"))
        {
            StageEditorState.RefreshAllPlacedObjects();
        }

        //if (GUILayout.Button("Bake From Scene (Update baseCell)"))
        if (GUILayout.Button("Save changes To Grid "))
        {
            StageEditorState.BakeTransformsToPlacedData(snapToGrid: false);
            StageEditorState.RefreshAllPlacedObjects();

        }


        //EditMode を ON にしたときに、シーン上の配置物から状態を再構築する
        if (newEditing != StageEditorState.IsEditing)
        {
            StageEditorState.IsEditing = newEditing;

            if (StageEditorState.IsEditing)
            {
                StageEditorState.RebuildOccupiedFromScene();
            }
        }

        GUILayout.Space(10);

        GUILayout.Label("Place Object", EditorStyles.boldLabel);

        foreach (var def in defs)
        {
            if (GUILayout.Button(def.name))
            {
                StageEditorState.currentDef = def;
            }
        }

        // 状態表示（デバッグ兼UX）
        GUILayout.Space(10);
        GUILayout.Label(
            $"Editing: {StageEditorState.IsEditing}\n" +
            $"Selected: {(StageEditorState.currentDef ? StageEditorState.currentDef.name : "None")}",
            EditorStyles.helpBox
        );
    }

}
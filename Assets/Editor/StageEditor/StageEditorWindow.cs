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

        StageEditorState.IsEditing =
            GUILayout.Toggle(StageEditorState.IsEditing, "Edit Mode");

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
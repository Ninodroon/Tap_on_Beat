using UnityEditor;
using UnityEngine;

// ステージエディタのウィンドウ
public class StageEditorWindow : EditorWindow
{
    StageObjectDefinition[] defs;
    Vector2 scrollPos;

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

        bool newEditing = GUILayout.Toggle(StageEditorState.IsEditing, "Edit Mode");

        if (GUILayout.Button("Update Objects(from Definitions)"))
        {
            StageEditorState.RefreshAllPlacedObjects();
        }

        if (GUILayout.Button("Save changes To Grid"))
        {
            StageEditorState.BakeTransformsToPlacedData(snapToGrid: false);
            StageEditorState.RefreshAllPlacedObjects();
        }

        // EditModeをONにしたときに、シーン上の配置物から状態を再構築
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

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        foreach (var def in defs)
        {
            if (def == null || def.prefab == null) continue;

            bool isSelected = StageEditorState.currentDef == def;
            Color prevColor = GUI.backgroundColor;
            if (isSelected)
                GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);

            GUILayout.BeginHorizontal("box");

            // プレハブのプレビュー画像
            Texture2D preview = AssetPreview.GetAssetPreview(def.prefab);
            if (preview != null)
            {
                GUILayout.Label(preview, GUILayout.Width(64), GUILayout.Height(64));
            }
            else
            {
                GUILayout.Box("No Preview", GUILayout.Width(64), GUILayout.Height(64));
            }

            // 名前とサイズ情報
            GUILayout.BeginVertical();
            GUILayout.Label(def.name, EditorStyles.boldLabel);
            GUILayout.Label($"Size: {def.size.x}x{def.size.y}", EditorStyles.miniLabel);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                StageEditorState.currentDef = def;
            }

            GUILayout.EndHorizontal();
            GUI.backgroundColor = prevColor;
        }

        GUILayout.EndScrollView();

        // 状態表示
        GUILayout.Space(10);
        GUILayout.Label(
            $"Editing: {StageEditorState.IsEditing}\n" +
            $"Selected: {(StageEditorState.currentDef ? StageEditorState.currentDef.name : "None")}",
            EditorStyles.helpBox
        );
    }
}
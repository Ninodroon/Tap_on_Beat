/****************************************************************************
 *
 * Copyright (c) 2023 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

/**
 * 本サンプルは、Sound xRを使用した再生を行います。
 * 画面中央の球体はリスナーを示します。
 * Playを押して、音を再生する球体を増やします。
 * Stop Allを押して、すべての球体を削除し、再生を停止します。
 * Enable/Disable Binualizerを押して、バイノーライザを切り替えます。
 * バイノーライザが有効する時、Sound xRを利用して音を再生します。
 */
/**
 * This sample performs playback with Sound xR.
 * The sphere shown in the center represents the listener.
 * Press Play to instantiate a sound-playing sphere.
 * Press Stop All to destory all sphere and stop all playback.
 * Press Enable/Disable Binualizer to toggle the binualizer.
 * When binualizer is enabled, Sound xR is used for audio playback.
 */
using System.Collections.Generic;
using UnityEngine;
using CriWare;

public class Scene_01_SoundxR_Preview : MonoBehaviour
{
    public GameObject atomSrcPrefab;
    private List<GameObject> atomSources;
    private string ToggleBtnText;

    private void OnGUI()
    {
        if (Scene_00_SampleList.ShowList == true)
        {
            return;
        }

        Scene_00_GUI.BeginGui("01/SoundxRPreview");
        GUI.skin = Scene_00_SampleList.uiSkin;

        GUILayout.BeginArea(new Rect(42, 100, 350, 150), Scene_00_SampleList.TextStyle);
        {
            if (Scene_00_GUI.Button("Play"))
            {
                InstantiateAtomSourceObj();
            }

            if (Scene_00_GUI.Button("Stop All"))
            {
                DestoryAllAtomSourceObj();
            }

            if (Scene_00_GUI.Button(ToggleBtnText))
            {
                CriAtomExAsr.EnableBinauralizer(!CriAtomExAsr.IsEnabledBinauralizer());
                ToggleBtnText = CriAtomExAsr.IsEnabledBinauralizer() ? "Disable Binauralizer" : "Enable Binauralizer";
            }

            GUILayout.Label("Playback Count : " + atomSources.Count);
        }
        GUILayout.EndArea();

        Scene_00_GUI.EndGui();
    }

    private void Start()
    {
        atomSources = new List<GameObject>();

        ToggleBtnText = CriAtomExAsr.IsEnabledBinauralizer() ? "Disable Binauralizer" : "Enable Binauralizer";
    }

    private void InstantiateAtomSourceObj()
    {
        GameObject newAtomSopurceObj = GameObject.Instantiate(atomSrcPrefab);
        atomSources.Add(newAtomSopurceObj);
    }

    private void DestoryAllAtomSourceObj()
    {
        foreach (GameObject atomSource in atomSources)
        {
            GameObject.Destroy(atomSource);
        }

        atomSources.Clear();
    }

}

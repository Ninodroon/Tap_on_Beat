/****************************************************************************
 *
 * Copyright (c) 2023 CRI Middleware Co., Ltd.
 *
 ****************************************************************************/

/**
 * 音を再生する球体を回転するスクリプトです。
 */
/**
 * This script keeps a sound-playing sphere orbiting.
 */
using UnityEngine;
using CriWare;

public class Scene_01_SoundxR_Preview_OrbitalObject : MonoBehaviour
{
    public float orbitSpeed = 5f;
    public float radius = 10f;
    public CriAtomSource atomSrc;

    private float angle;

    private void Start()
    {
        // Set to 3D Positioning for demo purpose. Effective when binauralizer is off.
        atomSrc.player.SetPanType(CriAtomEx.PanType.Pos3d);
    }

    void Update()
    {
        angle += orbitSpeed * Time.deltaTime;

        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;
        float y = Mathf.Sin(angle) * radius;

        transform.position = new Vector3(x, y, z);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ÉOÉäÉbÉhê›íË

//public class StageGridUtil : MonoBehaviour

public static class StageGridUtil
{
    public const float GRID_X = 1.0f;
    public const float GRID_Y = 0.5f;
    public const float FIXED_Z = 0f;

    public static Vector3 SnapToGrid(Vector3 worldPos)
    {
        float x = Mathf.Round(worldPos.x / GRID_X) * GRID_X;
        float y = Mathf.Round(worldPos.y / GRID_Y) * GRID_Y;
        return new Vector3(x, y, FIXED_Z);
    }

    public static Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int gx = Mathf.RoundToInt(worldPos.x / GRID_X);
        int gy = Mathf.RoundToInt(worldPos.y / GRID_Y);
        return new Vector2Int(gx, gy);
    }

    public static Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * GRID_X,
            gridPos.y * GRID_Y,
            FIXED_Z
        );
    }
}

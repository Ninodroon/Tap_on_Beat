using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//グリッド設定
//public class StageGridUtil : MonoBehaviour

public static class StageGridUtil
{
    //public const float GRID_X = 1.0f;
    //public const float GRID_Y = 1.0f;

    public const float GRID_X = 0.5f;
    public const float GRID_Y = 0.5f;

    public const float FIXED_Z = 0f;

    // マウスが属するセル番号（左下基準）
    public static Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int gx = Mathf.FloorToInt(worldPos.x / GRID_X);
        int gy = Mathf.FloorToInt(worldPos.y / GRID_Y);
        return new Vector2Int(gx, gy);
    }

    // セル番号 → セル中心のワールド座標
    public static Vector3 GridToWorld(Vector2Int cell)
    {
        return new Vector3(
            (cell.x + 0.5f) * GRID_X,
            (cell.y + 0.5f) * GRID_Y,
            FIXED_Z
        );
    }

    // ワールド → セル中心へスナップ
    public static Vector3 SnapToGrid(Vector3 worldPos)
    {
        return GridToWorld(WorldToGrid(worldPos));
    }

}

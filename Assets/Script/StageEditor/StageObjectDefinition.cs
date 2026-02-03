using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//グリッドの大きさとオブジェクトの大きさを合わせる
//オブジェクト１つでグリッド一つ、けどコイン起きにくくなるのでそこら辺の制限をきめること
//オブジェクトの大きさを決めること
//おけるとこだけ色付ける
//public enum StageObjectType
//{
//    Drum_Normal,
//    Drum_FlipOn,
//    Drum_MoveOn,    
//    Drum_Goal,
//    Drum_Break,
//    Coin,
//    Enemy
//}

//Definitionの共通設定なのでStagePlacedObjectとは分ける。

[CreateAssetMenu(menuName = "StageEditor/Object Definition")]
public class StageObjectDefinition : ScriptableObject
{
    //オブジェクト自体が持つ情報
    //StagePlacedObjectが参照してる。
    public GameObject prefab;
    public Vector2Int size = new Vector2Int(1, 1);
    public bool isDrum;

}

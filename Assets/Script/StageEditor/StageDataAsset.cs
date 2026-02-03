using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;


[CreateAssetMenu(menuName = "Stage/StageDataAsset", fileName = "StageData")]
public class StageDataAsset : ScriptableObject
{
    [Serializable]
    public class Item
    {
        public StageObjectDefinition definition;
        public int x;
        public int y;
        public int z; // 使わないなら常に0でOK（将来拡張用）
    }

    [SerializeField] private List<Item> items = new();
    public IReadOnlyList<Item> Items => items;

    public void Clear() => items.Clear();

    public void SetItems(IEnumerable<Item> src)
    {
        items = src.Select(i => new Item
        {
            definition = i.definition,
            x = i.x,
            y = i.y,
            z = i.z
        }).ToList();
    }
    public static Transform GetFirstDrumTransform()
    {
        StagePlacedObject best = null;

        foreach (var p in UnityEngine.Object.FindObjectsOfType<StagePlacedObject>())
        {
            if (p == null || p.definition == null) continue;
            if (!p.definition.isDrum) continue;

            if (best == null)
            {
                best = p;
                continue;
            }

            if (p.baseCell.x < best.baseCell.x ||
                (p.baseCell.x == best.baseCell.x && p.baseCell.y < best.baseCell.y))
            {
                best = p;
            }
        }

        return best != null ? best.transform : null;
    }

}

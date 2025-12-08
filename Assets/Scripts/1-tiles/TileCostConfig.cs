using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class TileCostEntry
{
    public TileBase tile;          // Tile type (plain, hill, mountain, water…)
    public float baseCost = 1f;
    public float costWithGoat = 1f;
    public float costWithBoat = 1f;

    [Tooltip("If true: cannot be crossed without a goat.")]
    public bool blocksWithoutGoat; // mountain without goat = blocked

    [Tooltip("If true: cannot be crossed without a boat.")]
    public bool blocksWithoutBoat; // water without boat = blocked

    [Tooltip("Completely unwalkable in every case (wall, decoration, etc.).")]
    public bool alwaysBlocked;     // never walkable
}

public class TileCostConfig : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileCostEntry[] entries;

    private Dictionary<TileBase, TileCostEntry> dict;
    private PlayerInventory inventory;

    [Obsolete]
    private void Awake()
    {
        inventory = FindObjectOfType<PlayerInventory>();

        dict = new Dictionary<TileBase, TileCostEntry>();
        foreach (var e in entries)
        {
            if (e.tile != null && !dict.ContainsKey(e.tile))
            {
                dict.Add(e.tile, e);
            }
        }
    }

    // returns true if this cell is walkable, given the player's inventory.
    public bool IsWalkable(Vector3Int cell)
    {
        TileBase tile = tilemap.GetTile(cell);
        if (tile == null) return false;

        if (!dict.TryGetValue(tile, out var entry))
            return false; // not configured = not walkable

        // blocked
        if (entry.alwaysBlocked)
            return false;

        // blocked depending on inventory
        if (entry.blocksWithoutGoat && (inventory == null || !inventory.hasGoat))
            return false;

        if (entry.blocksWithoutBoat && (inventory == null || !inventory.hasBoat))
            return false;

        return true;
    }

    // Returns the movement cost for this cell.
    // If not walkable -> Mathf.Infinity.
    public float GetCost(Vector3Int cell)
    {
        TileBase tile = tilemap.GetTile(cell);
        if (tile == null) return Mathf.Infinity;

        if (!dict.TryGetValue(tile, out var entry))
            return Mathf.Infinity;

        // if absolutely blocked, return infinite cost
        if (entry.alwaysBlocked)
            return Mathf.Infinity;

        float cost = entry.baseCost;

        if (inventory != null)
        {
            if (inventory.hasGoat && entry.costWithGoat > 0f)
                cost = entry.costWithGoat;

            if (inventory.hasBoat && entry.costWithBoat > 0f)
                cost = entry.costWithBoat;
        }

        // never allow cost <= 0
        if (cost <= 0f)
            cost = 1f;

        return cost;
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

/**
 * Keeps a list of allowed tiles.
 * Adds extra tiles depending on the player's inventory.
 */
public class AllowedTiles : MonoBehaviour
{
    [Header("Always walkable")]
    [SerializeField] private TileBase[] baseAllowedTiles = null;

    [Header("Water tiles (require boat)")]
    [SerializeField] private TileBase[] waterTiles = null;

    [Header("Mountain tiles (require goat)")]
    [SerializeField] private TileBase[] mountainTiles = null;

    private PlayerInventory inventory;

    private void Awake()
    {
        inventory = FindObjectOfType<PlayerInventory>();
    }

    public TileBase[] Get()
    {
        var list = new List<TileBase>();

        if (baseAllowedTiles != null)
            list.AddRange(baseAllowedTiles);

        if (inventory != null)
        {
            if (inventory.hasBoat && waterTiles != null)
                list.AddRange(waterTiles);

            if (inventory.hasGoat && mountainTiles != null)
                list.AddRange(mountainTiles);
        }

        return list.ToArray();
    }

    public bool Contains(TileBase tile)
    {
        return Get().Contains(tile);
    }
}


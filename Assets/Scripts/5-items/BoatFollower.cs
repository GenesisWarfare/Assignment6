using UnityEngine;
using UnityEngine.Tilemaps;

public class BoatFollower : MonoBehaviour
{
    [Header("Map settings")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase[] waterTiles;

    [Header("Boat visual")]
    [SerializeField] private GameObject boatPrefab;

    private PlayerInventory inventory;
    private GameObject boatInstance;
    private Vector3Int lastWaterCell;
    // private bool hasLastWaterCell = false;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }
    private void Update()
    {
        if (!inventory.hasBoat)
        {
            if (boatInstance != null)
                boatInstance.SetActive(false);
            return;
        }

        // one instance
        if (boatInstance == null)
        {
            boatInstance = Instantiate(boatPrefab);
            boatInstance.SetActive(false);
        }

        Vector3Int cell = tilemap.WorldToCell(transform.position);
        TileBase tile = tilemap.GetTile(cell);

        bool isOnWater = IsWaterTile(tile);

        if (isOnWater)
        {
            // the boat appear
            boatInstance.SetActive(true);
            boatInstance.transform.position = tilemap.GetCellCenterWorld(cell);
        }
        else
        {
            // the boat disappear
            boatInstance.SetActive(false);
        }
    }

    private bool IsWaterTile(TileBase tile)
    {
        if (tile == null) return false;
        foreach (var t in waterTiles)
            if (t == tile)
                return true;
        return false;
    }

}

using UnityEngine;
using UnityEngine.Tilemaps;

public class GoatFollower : MonoBehaviour
{
    [Header("Map settings")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase[] mountainTiles;

    [Header("Goat visual")]
    [SerializeField] private GameObject goatPrefab;

    private PlayerInventory inventory;
    private GameObject goatInstance;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        if (tilemap == null || goatPrefab == null || inventory == null)
            return;

        if (!inventory.hasGoat)
        {
            if (goatInstance != null)
                goatInstance.SetActive(false);
            return;
        }

        // one instance
        if (goatInstance == null)
        {
            goatInstance = Instantiate(goatPrefab, Vector3.zero, Quaternion.identity);
            goatInstance.SetActive(false);
        }

        Vector3Int currentCell = tilemap.WorldToCell(transform.position);
        TileBase currentTile = tilemap.GetTile(currentCell);

        bool isOnMountain = IsMountainTile(currentTile);

        if (isOnMountain)
        {
            // the goat appear
            goatInstance.SetActive(true);
            goatInstance.transform.position = tilemap.GetCellCenterWorld(currentCell);
        }
        else
        {
            // the goat disapear
            goatInstance.SetActive(false);
        }
    }

    private bool IsMountainTile(TileBase tile)
    {
        if (tile == null || mountainTiles == null)
            return false;

        for (int i = 0; i < mountainTiles.Length; i++)
        {
            if (mountainTiles[i] == tile)
                return true;
        }
        return false;
    }
}

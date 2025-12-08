using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

/**
 * This component allows the player to move to a point on the screen by clicking it.
 * Uses A* (via TargetMover) to find the path.
 * With a pickaxe, the player can break mountains using keys:
 * A = left, W = up, D = right, X = down, S = current tile.
 */
public class ClickMover : TargetMover
{
    [SerializeField] private InputAction moveTo = new InputAction(type: InputActionType.Button);

    [SerializeField]
    [Tooltip("Determine the location to 'moveTo'.")]
    private InputAction moveToLocation = new InputAction(type: InputActionType.Value, expectedControlType: "Vector2");

    [Header("Pickaxe settings")]
    [Tooltip("Tile used as 'floor' after breaking a mountain.")]
    [SerializeField] private TileBase grassTile;

    [Tooltip("Tiles considered as mountains (breakable with pickaxe).")]
    [SerializeField] private TileBase[] mountainTiles;

    [Tooltip("Tilemap used for mining (usually the same as in TargetMover).")]
    [SerializeField] private Tilemap clickTilemap;

    private PlayerInventory inventory;

    [System.Obsolete]
    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();

        if (clickTilemap == null)
            clickTilemap = FindObjectOfType<Tilemap>();
    }

    private void OnValidate()
    {
        // Mouse bindings for click-to-move
        if (moveTo.bindings.Count == 0)
            moveTo.AddBinding("<Mouse>/leftButton");
        if (moveToLocation.bindings.Count == 0)
            moveToLocation.AddBinding("<Mouse>/position");
    }

    private void OnEnable()
    {
        moveTo.Enable();
        moveToLocation.Enable();
    }

    private void OnDisable()
    {
        moveTo.Disable();
        moveToLocation.Disable();
    }

    private void Update()
    {
        // 1) Handle pickaxe with A/W/D/X/S
        HandlePickaxeInput();

        // 2) Handle click → A* movement
        if (!moveTo.WasPerformedThisFrame())
            return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(moveToLocation.ReadValue<Vector2>());
        mouseWorld.z = 0f;

        SetTarget(mouseWorld);   // A* 
    }

    // Reads keyboard A/W/D/X/S and, if player has pickaxe, mines the corresponding tile
    private void HandlePickaxeInput()
    {
        if (inventory == null || !inventory.hasPickaxe || clickTilemap == null)
            return;

        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        Vector3Int playerCell = clickTilemap.WorldToCell(transform.position);
        Vector3Int targetCell = playerCell;
        bool keyPressed = false;

        if (keyboard.aKey.wasPressedThisFrame)        // left
        {
            targetCell += new Vector3Int(-1, 0, 0);
            keyPressed = true;
        }
        else if (keyboard.dKey.wasPressedThisFrame)   // right
        {
            targetCell += new Vector3Int(1, 0, 0);
            keyPressed = true;
        }
        else if (keyboard.wKey.wasPressedThisFrame)   // up
        {
            targetCell += new Vector3Int(0, 1, 0);
            keyPressed = true;
        }
        else if (keyboard.xKey.wasPressedThisFrame)   // down
        {
            targetCell += new Vector3Int(0, -1, 0);
            keyPressed = true;
        }
        else if (keyboard.sKey.wasPressedThisFrame)   // current tile
        {
            // targetCell = playerCell
            keyPressed = true;
        }

        if (!keyPressed)
            return;

        TileBase tile = clickTilemap.GetTile(targetCell);
        if (tile != null && System.Array.IndexOf(mountainTiles, tile) >= 0)
        {
            clickTilemap.SetTile(targetCell, grassTile);
        }
    }
}


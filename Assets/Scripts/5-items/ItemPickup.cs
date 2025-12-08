using UnityEngine;

public enum ItemType
{
    Boat,
    Goat,
    Pickaxe
}

public class ItemPickup : MonoBehaviour
{
    public ItemType itemType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var inventory = other.GetComponent<PlayerInventory>();
        if (inventory == null) return;

        switch (itemType)
        {
            case ItemType.Boat:
                inventory.hasBoat = true;
                break;
            case ItemType.Goat:
                inventory.hasGoat = true;
                break;
            case ItemType.Pickaxe:
                inventory.hasPickaxe = true;
                break;
        }

        Destroy(gameObject); // the item disapear
    }
}

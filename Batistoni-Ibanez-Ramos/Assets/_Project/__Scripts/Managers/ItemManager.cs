using UnityEngine;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    [Header("Listas de Objetos (Por Tier)")]
    public List<ItemData> commonItems;
    public List<ItemData> rareItems;
    public List<ItemData> extraordinaryItems;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public ItemData GetRandomItem(ItemTier targetTier)
    {
        List<ItemData> pool = null;

        switch (targetTier)
        {
            case ItemTier.Comun: pool = commonItems; break;
            case ItemTier.Raro: pool = rareItems; break;
            case ItemTier.Extraordinario: pool = extraordinaryItems; break;
        }

        if (pool != null && pool.Count > 0)
        {
            return pool[Random.Range(0, pool.Count)];
        }

        return null;
    }
}
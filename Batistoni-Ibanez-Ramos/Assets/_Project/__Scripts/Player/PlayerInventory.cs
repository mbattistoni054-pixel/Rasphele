using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public Dictionary<ItemData, int> collectedItems = new Dictionary<ItemData, int>();

    public void AddItem(ItemData item)
    {
        if (collectedItems.ContainsKey(item))
        {
            collectedItems[item]++;
        }
        else
        {
            collectedItems.Add(item, 1);
        }

        Debug.Log($"Objeto adquirido: {item.itemName} (Total: {collectedItems[item]})");
        RecalculateItemStats();
    }

    private void RecalculateItemStats()
    {
        if (PlayerStats.Instance == null) return;

        PlayerStats.Instance.ResetItemBonuses();

        foreach (var kvp in collectedItems)
        {
            ItemData item = kvp.Key;
            int count = kvp.Value;

            switch (item.effect)
            {
                case ItemEffect.VidaFlat:
                    PlayerStats.Instance.itemHealthFlat += item.value * count;
                    break;
                case ItemEffect.VidaPorcentaje:
                    // MATEMÁTICA MULTIPLICATIVA: Toma el 100% (1f), le suma el buff, y lo eleva a la cantidad de objetos
                    PlayerStats.Instance.itemHealthMultiplier *= Mathf.Pow(1f + (item.value / 100f), count);
                    break;

                case ItemEffect.VelocidadFlat:
                    PlayerStats.Instance.itemSpeedFlat += item.value * count;
                    break;
                case ItemEffect.VelocidadPorcentaje:
                    PlayerStats.Instance.itemSpeedMultiplier *= Mathf.Pow(1f + (item.value / 100f), count);
                    break;

                case ItemEffect.RegenMovimiento:
                    PlayerStats.Instance.itemRegenMoving += item.value * count;
                    break;
                case ItemEffect.ExperienciaExtra:
                    PlayerStats.Instance.itemXpMultiplier *= Mathf.Pow(1f + (item.value / 100f), count);
                    break;

                case ItemEffect.SaltoExtra:
                    PlayerStats.Instance.itemExtraJumps += Mathf.RoundToInt(item.value) * count;
                    break;
                case ItemEffect.DashExtra:
                    PlayerStats.Instance.itemExtraDashes += Mathf.RoundToInt(item.value) * count;
                    break;

                case ItemEffect.EscudoInactividad:
                    PlayerStats.Instance.shieldStacks += count;
                    break;

                case ItemEffect.Apostador:
                    // Toma el valor (2) y lo eleva a la cantidad.
                    // Si tienes 1 anzuelo: 2^1 = x2 daño y dinero.
                    // Si tienes 2 anzuelos: 2^2 = x4 daño y dinero.
                    PlayerStats.Instance.itemDamageTakenMultiplier *= Mathf.Pow(item.value, count);
                    PlayerStats.Instance.itemMoneyMultiplier *= Mathf.Pow(item.value, count);
                    break;

                case ItemEffect.Ahorrador:
                    PlayerStats.Instance.goldBagStacks += count;
                    break;
            }
        }

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null) health.UpdateMaxHealthFromStats();
    }
}
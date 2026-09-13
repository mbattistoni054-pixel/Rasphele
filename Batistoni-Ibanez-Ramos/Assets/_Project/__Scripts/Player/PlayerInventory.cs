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
        if (PatronesAplicados.PlayerStatsRefactored.Instance == null) return;

        PatronesAplicados.PlayerStatsRefactored.Instance.ResetItemBonuses();

        foreach (var kvp in collectedItems)
        {
            ItemData item = kvp.Key;
            int count = kvp.Value;

            switch (item.effect)
            {
                case ItemEffect.VidaFlat:
                    PatronesAplicados.PlayerStatsRefactored.Instance.itemHealthFlat += item.value * count;
                    break;
                case ItemEffect.VidaPorcentaje:
                    // MATEMÁTICA MULTIPLICATIVA: Toma el 100% (1f), le suma el buff, y lo eleva a la cantidad de objetos
                    PatronesAplicados.PlayerStatsRefactored.Instance.itemHealthMultiplier *= Mathf.Pow(1f + (item.value / 100f), count);
                    break;

                case ItemEffect.VelocidadFlat:
                    PatronesAplicados.PlayerStatsRefactored.Instance.itemSpeedFlat += item.value * count;
                    break;
                case ItemEffect.VelocidadPorcentaje:
                    PatronesAplicados.PlayerStatsRefactored.Instance.itemSpeedMultiplier *= Mathf.Pow(1f + (item.value / 100f), count);
                    break;

                case ItemEffect.RegenMovimiento:
                    PatronesAplicados.PlayerStatsRefactored.Instance.itemRegenMoving += item.value * count;
                    break;
                case ItemEffect.ExperienciaExtra:
                    PatronesAplicados.PlayerStatsRefactored.Instance.itemXpMultiplier *= Mathf.Pow(1f + (item.value / 100f), count);
                    break;

                case ItemEffect.SaltoExtra:
                    PatronesAplicados.PlayerStatsRefactored.Instance.itemExtraJumps += Mathf.RoundToInt(item.value) * count;
                    break;
                case ItemEffect.DashExtra:
                    PatronesAplicados.PlayerStatsRefactored.Instance.itemExtraDashes += Mathf.RoundToInt(item.value) * count;
                    break;

                case ItemEffect.EscudoInactividad:
                    PatronesAplicados.PlayerStatsRefactored.Instance.shieldStacks += count;
                    break;

                case ItemEffect.Apostador:
                    // Toma el valor (2) y lo eleva a la cantidad.
                    // Si tienes 1 anzuelo: 2^1 = x2 daño y dinero.
                    // Si tienes 2 anzuelos: 2^2 = x4 daño y dinero.
                    PatronesAplicados.PlayerStatsRefactored.Instance.itemDamageTakenMultiplier *= Mathf.Pow(item.value, count);
                    PatronesAplicados.PlayerStatsRefactored.Instance.itemMoneyMultiplier *= Mathf.Pow(item.value, count);
                    break;

                case ItemEffect.Ahorrador:
                    PatronesAplicados.PlayerStatsRefactored.Instance.goldBagStacks += count;
                    break;
            }
        }

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null) health.UpdateMaxHealthFromStats();

        PatronesAplicados.PlayerHealthRefactored newHealth = GetComponent<PatronesAplicados.PlayerHealthRefactored>();
        if (newHealth != null) newHealth.UpdateMaxHealthFromStats();
    }
}

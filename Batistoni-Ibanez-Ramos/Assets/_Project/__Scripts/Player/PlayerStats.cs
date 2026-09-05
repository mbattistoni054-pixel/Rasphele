using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public delegate void MoneyChangedHandler(int currentMoney);
    public static event MoneyChangedHandler OnMoneyChanged;

    [Header("Multiplicadores Globales (Armas)")]
    public float globalDamageMultiplier = 1f;
    public float globalFireRateMultiplier = 1f;
    public float globalRangeMultiplier = 1f;
    public float globalSpeedMultiplier = 1f;

    [Header("Estadísticas Base del Jugador")]
    public float baseMaxHealth = 100f;
    public float baseSpeed = 8f;
    public int currentMoney = 0;

    [Header("Bonos Acumulados por Objetos")]
    public float itemHealthFlat = 0f;
    public float itemHealthMultiplier = 1f;
    public float itemSpeedFlat = 0f;
    public float itemSpeedMultiplier = 1f;
    public float itemRegenMoving = 0f;
    public float itemXpMultiplier = 1f;
    public int itemExtraJumps = 0;
    public int itemExtraDashes = 0;
    public float itemDamageTakenMultiplier = 1f;
    public float itemMoneyMultiplier = 1f;
    public int goldBagStacks = 0;
    public int shieldStacks = 0;
    public float weaponSpeedFlat = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public float GetTotalMaxHealth()
    {
        return (baseMaxHealth + itemHealthFlat) * itemHealthMultiplier;
    }

    public float GetTotalSpeed(float currentMovementStateSpeed)
    {
        return (currentMovementStateSpeed + itemSpeedFlat) * itemSpeedMultiplier * globalSpeedMultiplier;
    }

    public void AddWeaponSpeedFlat(float amount) { weaponSpeedFlat += amount; }
    public void AddGlobalDamage(float percent) { globalDamageMultiplier *= (1f + (percent / 100f)); }
    public void AddGlobalSpeed(float percent) { globalSpeedMultiplier *= (1f + (percent / 100f)); }
    public void ReduceGlobalCooldown(float percent) { globalFireRateMultiplier *= (1f - (percent / 100f)); }

    public void ResetItemBonuses()
    {
        itemHealthFlat = 0f; itemHealthMultiplier = 1f; itemSpeedFlat = 0f;
        itemSpeedMultiplier = 1f; itemRegenMoving = 0f; itemXpMultiplier = 1f;
        itemExtraJumps = 0; itemExtraDashes = 0; itemDamageTakenMultiplier = 1f;
        itemMoneyMultiplier = 1f; goldBagStacks = 0; shieldStacks = 0;
    }

    public void AddMoney(int amount)
    {
        float totalAmount = amount * itemMoneyMultiplier;

        if (goldBagStacks > 0)
        {
            int hundreds = Mathf.Min(currentMoney, 2000) / 100;
            int bonusPerHundred = 2 * goldBagStacks;
            totalAmount += (hundreds * bonusPerHundred);
        }

        currentMoney += Mathf.CeilToInt(totalAmount);

        OnMoneyChanged?.Invoke(currentMoney);
    }

    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;

            OnMoneyChanged?.Invoke(currentMoney);

            return true;
        }
        return false;
    }
}
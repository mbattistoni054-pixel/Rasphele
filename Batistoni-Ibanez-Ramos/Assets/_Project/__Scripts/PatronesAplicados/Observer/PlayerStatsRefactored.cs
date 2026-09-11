using UnityEngine;

namespace PatronesAplicados
{
    public class PlayerStatsRefactored : MonoBehaviour
    {
        public static PlayerStatsRefactored Instance;

        [Header("Multiplicadores Globales (Armas)")]
        public float globalDamageMultiplier = 1f;
        public float globalFireRateMultiplier = 1f;
        public float globalRangeMultiplier = 1f;
        public float globalSpeedMultiplier = 1f;

        [Header("Estadsticas Base del Jugador")]
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

        void Start()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StartListening<int>("EnemyKilled_GoldReward", AddMoney);
                EventManager.Instance.StartListening<int, System.Action<bool>>("RequestSpendMoney", HandleSpendMoneyRequest);
                EventManager.Instance.StartListening<System.Action<PlayerStatsRefactored>>("RequestCurrentStats", HandleStatsRequest);
                EventManager.Instance.StartListening<float>("AddGlobalSpeed", AddGlobalSpeed);
                EventManager.Instance.StartListening<float>("AddWeaponSpeedFlat", AddWeaponSpeedFlat);
            }
        }

        void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.StopListening<int>("EnemyKilled_GoldReward", AddMoney);
                EventManager.Instance.StopListening<int, System.Action<bool>>("RequestSpendMoney", HandleSpendMoneyRequest);
                EventManager.Instance.StopListening<System.Action<PlayerStatsRefactored>>("RequestCurrentStats", HandleStatsRequest);
                EventManager.Instance.StopListening<float>("AddGlobalSpeed", AddGlobalSpeed);
                EventManager.Instance.StopListening<float>("AddWeaponSpeedFlat", AddWeaponSpeedFlat);
            }
        }

        private void HandleSpendMoneyRequest(int amount, System.Action<bool> callback)
        {
            bool success = SpendMoney(amount);
            callback?.Invoke(success);
        }

        private void HandleStatsRequest(System.Action<PlayerStatsRefactored> callback)
        {
            callback?.Invoke(this);
        }

        public void NotifyStatsUpdated()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent("PlayerStatsUpdated");
            }
        }

        public float GetTotalMaxHealth()
        {
            return (baseMaxHealth + itemHealthFlat) * itemHealthMultiplier;
        }

        public float GetTotalSpeed(float currentMovementStateSpeed)
        {
            return (currentMovementStateSpeed + itemSpeedFlat) * itemSpeedMultiplier * globalSpeedMultiplier;
        }

        public void AddWeaponSpeedFlat(float amount) { weaponSpeedFlat += amount; NotifyStatsUpdated(); }
        public void AddGlobalDamage(float percent) { globalDamageMultiplier *= (1f + (percent / 100f)); NotifyStatsUpdated(); }
        public void AddGlobalSpeed(float percent) { globalSpeedMultiplier *= (1f + (percent / 100f)); NotifyStatsUpdated(); }
        public void ReduceGlobalCooldown(float percent) { globalFireRateMultiplier *= (1f - (percent / 100f)); NotifyStatsUpdated(); }

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

            // DESACOPLAMIENTO: Usamos el Event Bus en lugar de un delegate esttico
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent("MoneyUpdated", currentMoney);
            }
        }

        public bool SpendMoney(int amount)
        {
            if (currentMoney >= amount)
            {
                currentMoney -= amount;

                // DESACOPLAMIENTO
                if (EventManager.Instance != null)
                {
                    EventManager.Instance.TriggerEvent("MoneyUpdated", currentMoney);
                }

                return true;
            }
            return false;
        }
    }
}

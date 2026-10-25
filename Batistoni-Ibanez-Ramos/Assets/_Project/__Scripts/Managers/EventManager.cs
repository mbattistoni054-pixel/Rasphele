using System;
using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados
{
    public static class GameEventKeys
    {
        public const string PlayerDeath = "PlayerDeath";
        public const string PlayerHealthChanged = "PlayerHealthChanged";
        public const string ExperienceChanged = "ExperienceChanged";
        public const string DashCooldownChanged = "DashCooldownChanged";
        public const string MoneyUpdated = "MoneyUpdated";
        public const string RequestSpendMoney = "RequestSpendMoney";

        public const string LevelComplete = "LevelComplete";
        public const string PauseRequested = "PauseRequested";
        public const string ResumeRequested = "ResumeRequested";
        public const string TimeUpdated = "TimeUpdated";
        public const string RequestCurrentStats = "RequestCurrentStats";
        public const string PlayerStatsUpdated = "PlayerStatsUpdated";

        public const string EnemyKilledGoldReward = "EnemyKilled_GoldReward";
        public const string EnemyDied = "EnemyDied";

        public const string BossSpawned = "BossSpawned";
        public const string BossHealthChanged = "BossHealthChanged";
        public const string BossDefeated = "BossDefeated";

        public const string RequestUpgradeProfile = "RequestUpgradeProfile";
        public const string RequestGlobalRandomUpgrades = "RequestGlobalRandomUpgrades";
        public const string ApplyUpgrade = "ApplyUpgrade";
        public const string RequestPurgeCost = "RequestPurgeCost";
        public const string PurgeUpgrade = "PurgeUpgrade";
        public const string RequestAllAvailableUpgrades = "RequestAllAvailableUpgrades";
        public const string RequestChosenUpgrades = "RequestChosenUpgrades";

        public const string AddGlobalSpeed = "AddGlobalSpeed";
        public const string AddWeaponSpeedFlat = "AddWeaponSpeedFlat";
    }

    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        private readonly Dictionary<string, Delegate> _eventDictionary = new Dictionary<string, Delegate>(32, StringComparer.Ordinal);

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void StartListening(string eventName, Action listener) => AddListener(eventName, listener);
        public void StartListening<T>(string eventName, Action<T> listener) => AddListener(eventName, listener);
        public void StartListening<T1, T2>(string eventName, Action<T1, T2> listener) => AddListener(eventName, listener);
        public void StartListening<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener) => AddListener(eventName, listener);

        private void AddListener(string eventName, Delegate listener)
        {
            if (string.IsNullOrEmpty(eventName) || listener == null) return;

            if (_eventDictionary.TryGetValue(eventName, out Delegate thisEvent) && thisEvent != null)
            {
                if (thisEvent.GetType() != listener.GetType())
                {
                    Debug.LogError($"[EventManager] Conflicto de tipos en el evento '{eventName}'. Registrado: {thisEvent.GetType().Name}, Intentado: {listener.GetType().Name}");
                    return;
                }
                _eventDictionary[eventName] = Delegate.Combine(thisEvent, listener);
            }
            else
            {
                _eventDictionary[eventName] = listener;
            }
        }

        public void StopListening(string eventName, Action listener) => RemoveListener(eventName, listener);
        public void StopListening<T>(string eventName, Action<T> listener) => RemoveListener(eventName, listener);
        public void StopListening<T1, T2>(string eventName, Action<T1, T2> listener) => RemoveListener(eventName, listener);
        public void StopListening<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener) => RemoveListener(eventName, listener);

        private void RemoveListener(string eventName, Delegate listener)
        {
            if (Instance == null || string.IsNullOrEmpty(eventName) || listener == null) return;

            if (_eventDictionary.TryGetValue(eventName, out Delegate thisEvent) && thisEvent != null)
            {
                if (thisEvent.GetType() != listener.GetType()) return;

                Delegate currentDel = Delegate.Remove(thisEvent, listener);
                if (currentDel == null)
                {
                    _eventDictionary.Remove(eventName);
                }
                else
                {
                    _eventDictionary[eventName] = currentDel;
                }
            }
        }

        public void TriggerEvent(string eventName)
        {
            if (_eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
            {
                (thisEvent as Action)?.Invoke();
            }
        }

        public void TriggerEvent<T>(string eventName, T parameter)
        {
            if (_eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
            {
                (thisEvent as Action<T>)?.Invoke(parameter);
            }
        }

        public void TriggerEvent<T1, T2>(string eventName, T1 param1, T2 param2)
        {
            if (_eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
            {
                (thisEvent as Action<T1, T2>)?.Invoke(param1, param2);
            }
        }

        public void TriggerEvent<T1, T2, T3>(string eventName, T1 param1, T2 param2, T3 param3)
        {
            if (_eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
            {
                (thisEvent as Action<T1, T2, T3>)?.Invoke(param1, param2, param3);
            }
        }
    }
}
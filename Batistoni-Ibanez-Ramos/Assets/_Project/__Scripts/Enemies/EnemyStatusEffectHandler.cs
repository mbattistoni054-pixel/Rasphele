using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados
{

    public class EnemyStatusEffectHandler
    {
        private class DoTData
        {
            public float timeLeft;
            public float tickTimer;
            public float value;
        }

        private readonly Dictionary<int, DoTData> activeBurns = new Dictionary<int, DoTData>();
        private readonly Dictionary<int, DoTData> activeBleeds = new Dictionary<int, DoTData>();
        private readonly Dictionary<int, DoTData> activePoisons = new Dictionary<int, DoTData>();
        private readonly List<float> activeSlows = new List<float>();


        private readonly List<int> _keysBuffer = new List<int>(8);

        public bool IsStunned { get; private set; }
        private float stunImmunityTimer = 0f;

        public void ResetAll()
        {
            activeBurns.Clear();
            activeBleeds.Clear();
            activePoisons.Clear();
            activeSlows.Clear();
            _keysBuffer.Clear();
            IsStunned = false;
            stunImmunityTimer = 0f;
        }

        public void Tick(float deltaTime, float currentHealth, Action<float, bool, DamageType, bool> onDealDamage)
        {
            if (stunImmunityTimer > 0f)
                stunImmunityTimer -= deltaTime;

            ProcessDoTDictionary(activeBurns, deltaTime, false, currentHealth, DamageType.Fuego, false, onDealDamage);
            ProcessDoTDictionary(activeBleeds, deltaTime, true, currentHealth, DamageType.Fisico, true, onDealDamage);
            ProcessDoTDictionary(activePoisons, deltaTime, false, currentHealth, DamageType.Veneno, false, onDealDamage);
        }

        private void ProcessDoTDictionary(
            Dictionary<int, DoTData> dict,
            float deltaTime,
            bool isPercentOfCurrentHealth,
            float currentHealth,
            DamageType damageType,
            bool isBleed,
            Action<float, bool, DamageType, bool> onDealDamage)
        {
            if (dict.Count == 0) return;

            _keysBuffer.Clear();
            foreach (var kvp in dict)
            {
                _keysBuffer.Add(kvp.Key);
            }

            for (int i = 0; i < _keysBuffer.Count; i++)
            {
                int key = _keysBuffer[i];
                if (!dict.TryGetValue(key, out DoTData dot)) continue;

                dot.timeLeft -= deltaTime;
                dot.tickTimer -= deltaTime;

                if (dot.tickTimer <= 0f)
                {
                    dot.tickTimer += 1f;
                    float dmg = isPercentOfCurrentHealth ? currentHealth * (dot.value / 100f) : dot.value;
                    onDealDamage?.Invoke(dmg, false, damageType, isBleed);
                }

                if (dot.timeLeft <= 0f)
                {
                    dict.Remove(key);
                }
            }
        }

        public void ApplyBurn(float burnDamage, int weaponID) => AddOrRefreshDoT(activeBurns, weaponID, burnDamage);
        public void ApplyBleed(float bleedPercent, int weaponID) => AddOrRefreshDoT(activeBleeds, weaponID, bleedPercent);
        public void ApplyPoison(float poisonDamage, int weaponID) => AddOrRefreshDoT(activePoisons, weaponID, poisonDamage);

        private void AddOrRefreshDoT(Dictionary<int, DoTData> dict, int weaponID, float val)
        {
            if (!dict.TryGetValue(weaponID, out DoTData dot))
            {
                dict[weaponID] = new DoTData { timeLeft = 3f, tickTimer = 1f, value = val };
            }
            else
            {
                dot.timeLeft = 3f;
                dot.value = val;
            }
        }

        public bool CanApplyFreeze() => activeSlows.Count < 3;

        public IEnumerator FreezeRoutine(float speedReduction, Action onRecalculateSpeed)
        {
            activeSlows.Add(speedReduction);
            onRecalculateSpeed?.Invoke();

            yield return new WaitForSeconds(5f);

            activeSlows.Remove(speedReduction);
            onRecalculateSpeed?.Invoke();
        }

        public float CalculateCurrentSpeed(float baseSpeed)
        {
            float speed = baseSpeed;
            for (int i = 0; i < activeSlows.Count; i++)
            {
                speed -= activeSlows[i];
            }

            float minSpeed = baseSpeed * 0.1f;
            return speed < minSpeed ? minSpeed : speed;
        }

        public bool TryRollStun(float stunChance)
        {
            if (IsStunned || stunImmunityTimer > 0f) return false;
            return UnityEngine.Random.Range(0f, 100f) <= stunChance;
        }

        public IEnumerator StunRoutine(Action<bool> onStunStateChanged)
        {
            IsStunned = true;
            onStunStateChanged?.Invoke(true);

            yield return new WaitForSeconds(2f);

            IsStunned = false;
            onStunStateChanged?.Invoke(false);
            stunImmunityTimer = 5f;
        }
    }
}
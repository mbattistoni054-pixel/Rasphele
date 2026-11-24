using System;
using System.Collections.Generic;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{
    public class EnemyStatusEffectHandler
    {
        public const int BurnEffectId = 1;
        public const int BleedEffectId = 2;
        public const int PoisonEffectId = 3;

        private const int MaxFreezeStacks = 3;
        private const float FreezeDuration = 5f;
        private const float StunDuration = 2f;
        private const float StunImmunityDuration = 5f;
        private const float DoTDuration = 3f;
        private const float DoTTickInterval = 1f;

        // Lista polimórfica genérica (Cumple OCP)
        private readonly List<IStatusEffect> _activeEffects = new List<IStatusEffect>(12);
        private int _nextSlowStackId = 1;

        public bool IsStunned { get; private set; }
        private float _stunImmunityTimer;

        private Action _onRecalculateSpeed;
        private Action<bool> _onStunStateChanged;
        private readonly Action<bool> _internalStunHandler;

        public EnemyStatusEffectHandler()
        {
            _internalStunHandler = HandleInternalStunChanged;
        }

        public void BindCallbacks(Action onRecalculateSpeed, Action<bool> onStunStateChanged)
        {
            _onRecalculateSpeed = onRecalculateSpeed;
            _onStunStateChanged = onStunStateChanged;
        }

        public void ResetAll()
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                _activeEffects[i].ReleaseToPool();
            }
            _activeEffects.Clear();

            IsStunned = false;
            _stunImmunityTimer = 0f;
            _nextSlowStackId = 1;
        }

        /// <summary>
        /// Método abierto a extensión (OCP): permite registrar cualquier implementación nueva
        /// de IStatusEffect (ej. Electrocutado, Corrosión) sin modificar esta clase.
        /// </summary>
        public void AddEffect(IStatusEffect effect)
        {
            if (effect == null) return;

            StatusEffectContext context = CreateContext(0f, null);
            _activeEffects.Add(effect);
            effect.OnApply(in context);
        }

        public void ApplyAllEffects(
            in WeaponData.ImpactEffects effects,
            int sourceWeaponID,
            float baseSpeed,
            Action onRecalculateSpeed = null,
            Action<bool> onStunStateChanged = null)
        {
            if (onRecalculateSpeed != null) _onRecalculateSpeed = onRecalculateSpeed;
            if (onStunStateChanged != null) _onStunStateChanged = onStunStateChanged;

            if (effects.burnDamage > 0f) ApplyBurn(effects.burnDamage, sourceWeaponID);
            if (effects.bleedPercent > 0f) ApplyBleed(effects.bleedPercent, sourceWeaponID);
            if (effects.poisonDamage > 0f) ApplyPoison(effects.poisonDamage, sourceWeaponID);

            if (effects.freezePercent > 0f && CanApplyFreeze())
            {
                float speedReduction = baseSpeed * (effects.freezePercent / 100f);
                ApplyFreezeInstant(speedReduction);
            }

            if (effects.stunChance > 0f && TryRollStun(effects.stunChance))
            {
                ApplyStunInstant();
            }
        }

        public void Tick(float deltaTime, float currentHealth, Action<float, bool, DamageType, bool> onDealDamage)
        {
            if (!IsStunned && _stunImmunityTimer > 0f)
            {
                _stunImmunityTimer -= deltaTime;
            }

            if (_activeEffects.Count == 0) return;

            StatusEffectContext context = CreateContext(currentHealth, onDealDamage);

            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                IStatusEffect effect = _activeEffects[i];
                effect.Tick(deltaTime, in context);

                if (effect.IsExpired)
                {
                    _activeEffects.RemoveAt(i);
                    effect.OnExpire(in context);
                    effect.ReleaseToPool();
                }
            }
        }

        public void ApplyBurn(float burnDamage, int weaponID)
        {
            AddOrRefreshDoT(BurnEffectId, weaponID, burnDamage, DamageType.Fuego, false, false);
        }

        public void ApplyBleed(float bleedPercent, int weaponID)
        {
            AddOrRefreshDoT(BleedEffectId, weaponID, bleedPercent, DamageType.Fisico, true, true);
        }

        public void ApplyPoison(float poisonDamage, int weaponID)
        {
            AddOrRefreshDoT(PoisonEffectId, weaponID, poisonDamage, DamageType.Veneno, false, false);
        }

        private void AddOrRefreshDoT(
            int effectTypeId,
            int weaponId,
            float value,
            DamageType damageType,
            bool scalesWithCurrentHealth,
            bool isBleedVisual)
        {
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                if (_activeEffects[i].EffectTypeId == effectTypeId &&
                    _activeEffects[i].SourceId == weaponId &&
                    _activeEffects[i] is DamageOverTimeEffect existingDoT)
                {
                    existingDoT.Refresh(value, DoTDuration);
                    return;
                }
            }

            DamageOverTimeEffect newDoT = StatusEffectPool<DamageOverTimeEffect>.Get()
                .Setup(effectTypeId, weaponId, value, DoTDuration, DoTTickInterval, damageType, scalesWithCurrentHealth, isBleedVisual);

            AddEffect(newDoT);
        }

        public bool CanApplyFreeze()
        {
            int slowCount = 0;
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                if (_activeEffects[i].EffectTypeId == SlowStatusEffect.TypeId)
                {
                    slowCount++;
                }
            }
            return slowCount < MaxFreezeStacks;
        }

        public void ApplyFreezeInstant(float speedReduction)
        {
            if (!CanApplyFreeze()) return;

            SlowStatusEffect slow = StatusEffectPool<SlowStatusEffect>.Get()
                .Setup(_nextSlowStackId++, speedReduction, FreezeDuration);

            AddEffect(slow);
        }

        public float CalculateCurrentSpeed(float baseSpeed)
        {
            float speed = baseSpeed;
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                _activeEffects[i].ModifySpeed(ref speed, baseSpeed);
            }

            float minSpeed = baseSpeed * 0.1f;
            return speed < minSpeed ? minSpeed : speed;
        }

        public bool TryRollStun(float stunChance)
        {
            if (IsStunned || _stunImmunityTimer > 0f) return false;
            return UnityEngine.Random.Range(0f, 100f) <= stunChance;
        }

        public void ApplyStunInstant()
        {
            if (IsStunned) return;

            StunStatusEffect stun = StatusEffectPool<StunStatusEffect>.Get().Setup(StunDuration);
            AddEffect(stun);
        }

        private void HandleInternalStunChanged(bool stunned)
        {
            IsStunned = stunned;
            if (!stunned)
            {
                _stunImmunityTimer = StunImmunityDuration;
            }
            _onStunStateChanged?.Invoke(stunned);
        }

        private StatusEffectContext CreateContext(float currentHealth, Action<float, bool, DamageType, bool> onDealDamage)
        {
            return new StatusEffectContext(
                currentHealth,
                onDealDamage,
                _onRecalculateSpeed,
                _internalStunHandler);
        }
    }
}
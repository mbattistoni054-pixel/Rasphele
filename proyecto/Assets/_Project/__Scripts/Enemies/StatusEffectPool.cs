using System;
using System.Collections.Generic;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{
    public readonly struct StatusEffectContext
    {
        public readonly float CurrentHealth;
        public readonly Action<float, bool, DamageType, bool> DealDamageCallback;
        public readonly Action RecalculateSpeedCallback;
        public readonly Action<bool> StunStateChangedCallback;

        public StatusEffectContext(
            float currentHealth,
            Action<float, bool, DamageType, bool> dealDamageCallback,
            Action recalculateSpeedCallback,
            Action<bool> stunStateChangedCallback)
        {
            CurrentHealth = currentHealth;
            DealDamageCallback = dealDamageCallback;
            RecalculateSpeedCallback = recalculateSpeedCallback;
            StunStateChangedCallback = stunStateChangedCallback;
        }
    }


    public static class StatusEffectPool<T> where T : class, IStatusEffect, new()
    {
        private static readonly Stack<T> _pool = new Stack<T>(64);

        public static T Get() => _pool.Count > 0 ? _pool.Pop() : new T();

        public static void Release(T instance)
        {
            if (instance != null) _pool.Push(instance);
        }
    }

    public class DamageOverTimeEffect : IStatusEffect
    {
        public int EffectTypeId { get; private set; }
        public int SourceId { get; private set; }
        public bool IsExpired => _timeLeft <= 0f;

        private float _value;
        private float _duration;
        private float _timeLeft;
        private float _tickInterval;
        private float _tickTimer;
        private DamageType _damageType;
        private bool _scalesWithCurrentHealth;
        private bool _isBleedVisual;

        public DamageOverTimeEffect Setup(int effectTypeId, int sourceWeaponId, float value, float duration, float tickInterval, DamageType damageType, bool scalesWithCurrentHealth, bool isBleedVisual)
        {
            EffectTypeId = effectTypeId;
            SourceId = sourceWeaponId;
            _value = value;
            _duration = duration;
            _timeLeft = duration;
            _tickInterval = tickInterval;
            _tickTimer = tickInterval;
            _damageType = damageType;
            _scalesWithCurrentHealth = scalesWithCurrentHealth;
            _isBleedVisual = isBleedVisual;
            return this;
        }

        public void Refresh(float newValue, float newDuration)
        {
            _value = newValue;
            _duration = newDuration;
            _timeLeft = newDuration;
        }

        public void OnApply(in StatusEffectContext context) { }

        public void Tick(float deltaTime, in StatusEffectContext context)
        {
            _timeLeft -= deltaTime;
            _tickTimer -= deltaTime;

            if (_tickTimer <= 0f)
            {
                _tickTimer += _tickInterval;
                float dmg = _scalesWithCurrentHealth
                    ? context.CurrentHealth * (_value / 100f)
                    : _value;

                context.DealDamageCallback?.Invoke(dmg, false, _damageType, _isBleedVisual);
            }
        }

        public void OnExpire(in StatusEffectContext context) { }

        public void ModifySpeed(ref float currentSpeed, float baseSpeed) { }

        public void ReleaseToPool() => StatusEffectPool<DamageOverTimeEffect>.Release(this);
    }

    public class SlowStatusEffect : IStatusEffect
    {
        public const int TypeId = 100;

        public int EffectTypeId => TypeId;
        public int SourceId { get; private set; }
        public bool IsExpired => _timeLeft <= 0f;

        private float _speedReduction;
        private float _timeLeft;

        public SlowStatusEffect Setup(int stackInstanceId, float speedReduction, float duration)
        {
            SourceId = stackInstanceId;
            _speedReduction = speedReduction;
            _timeLeft = duration;
            return this;
        }

        public void OnApply(in StatusEffectContext context)
        {
            context.RecalculateSpeedCallback?.Invoke();
        }

        public void Tick(float deltaTime, in StatusEffectContext context)
        {
            _timeLeft -= deltaTime;
        }

        public void OnExpire(in StatusEffectContext context)
        {
            context.RecalculateSpeedCallback?.Invoke();
        }

        public void ModifySpeed(ref float currentSpeed, float baseSpeed)
        {
            currentSpeed -= _speedReduction;
        }

        public void ReleaseToPool() => StatusEffectPool<SlowStatusEffect>.Release(this);
    }


    public class StunStatusEffect : IStatusEffect
    {
        public const int TypeId = 200;

        public int EffectTypeId => TypeId;
        public int SourceId => 0;
        public bool IsExpired => _timeLeft <= 0f;

        private float _timeLeft;

        public StunStatusEffect Setup(float duration)
        {
            _timeLeft = duration;
            return this;
        }

        public void OnApply(in StatusEffectContext context)
        {
            context.StunStateChangedCallback?.Invoke(true);
        }

        public void Tick(float deltaTime, in StatusEffectContext context)
        {
            _timeLeft -= deltaTime;
        }

        public void OnExpire(in StatusEffectContext context)
        {
            context.StunStateChangedCallback?.Invoke(false);
        }

        public void ModifySpeed(ref float currentSpeed, float baseSpeed) { }

        public void ReleaseToPool() => StatusEffectPool<StunStatusEffect>.Release(this);
    }
}
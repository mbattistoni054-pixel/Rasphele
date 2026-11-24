using System;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public interface IUpgradeStrategy
    {
        void Execute(WeaponBaseRealRefactored weapon, float value, bool isRecalculation);
    }

    public sealed class WeaponStatUpgradeStrategy : IUpgradeStrategy
    {
        private readonly WeaponStatModifier _reusableModifier = new WeaponStatModifier();
        private readonly Action<WeaponStatModifier, float> _configureAction;

        public WeaponStatUpgradeStrategy(Action<WeaponStatModifier, float> configureAction)
        {
            _configureAction = configureAction ?? throw new ArgumentNullException(nameof(configureAction));
        }

        public void Execute(WeaponBaseRealRefactored weapon, float value, bool isRecalculation)
        {
            if (weapon == null) return;

            _reusableModifier.Reset();
            _configureAction(_reusableModifier, value);

            weapon.ApplyUpgrade(_reusableModifier);
        }
    }

    public sealed class PlayerHealUpgradeStrategy : IUpgradeStrategy
    {
        private Transform _cachedPlayerTransform;
        private IHealable _cachedPlayerHealable;
        private PlayerHealthRefactored _cachedPlayerHealth;

        public void Execute(WeaponBaseRealRefactored weapon, float value, bool isRecalculation)
        {
            if (isRecalculation || value <= 0f) return;

            Transform currentTransform = GameManagerRefactored.Instance.Player;

            if (currentTransform == null) return;

            if (_cachedPlayerTransform != currentTransform || (_cachedPlayerHealable == null && _cachedPlayerHealth == null))
            {
                _cachedPlayerTransform = currentTransform;
                currentTransform.TryGetComponent(out _cachedPlayerHealable);
                currentTransform.TryGetComponent(out _cachedPlayerHealth);
            }

            if (_cachedPlayerHealable != null)
            {
                _cachedPlayerHealable.Heal(value);
            }
            else if (_cachedPlayerHealth != null)
            {
                _cachedPlayerHealth.Heal(value);
            }
        }
    }

    public sealed class PlayerSpeedEventUpgradeStrategy : IUpgradeStrategy
    {
        private readonly string _eventKey;

        public PlayerSpeedEventUpgradeStrategy(string eventKey)
        {
            _eventKey = eventKey;
        }

        public void Execute(WeaponBaseRealRefactored weapon, float value, bool isRecalculation)
        {
            if (isRecalculation || EventManager.Instance == null || string.IsNullOrEmpty(_eventKey)) return;

            EventManager.Instance.TriggerEvent(_eventKey, value);
        }
    }

    public sealed class WeaponBehaviorUpgradeStrategy<TWeapon> : IUpgradeStrategy where TWeapon : WeaponBaseRealRefactored
    {
        private readonly Action<TWeapon, float> _applyBehavior;

        public WeaponBehaviorUpgradeStrategy(Action<TWeapon, float> applyBehavior)
        {
            _applyBehavior = applyBehavior ?? throw new ArgumentNullException(nameof(applyBehavior));
        }

        public void Execute(WeaponBaseRealRefactored weapon, float value, bool isRecalculation)
        {
            if (weapon is TWeapon typedWeapon)
            {
                typedWeapon.EnsureInitialized();
                _applyBehavior(typedWeapon, value);
            }
        }
    }
}
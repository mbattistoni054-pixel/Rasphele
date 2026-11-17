using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public readonly struct ProjectilePayload
    {
        public readonly Transform Target;
        public readonly float FinalDamage;
        public readonly float Speed;
        public readonly int ChainBounces;
        public readonly float ExplosiveRadius;
        public readonly LayerMask EnemyMask;
        public readonly WeaponData.ImpactEffects Effects;
        public readonly int WeaponID;
        public readonly DamageType DamageType;

        public ProjectilePayload(Transform target, float finalDamage, float speed, int chainBounces, float explosiveRadius, LayerMask enemyMask, WeaponData.ImpactEffects effects, int weaponId, DamageType damageType)
        {
            Target = target;
            FinalDamage = finalDamage;
            Speed = speed;
            ChainBounces = chainBounces;
            ExplosiveRadius = explosiveRadius;
            EnemyMask = enemyMask;
            Effects = effects;
            WeaponID = weaponId;
            DamageType = damageType;
        }
    }
}
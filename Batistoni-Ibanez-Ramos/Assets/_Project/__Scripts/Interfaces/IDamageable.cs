public interface IDamageable
{

    void TakeDamage(float amount, bool isCrit, DamageType type, bool isBleed = false);


    void ApplyEffects(WeaponData.ImpactEffects effects, int sourceWeaponID);
}
public interface IEffectReceivable
{
    void ApplyEffects(WeaponData.ImpactEffects effects, int sourceWeaponID);
}



public static class DamageableEffectExtensions
{
    public static void ApplyEffects(this IDamageable damageable, WeaponData.ImpactEffects effects, int sourceWeaponID)
    {
        if (damageable is IEffectReceivable effectReceivable)
        {
            effectReceivable.ApplyEffects(effects, sourceWeaponID);
        }
    }
}
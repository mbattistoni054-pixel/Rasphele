using PatronesAplicados;

public interface IStatusEffect
{
    /// <summary>Identificador del tipo de efecto (ej. Burn, Bleed, Poison, Slow, Stun).</summary>
    int EffectTypeId { get; }

    /// <summary>Identificador de la fuente/arma para permitir apilamiento por arma o global.</summary>
    int SourceId { get; }

    /// <summary>Indica si el efecto ya terminó su duración.</summary>
    bool IsExpired { get; }

    /// <summary>Se ejecuta al aplicarse por primera vez.</summary>
    void OnApply(in StatusEffectContext context);

    /// <summary>Se ejecuta en cada Update mientras esté activo.</summary>
    void Tick(float deltaTime, in StatusEffectContext context);

    /// <summary>Se ejecuta al expirar o limpiarse.</summary>
    void OnExpire(in StatusEffectContext context);

    /// <summary>Permite que el efecto modifique la velocidad del enemigo.</summary>
    void ModifySpeed(ref float currentSpeed, float baseSpeed);

    /// <summary>Libera la instancia de vuelta a su pool estático para lograr 0 GC.</summary>
    void ReleaseToPool();
}
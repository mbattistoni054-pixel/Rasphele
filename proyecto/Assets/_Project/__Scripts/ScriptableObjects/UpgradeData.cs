using UnityEngine;

public enum UpgradeCategory { Standard, Firing, Impact }

public enum UpgradeType
{
    Explosive, Chain, Multiple, FireRate, Range,
    Damage, Bleed, Burn, Freeze, Stun, Crit, Poison,
    Heal,
    Duration,
    ElectricStorm,
    PlayerSpeed,
    HeatHeal,
    CometMode,
    ScatterMode,
    PlayerSpeedFlat,
    MaxDamageCap,     // Daño Máximo (Reemplaza el valor)
    RampUpInterval,   // Velocidad de escalado (0.9, 0.8...) (Reemplaza el valor)
    RangeMultiplier,   // Distancia de disparo (Multiplicador directo)
    MagicMode,        // Cambia a daño Mágico
    ProjectileSpeed
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "ScriptableObjects/Upgrade Card")]
public class UpgradeData : ScriptableObject
{
    [Header("Exclusividad")]
    [Tooltip("Arrastra un WeaponData aquí (ej. Data_Dron) si esta mejora es exclusiva. Déjalo vacío si es para todas.")]
    public WeaponData exclusiveWeapon;

    [Header("Identificación")]
    public string baseName = "Nombre de Mejora";
    public Sprite icon;
    public UpgradeCategory category;
    public UpgradeType type;

    [Header("Progresión por Niveles (Arrays)")]
    [Tooltip("La cantidad de elementos aquí define el Nivel Máximo de la mejora.")]
    public float[] levelValues;

    [TextArea]
    [Tooltip("Descripción que se mostrará en cada nivel.")]
    public string[] levelDescriptions;
}
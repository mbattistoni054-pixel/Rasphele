using UnityEngine;

public enum DamageType { Fisico, Magico, Fuego, Agua, Veneno, Electrico }

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "ScriptableObjects/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [System.Serializable]
    public struct ImpactEffects
    {
        public float critChance;
        public float bleedPercent;
        public float burnDamage;
        public float freezePercent;
        public float stunChance;
        public float poisonDamage;
    }

    [Header("Límites de Mejoras (Cupos)")]
    [Tooltip("Máximo de mejoras de Comportamiento (Disparo) para ESTA arma")]
    public int maxFiringUpgrades = 2;

    [Tooltip("Máximo de mejoras de Efecto (Impacto) para ESTA arma")]
    public int maxImpactUpgrades = 3;

    [Header("Información Básica")]
    public string weaponName;
    public Sprite weaponIcon;
    public GameObject projectilePrefab;

    [Header("Audio")]
    public AudioClip attackSound;

    [Header("Atributos de Daño")]
    public DamageType damageType = DamageType.Fisico;

    [Header("Estadísticas Base")]
    public float baseDamage = 10f;
    public float baseCooldown = 1f;
    public float baseRange = 15f;
    public float baseDuration = 0f;

    [Header("Mejoras de Disparo")]
    public int multipleShots = 1;
    public int chainBounces = 0;
    public float explosiveRadius = 0f;

    [Header("Mejoras Elementales")]
    public int electricStormChance = 0;

    [Header("Efectos de Impacto Base")]
    public ImpactEffects baseEffects;
}
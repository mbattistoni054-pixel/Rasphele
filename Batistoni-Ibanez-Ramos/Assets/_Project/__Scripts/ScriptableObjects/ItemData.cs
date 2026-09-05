using UnityEngine;

public enum ItemTier { Comun, Raro, Extraordinario }

public enum ItemEffect
{
    VidaFlat, VidaPorcentaje,
    VelocidadFlat, VelocidadPorcentaje,
    RegenMovimiento, ExperienciaExtra,
    EscudoInactividad, SaltoExtra, DashExtra,
    Apostador, Ahorrador
}

[CreateAssetMenu(fileName = "NewItem", menuName = "ScriptableObjects/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Información del Objeto")]
    public string itemName;
    public ItemTier tier;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Mecánica")]
    public ItemEffect effect;
    [Tooltip("El valor numérico del efecto. Ej: 2 para el Café. 0.1 para el 10% del Mate.")]
    public float value;
}
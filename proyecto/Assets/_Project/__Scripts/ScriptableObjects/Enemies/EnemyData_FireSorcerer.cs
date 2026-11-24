using UnityEngine;

[CreateAssetMenu(fileName = "Fire Sorcerer", menuName = "ScriptableObjects/Enemy Data/Fire Sorcerer")]
public class EnemyData_FireSorcerer : EnemyData_Base
{

    [Header("Fire Sorcerer")]

    [SerializeField] private GameObject fireLinePrefab;
    public GameObject FireLinePrefab => fireLinePrefab;

    [Tooltip("Duración en segundos de la línea de fuego activa.")]
    [SerializeField] private float fireLineDuration = 10f;
    public float FireLineDuration => fireLineDuration;

    [Tooltip("Tiempo en segundos del aviso antes de que salga el fuego.")]
    [SerializeField] private float fireLineWarningTime = 1f;
    public float FireLineWarningTime => fireLineWarningTime;

    [Tooltip("Longitud de la línea de fuego.")]
    [SerializeField] private float fireLineLength = 50f;
    public float FireLineLength => fireLineLength;

}

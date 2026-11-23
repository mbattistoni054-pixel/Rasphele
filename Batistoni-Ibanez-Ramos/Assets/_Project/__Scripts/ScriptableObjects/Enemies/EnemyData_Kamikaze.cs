using UnityEngine;

[CreateAssetMenu(fileName = "Kamikaze", menuName = "ScriptableObjects/Enemy Data/Kamikaze")]
public class EnemyData_Kamikaze : EnemyData_Base
{
    [Header("Kamikaze")]

    [SerializeField] private float explosionRadius;
    public float ExplosionRadius => explosionRadius;

    [SerializeField] private float explosionDelay;
    public float ExplosionDelay => explosionDelay;

    [SerializeField] private GameObject explosionVisualPrefab;
    public GameObject ExplosionVisualPrefab => explosionVisualPrefab;
}

using UnityEngine;

[CreateAssetMenu(fileName = "Golem", menuName = "ScriptableObjects/Enemy Data/Golem")]
public class EnemyData_Golem : EnemyData_Base
{
    [Header("Golem")]

    [SerializeField] private float attackRadius;
    public float AttackRadius => attackRadius;

    [SerializeField] private float hitDelay;
    public float HitDelay => hitDelay;

    [SerializeField] private float knockbackHorizontal;
    public float KnockbackHorizontal => knockbackHorizontal;

    [SerializeField] private float knockbackUpward;
    public float KnockbackUpward => knockbackUpward;

    [SerializeField] private GameObject slamVisualPrefab;
    public GameObject SlamVisualPrefab => slamVisualPrefab;

    [SerializeField] public float knockbackStunDuration = 0.4f;
    public float KnockbackStunDuration => knockbackStunDuration;


}

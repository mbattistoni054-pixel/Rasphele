using UnityEngine;

[CreateAssetMenu(fileName = "Boss Plant", menuName = "ScriptableObjects/Enemy Data/Boss Plant")]
public class EnemyData_BossPlant : EnemyData_Base
{
    [Header("Boss Plant")]

    [SerializeField] private GameObject orbPrefab;
    public GameObject OrbPrefab => orbPrefab;

    [SerializeField] private int orbsPerBurst;
    public int OrbsPerBurst => orbsPerBurst;

    [SerializeField] private float timeBetweenOrbs;
    public float TimeBetweenOrbs => timeBetweenOrbs;

    [SerializeField] private GameObject rootTrapPrefab;
    public GameObject RootTrapPrefab => rootTrapPrefab;

    [SerializeField] private int orbsPerBurstPhase2;
    public int OrbsPerBurstPhase2 => orbsPerBurstPhase2;

    [SerializeField] private int rootsPhase2;
    public int RootsPhase2 => rootsPhase2;

    [SerializeField] private float delayBetweenRootsPhase2;
    public float DelayBetweenRootsPhase2 => delayBetweenRootsPhase2;

    [SerializeField] private float meleeRange;
    public float MeleeRange => meleeRange;

    [SerializeField] private float meleeDamage;
    public float MeleeDamage => meleeDamage;

    [SerializeField] private float bossKnockbackHorizontal;
    public float BossKnockbackHorizontal => bossKnockbackHorizontal;

    [SerializeField] private float bossKnockbackUpward;
    public float BossKnockbackUpward => bossKnockbackUpward;

    [SerializeField] private float meleeHitDelay;
    public float MeleeHitDelay => meleeHitDelay;
}

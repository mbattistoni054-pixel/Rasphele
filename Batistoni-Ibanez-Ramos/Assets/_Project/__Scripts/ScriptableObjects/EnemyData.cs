using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "ScriptableObjects/Enemy Data")]

public class EnemyData : ScriptableObject
{

    [Header("Base Stats")]
    [SerializeField] private float maxHealth;
    public float MaxHealth => maxHealth;
    [SerializeField] private float baseSpeed;
    public float BaseSpeed => baseSpeed;
    [SerializeField] private float attackDamage;
    public float AttackDamage => attackDamage;
    [SerializeField] private float attackCooldown;
    public float AttackCooldown => attackCooldown;
    [SerializeField] private float rangeAttack;
    public float RangeAttack => rangeAttack;
    [SerializeField] private float fleeDistance;
    public float FleeDistance => fleeDistance;


    [Header("Drops")]
    [SerializeField] private int goldReward;
    public int GoldReward => goldReward;
    [SerializeField] private int expReward;
    public int ExpReward => expReward;


    [Header("Range")]
    [SerializeField] private GameObject bullet;
    public GameObject Bullet => bullet;
    [SerializeField] private int bulletSpeed;
    public int BulletSpeed => bulletSpeed;


    [Header("Kamikaze")]
    [SerializeField] private float explosionRadius;
    public float ExplosionRadius => explosionRadius;
    [SerializeField] private float explosionDelay;
    public float ExplosionDelay => explosionDelay;
    [SerializeField] private GameObject explosionVisualPrefab;
    public GameObject ExplosionVisualPrefab => explosionVisualPrefab;
    

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


    [Header("Magic Sorcerer")]
    [SerializeField] private float trackingTime;
    public float TrackingTime => trackingTime;
    [SerializeField] private float lockedTime;
    public float LockedTime => lockedTime;
    [SerializeField] private float laserDuration;
    public float LaserDuration => laserDuration;
    [SerializeField] private Material beamMaterial;
    public Material BeamMaterial => beamMaterial;


    [Header("Fire Sorcerer")]
    [SerializeField] private GameObject fireLinePrefab;
    public GameObject FireLinePrefab => fireLinePrefab;


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

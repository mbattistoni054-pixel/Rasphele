using UnityEngine;

public class EnemyData_Base : ScriptableObject
{
    [Header("Base Stats")]

    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;

    [SerializeField] private float baseSpeed = 3.5f;
    public float BaseSpeed => baseSpeed;

    [SerializeField] private float attackDamage = 10f;
    public float AttackDamage => attackDamage;

    [SerializeField] private float attackCooldown = 1.5f;
    public float AttackCooldown => attackCooldown;

    [SerializeField] private float rangeAttack = 2f;
    public float RangeAttack => rangeAttack;

    [SerializeField] private float fleeDistance = 0f;
    public float FleeDistance => fleeDistance;


    [Header("Base Drops")]

    [SerializeField] private int goldReward = 5;
    public int GoldReward => goldReward;

    [SerializeField] private int expReward = 20;
    public int ExpReward => expReward;

    [SerializeField] private GameObject xpOrb;
    public GameObject XpOrb => xpOrb;

    [SerializeField] private GameObject popUp;
    public GameObject PopUp => popUp;
}

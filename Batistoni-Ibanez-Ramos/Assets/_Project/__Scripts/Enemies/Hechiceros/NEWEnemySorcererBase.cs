using PatronesAplicados;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public abstract class NEWEnemySorcererBase : EnemyBaseRefactored
{

    [Header("Animacin")]
    public Animator animator;
    protected bool hasMoveParam = false;

    protected float lastAttackTime = -10f;
    protected bool isChargingAttack = false;
    protected bool isFleeing = false;
    protected float pathTimer = 0f;

    public override void ResetStats()
    {
        base.ResetStats();
        isChargingAttack = false;
        isFleeing = false;
    }

    protected override void Start()
    {
        base.Start();
        if (animator == null) animator = GetComponent<Animator>();

        if (animator != null)
        {
            foreach (var param in animator.parameters)
            {
                if (param.name == "Move") hasMoveParam = true;
            }
        }

        if (agent != null)
        {
            agent.acceleration = 40f;
            agent.angularSpeed = 400f;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (player != null && !isStunned && agent != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer < data.FleeDistance && !isChargingAttack)
            {
                FleeFromPlayer();
                return;
            }

            if (distanceToPlayer > data.RangeAttack && !isChargingAttack)
            {
                ApproachPlayer();
                return;
            }

            if (distanceToPlayer <= data.RangeAttack && Time.time >= lastAttackTime + data.AttackCooldown && !isChargingAttack)
            {
                StartAttackRoutine();
                return;
            }

            if (!isChargingAttack && distanceToPlayer >= data.FleeDistance && distanceToPlayer <= data.RangeAttack)
            {
                agent.isStopped = true;
                if (animator != null && hasMoveParam) animator.SetBool("Move", false);
                LookAtPlayer();
            }
        }
    }

    protected void FleeFromPlayer()
    {
        isFleeing = true;
        agent.isStopped = false;

        Vector3 dirAwayFromPlayer = (transform.position - player.position).normalized;
        
        Vector3 fleePosition = transform.position + dirAwayFromPlayer * 10f;

        if (Time.time >= pathTimer)
        {
            agent.SetDestination(fleePosition);
            pathTimer = Time.time + 0.2f; 
        }

        if (animator != null && hasMoveParam) animator.SetBool("Move", true);
    }

    protected void ApproachPlayer()
    {
        isFleeing = false;
        agent.isStopped = false;

        if (Time.time >= pathTimer)
        {
            agent.SetDestination(player.position);
            pathTimer = Time.time + 0.2f;
        }

        if (animator != null && hasMoveParam) animator.SetBool("Move", true);
    }

    protected void LookAtPlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        dirToPlayer.y = 0; 

        if (dirToPlayer != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dirToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    protected abstract void StartAttackRoutine();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.RangeAttack);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, data.FleeDistance);
    }
}


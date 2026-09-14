using PatronesAplicados;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public abstract class NEWEnemySorcererBase : EnemyBaseRefactored
{
    //[Header("Comportamiento de Hechicero")]
    //public float attackRange = 40f;   // Distancia a la que se frena para atacar
    //public float fleeDistance = 20f;  // Si el jugador entra en esta zona, el hechicero huye
   // public float attackCooldown = 10f; // Tiempo entre ataques

    [Header("Animacin")]
    public Animator animator;
    protected bool hasMoveParam = false;

    protected float lastAttackTime = -10f; // Para que pueda atacar inmediatamente al aparecer
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

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        if (player != null && !isStunned && agent != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // 1. Huida (Fleeing)
            if (distanceToPlayer < data.FleeDistance && !isChargingAttack)
            {
                FleeFromPlayer();
                return;
            }

            // 2. Acercarse al Rango de Ataque
            if (distanceToPlayer > data.RangeAttack && !isChargingAttack)
            {
                ApproachPlayer();
                return;
            }

            // 3. Atacar si est en rango y termin el cooldown
            if (distanceToPlayer <= data.RangeAttack && Time.time >= lastAttackTime + data.AttackCooldown && !isChargingAttack)
            {
                StartAttackRoutine();
                return;
            }

            // Si est en rango de ataque (pero no demasiado cerca) y en Cooldown -> Se queda quieto mirndote
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

        // Calculamos la direccin opuesta al jugador
        Vector3 dirAwayFromPlayer = (transform.position - player.position).normalized;
        
        // Buscamos un punto lejos en esa direccin
        Vector3 fleePosition = transform.position + dirAwayFromPlayer * 10f;

        if (Time.time >= pathTimer)
        {
            agent.SetDestination(fleePosition);
            pathTimer = Time.time + 0.2f; // Actualizamos el path cada 0.2s para que no sature
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
        dirToPlayer.y = 0; // Para que no se incline hacia arriba o abajo

        if (dirToPlayer != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dirToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    // Este mtodo debe ser sobreescrito por cada tipo de hechicero (fuego, mgico, etc.)
    protected abstract void StartAttackRoutine();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.RangeAttack);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, data.FleeDistance);
    }
}


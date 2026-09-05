using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public abstract class EnemySorcererBase : EnemyBase
{
    [Header("Comportamiento de Hechicero")]
    public float attackRange = 40f;   // Distancia a la que se frena para atacar
    public float fleeDistance = 20f;  // Si el jugador entra en esta zona, el hechicero huye
    public float attackCooldown = 10f; // Tiempo entre ataques

    [Header("Animación")]
    public Animator animator;
    protected bool hasMoveParam = false;

    protected float lastAttackTime = -10f; // Para que pueda atacar inmediatamente al aparecer
    protected bool isChargingAttack = false;
    protected bool isFleeing = false;
    protected float pathTimer = 0f;

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
            float distance = Vector3.Distance(player.position, transform.position);

            // SI ESTÁ CARGANDO UN ATAQUE
            if (isChargingAttack)
            {
                agent.isStopped = true;
                if (animator != null && hasMoveParam) animator.SetBool("Move", false);

                // Lo hacemos rotar suavemente hacia el jugador para que no falle feo
                Vector3 dir = (player.position - transform.position).normalized;
                dir.y = 0;
                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);

                return;
            }

            // DECIDIR SI HUYE
            if (distance <= fleeDistance)
            {
                isFleeing = true;
            }
            else if (distance >= attackRange)
            {
                isFleeing = false; // Ya se alejó lo suficiente
            }


            if (isFleeing)
            {

                agent.stoppingDistance = 0f; // CLAVE: Frenado en 0 para que corra libremente
                agent.isStopped = false;

                if (animator != null && hasMoveParam) animator.SetBool("Move", true);

                if (Time.time >= pathTimer)
                {
                    // Busca un punto en la dirección completamente opuesta al jugador
                    Vector3 dirAway = (transform.position - player.position).normalized;
                    dirAway.y = 0;
                    Vector3 targetPos = transform.position + dirAway * 15f;

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(targetPos, out hit, 15f, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                    }
                    pathTimer = Time.time + 0.2f;
                }
            }
            else
            {
                agent.stoppingDistance = attackRange - 1f; // Restauramos la distancia de frenado

                if (distance <= attackRange)
                {
                    agent.isStopped = true;
                    if (animator != null && hasMoveParam) animator.SetBool("Move", false);

                    if (Time.time >= lastAttackTime + attackCooldown)
                    {
                        StartCoroutine(PerformAttackRoutine());
                    }
                    else
                    {
                        // Está en cooldown, se queda mirando al jugador de lejos
                        Vector3 dir = (player.position - transform.position).normalized;
                        dir.y = 0;
                        if (dir != Vector3.zero)
                            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
                    }
                }
                else
                {
                    // Si el jugador está muy lejos, se acerca para entrar en rango
                    agent.isStopped = false;
                    if (animator != null && hasMoveParam) animator.SetBool("Move", true);

                    if (Time.time >= pathTimer)
                    {
                        agent.SetDestination(player.position);
                        pathTimer = Time.time + 0.2f;
                    }
                }
            }
        }
    }

    protected abstract IEnumerator PerformAttackRoutine();
}
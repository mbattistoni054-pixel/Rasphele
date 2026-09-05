using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemyGolem : EnemyBase
{
    [Header("Estadísticas del Golem")]
    public float attackDamage = 30f;
    public float attackRange = 3f;
    public float attackRadius = 4.5f;

    [Tooltip("Tiempo que tarda la animación desde que empieza hasta que los puños tocan el piso")]
    public float hitDelay = 1.0f;

    public float attackCooldown = 2f;
    public float knockbackHorizontal = 65f;
    public float knockbackUpward = 23f;

    [Header("Efectos")]
    public GameObject slamVisualPrefab;
    public Animator animator;

    private bool isAttacking = false;
    private bool hasMoveParam = false;
    private float pathTimer = 0f;

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
            agent.stoppingDistance = attackRange - 0.5f;
            agent.acceleration = 40f;
            agent.angularSpeed = 300f;
        }
    }

    protected override void Update()
    {
        base.Update(); // Llama a la lógica de veneno y fuego del EnemyBase

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        if (player != null && !isStunned && agent != null)
        {
            float distance = Vector3.Distance(player.position, transform.position);

            // LÓGICA DE MOVIMIENTO
            if (!isAttacking)
            {
                if (distance > attackRange)
                {
                    agent.isStopped = false;

                    if (Time.time >= pathTimer)
                    {
                        agent.SetDestination(player.position);
                        pathTimer = Time.time + 0.2f;
                    }

                    if (animator != null && hasMoveParam) animator.SetBool("Move", true);
                }
                else
                {
                    // Llegó al rango: Frenar y empezar a atacar
                    agent.isStopped = true;
                    if (animator != null && hasMoveParam) animator.SetBool("Move", false);

                    StartCoroutine(SlamAttackRoutine());
                }
            }

            // LÓGICA DE ROTACIÓN
            if (isAttacking && distance <= attackRange * 2f)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                directionToPlayer.y = 0;

                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }
        }
    }

    private IEnumerator SlamAttackRoutine()
    {
        isAttacking = true;

        if (animator != null) animator.SetTrigger("Attack");

        yield return new WaitForSeconds(hitDelay);

        if (isStunned)
        {
            isAttacking = false;
            yield break;
        }


        Vector3 impactCenter = transform.position + (transform.forward * 2.5f);

        if (slamVisualPrefab != null)
        {
            GameObject visual = Instantiate(slamVisualPrefab, impactCenter, Quaternion.identity);
            float visualScale = attackRadius * 2f;
            visual.transform.localScale = new Vector3(visualScale, visualScale, visualScale);
            Destroy(visual, 1f);
        }

        Collider[] hitObjects = Physics.OverlapSphere(impactCenter, attackRadius);
        foreach (Collider hit in hitObjects)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth pHealth = hit.GetComponent<PlayerHealth>();
                if (pHealth != null) pHealth.TakeDamage(attackDamage * damageMultiplier);

                Rigidbody pRb = hit.GetComponent<Rigidbody>();
                if (pRb != null)
                {
                    Vector3 pushDir = (hit.transform.position - transform.position).normalized;
                    pushDir.y = 0;

                    Vector3 finalKnockback = (pushDir * knockbackHorizontal) + (Vector3.up * knockbackUpward);

                    pRb.linearVelocity = Vector3.zero;
                    pRb.AddForce(finalKnockback, ForceMode.Impulse);
                }
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.red;
        Vector3 impactCenter = transform.position + (transform.forward * 2.5f);
        Gizmos.DrawWireSphere(impactCenter, attackRadius);
    }
}
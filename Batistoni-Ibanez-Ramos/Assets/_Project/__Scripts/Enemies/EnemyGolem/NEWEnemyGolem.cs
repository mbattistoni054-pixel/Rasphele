using PatronesAplicados;
using PatronesAplicados.RealImplementation;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NEWEnemyGolem : EnemyBaseRefactored
{

    [Header("Efectos")]
    public GameObject slamVisualPrefab;
    public Animator animator;

    private bool isAttacking = false;
    private bool hasMoveParam = false;
    private float pathTimer = 0f;

    public override void ResetStats()
    {
        base.ResetStats();
        isAttacking = false;
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
            agent.stoppingDistance = data.RangeAttack - 0.5f;
            agent.acceleration = 40f;
            agent.angularSpeed = 300f;
        }
    }

    protected override void Update()
    {
        base.Update(); 

        if (player != null && !isStunned && agent != null)
        {
            float distance = Vector3.Distance(player.position, transform.position);

            if (!isAttacking)
            {
                if (distance > data.RangeAttack)
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
                    agent.isStopped = true;
                    if (animator != null && hasMoveParam) animator.SetBool("Move", false);

                    StartCoroutine(SlamAttackRoutine());
                }
            }

            if (isAttacking && distance <= data.RangeAttack * 2f)
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

        yield return new WaitForSeconds(data.HitDelay);

        if (isStunned)
        {
            isAttacking = false;
            yield break;
        }


        Vector3 impactCenter = transform.position + (transform.forward * 2.5f);

        if (data.SlamVisualPrefab != null)
        {
            GameObject visual;
            if (ProjectilePoolManager.Instance != null)
            {
                visual = ProjectilePoolManager.Instance.GetProjectile(data.SlamVisualPrefab, impactCenter, Quaternion.identity);
            }
            else
            {
                visual = Instantiate(data.SlamVisualPrefab, impactCenter, Quaternion.identity);
            }
            float visualScale = data.AttackRadius * 2f;
            visual.transform.localScale = new Vector3(visualScale, visualScale, visualScale);
        }

        Collider[] hitObjects = Physics.OverlapSphere(impactCenter, data.AttackRadius);
        foreach (Collider hit in hitObjects)
        {
            if (hit.CompareTag("Player"))
            {
                var refHealth = hit.GetComponent<PatronesAplicados.PlayerHealthRefactored>();
                if (refHealth != null) refHealth.TakeDamage(data.AttackDamage * damageMultiplier);
                else {
                    PlayerHealth pHealth = hit.GetComponent<PlayerHealth>();
                    if (pHealth != null) pHealth.TakeDamage(data.AttackDamage * damageMultiplier);
                }

                Rigidbody pRb = hit.GetComponent<Rigidbody>();
                if (pRb != null)
                {
                    Vector3 pushDir = (hit.transform.position - transform.position).normalized;
                    pushDir.y = 0;

                    Vector3 finalKnockback = (pushDir * data.KnockbackHorizontal) + (Vector3.up * data.KnockbackUpward);

                    pRb.linearVelocity = Vector3.zero;
                    pRb.AddForce(finalKnockback, ForceMode.Impulse);
                }
            }
        }

        yield return new WaitForSeconds(data.AttackCooldown);
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.RangeAttack);

        Gizmos.color = Color.red;
        Vector3 impactCenter = transform.position + (transform.forward * 2.5f);
        Gizmos.DrawWireSphere(impactCenter, data.AttackRadius);
    }
}




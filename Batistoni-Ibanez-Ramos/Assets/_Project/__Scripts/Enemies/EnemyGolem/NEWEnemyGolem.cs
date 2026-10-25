using System.Collections;
using UnityEngine;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class NEWEnemyGolem : EnemyBaseRefactored
{
    [Header("Efectos")]
    [Tooltip("Si se deja vacío, usará automáticamente data.SlamVisualPrefab")]
    public GameObject slamVisualPrefab;
    public Animator animator;

    private bool isAttacking = false;
    private bool hasMoveParam = false;
    private float pathTimer = 0f;

    private readonly Collider[] _slamHitsBuffer = new Collider[16];

    public override void ResetStats()
    {
        base.ResetStats();
        isAttacking = false;
        pathTimer = 0f;
    }

    protected override void Start()
    {
        base.Start();
        if (animator == null) animator = GetComponent<Animator>();

        if (animator != null)
        {
            foreach (var param in animator.parameters)
            {
                if (param.name == "Move")
                {
                    hasMoveParam = true;
                    break;
                }
            }
        }

        if (agent != null && data != null)
        {
            agent.stoppingDistance = Mathf.Max(0.5f, data.RangeAttack - 0.5f);
            agent.acceleration = 40f;
            agent.angularSpeed = 300f;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (isDead || data == null) return;

        if (player != null && !isStunned && agent != null && agent.enabled && agent.isOnNavMesh)
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
                directionToPlayer.y = 0f;

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

        if (isDead || isStunned)
        {
            isAttacking = false;
            yield break;
        }

        Vector3 impactCenter = transform.position + (transform.forward * 2.5f);
        GameObject visualPrefabToUse = slamVisualPrefab != null ? slamVisualPrefab : data.SlamVisualPrefab;

        if (visualPrefabToUse != null)
        {
            GameObject visual = null;


            if (VfxPoolManager.Instance != null)
            {
                visual = VfxPoolManager.Instance.GetVfx(visualPrefabToUse, impactCenter, Quaternion.identity, 1.5f);
            }

            if (visual != null)
            {
                float visualScale = data.AttackRadius * 2f;
                visual.transform.localScale = new Vector3(visualScale, visualScale, visualScale);
            }
        }

        int hitCount = Physics.OverlapSphereNonAlloc(impactCenter, data.AttackRadius, _slamHitsBuffer);
        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _slamHitsBuffer[i];
            if (hit != null && hit.CompareTag("Player"))
            {
                float finalDamage = data.AttackDamage * damageMultiplier;

                if (hit.TryGetComponent(out PlayerHealthRefactored refHealth))
                    refHealth.TakeDamage(finalDamage);

                Vector3 pushDir = (hit.transform.position - transform.position).normalized;
                pushDir.y = 0f;
                Vector3 finalKnockback = (pushDir * data.KnockbackHorizontal) + (Vector3.up * data.KnockbackUpward);

                if (hit.TryGetComponent(out PlayerControllerRefactored playerCtrl))
                {
                    playerCtrl.ApplyKnockback(finalKnockback);
                }
            }
        }

        yield return new WaitForSeconds(data.AttackCooldown);
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (data == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.RangeAttack);

        Gizmos.color = Color.red;
        Vector3 impactCenter = transform.position + (transform.forward * 2.5f);
        Gizmos.DrawWireSphere(impactCenter, data.AttackRadius);
    }
}
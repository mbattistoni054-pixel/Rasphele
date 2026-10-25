using UnityEngine;
using PatronesAplicados;

public class NEWCopiaMeleeEnemy : EnemyBaseRefactored
{
    private float lastAttackTime;
    private float lastFrameTime;
    private bool onAttack;

    [SerializeField] private Animator animator;

    private bool hasMoveParam = false;
    private float pathTimer = 0f;

    public override void ResetStats()
    {
        base.ResetStats();
        onAttack = false;
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

        if (agent != null)
        {
            agent.stoppingDistance = 1.5f;
            agent.acceleration = 60f;
            agent.angularSpeed = 600f;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (isDead || data == null) return;

        if (player != null && !isStunned && agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            float distance = Vector3.Distance(player.position, transform.position);

            if (!onAttack && distance > 1.5f)
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
            }

            if (distance <= 2f || onAttack)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                directionToPlayer.y = 0f;

                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
                }
            }

            if (distance <= 1.5f || onAttack)
            {
                if (Time.time >= lastAttackTime + data.AttackCooldown && !onAttack)
                {
                    if (animator != null) animator.SetTrigger("Attack");

                    lastAttackTime = Time.time;
                    lastFrameTime = Time.time;

                    onAttack = true;
                }

                if (Time.time >= lastFrameTime + 0.3f && onAttack)
                {
                    if (distance <= 2f && player.TryGetComponent(out PlayerHealthRefactored newHealth))
                    {
                        newHealth.TakeDamage(data.AttackDamage * damageMultiplier);
                    }

                    lastFrameTime = Time.time;
                    onAttack = false;
                }
            }
        }
    }
}
using PatronesAplicados;
using UnityEngine;

public class NEWCopiaMeleeEnemy : EnemyBaseRefactored
{

    private float lastAttackTime;
    private float lastFrameTime;

    bool onAttack;

    [SerializeField] Animator animator;

    private bool hasMoveParam = false;
    private float pathTimer = 0f;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            foreach (var param in animator.parameters)
            {
                if (param.name == "Move") hasMoveParam = true;
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
                directionToPlayer.y = 0; 

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
                    if (distance <= 2f)
                    {
                        PlayerHealth health = player.GetComponent<PlayerHealth>();

                        if (health != null)
                        {
                            health.TakeDamage(data.AttackDamage * damageMultiplier);
                        }
                        else
                        {
                            PatronesAplicados.PlayerHealthRefactored newHealth = player.GetComponent<PatronesAplicados.PlayerHealthRefactored>();
                            if (newHealth != null) newHealth.TakeDamage(data.AttackDamage * damageMultiplier);
                        }
                    }

                    lastFrameTime = Time.time;
                    onAttack = false;
                }
            }
        }
    }
}

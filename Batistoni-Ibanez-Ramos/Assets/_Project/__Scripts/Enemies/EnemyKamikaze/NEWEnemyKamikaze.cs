using PatronesAplicados;
using PatronesAplicados.RealImplementation;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class NEWEnemyKamikaze : EnemyBaseRefactored
{

    private bool isTriggered = false;
    private Renderer rend;
    [SerializeField] Animator animator;

    private bool hasMoveParam = false;
    private float pathTimer = 0f; 

    public override void ResetStats()
    {
        base.ResetStats();
        isTriggered = false;
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

        rend = GetComponentInChildren<Renderer>();

        if (agent != null)
        {
            agent.stoppingDistance = data.RangeAttack - 0.5f;
            agent.acceleration = 60f;      
            agent.angularSpeed = 600f;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (player != null && !isStunned && agent != null)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            if (!isTriggered)
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
                    if (animator != null && hasMoveParam) animator.SetBool("Move", false);
                    StartCoroutine(KamikazeRoutine());

                }
            }

            if (distance <= data.RangeAttack || isTriggered)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                directionToPlayer.y = 0;

                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 30f);
                }
            }
        }
    }

    private IEnumerator KamikazeRoutine()
    {
        isTriggered = true;

        if (agent != null) agent.isStopped = true;
        if (animator != null && hasMoveParam) animator.SetBool("Move", false);

        float timer = data.ExplosionDelay;

        while (timer > 0)
        {
            if (rend != null)
            {
                rend.material.color = (Mathf.FloorToInt(timer * 8) % 2 == 0) ? Color.red : Color.yellow;
            }
            yield return new WaitForSeconds(0.125f);
            timer -= 0.125f;
        }

        Explode();
    }

    private void Explode()
    {
        if (data.ExplosionVisualPrefab != null)
        {
            GameObject visual;
            if (ProjectilePoolManager.Instance != null)
            {
                visual = ProjectilePoolManager.Instance.GetProjectile(data.ExplosionVisualPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                visual = Instantiate(data.ExplosionVisualPrefab, transform.position, Quaternion.identity);
            }
            
            float visualScale = data.ExplosionRadius * 2f;
            visual.transform.localScale = new Vector3(visualScale, visualScale, visualScale);
        }

        Collider[] hitObjects = Physics.OverlapSphere(transform.position, data.ExplosionRadius);
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
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.RangeAttack);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.ExplosionRadius);
    }
}





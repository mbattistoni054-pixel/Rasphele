using PatronesAplicados;
using PatronesAplicados.RealImplementation;
using UnityEngine;

public class NEWEnemyRange : EnemyBaseRefactored
{
    [Header("Estadsticas de Ataque")]

    private float lastAttackTime;
    bool onAttack;

    [SerializeField] Animator animator;
    [SerializeField] Transform firePoint;

    private bool hasMoveParam = false;
    private float pathTimer = 0f;

    public override void ResetStats()
    {
        base.ResetStats();
        onAttack = false;
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
            agent.stoppingDistance = data.RangeAttack;
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

            if (!onAttack && distance > data.RangeAttack)
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

            if (distance <= data.RangeAttack || onAttack)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                directionToPlayer.y = 0; 

                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
                }
            }

            if (distance <= data.RangeAttack && Time.time >= lastAttackTime + data.AttackCooldown)
            {
                onAttack = true;
                lastAttackTime = Time.time;
                if (animator != null) animator.SetTrigger("Shoot");

                Invoke(nameof(AttackEnd), 1f);
            }
        }
    }

    public void Shoot()
    {
        if (data.Bullet == null || firePoint == null) return;
        
        GameObject obj;
        if (ProjectilePoolManager.Instance != null)
        {
            obj = ProjectilePoolManager.Instance.GetProjectile(data.Bullet, firePoint.position, firePoint.rotation);
        }
        else
        {
            obj = Instantiate(data.Bullet, firePoint.position, firePoint.rotation);
        }
        
        NEWBulletEnemy newBullet = obj.GetComponent<NEWBulletEnemy>();
        if (newBullet != null)
        {
            newBullet.damage = data.AttackDamage * damageMultiplier;
        }
        else
        {
            BulletEnemy oldBullet = obj.GetComponent<BulletEnemy>();
            if (oldBullet != null) oldBullet.damageMultiplier = damageMultiplier;
            if (oldBullet != null) oldBullet.data = data;
        }
    }

    public void AttackEnd()
    {
        onAttack = false;
    }
}


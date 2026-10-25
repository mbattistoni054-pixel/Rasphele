using UnityEngine;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class NEWEnemyRange : EnemyBaseRefactored
{
    private float _cooldownTimer;
    private float _pathTimer;
    private bool _onAttack;

    [SerializeField] private Animator animator;
    [SerializeField] private Transform firePoint;

    private bool _hasMoveParam;

    public override void ResetStats()
    {
        base.ResetStats();
        CancelInvoke(nameof(AttackEnd));
        _onAttack = false;
        _pathTimer = 0f;
        _cooldownTimer = data != null ? data.AttackCooldown : 0f;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        CancelInvoke(nameof(AttackEnd));
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
                    _hasMoveParam = true;
                    break;
                }
            }
        }

        if (agent != null && data != null)
        {
            agent.stoppingDistance = data.RangeAttack;
            agent.acceleration = 60f;
            agent.angularSpeed = 600f;
            _cooldownTimer = data.AttackCooldown;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (isDead || isPaused || data == null) return;

        float dt = Time.deltaTime;
        _cooldownTimer += dt;
        _pathTimer -= dt;

        if (player != null && !isStunned && agent != null && agent.enabled && agent.isOnNavMesh)
        {
            float distance = Vector3.Distance(player.position, transform.position);

            if (!_onAttack && distance > data.RangeAttack)
            {
                agent.isStopped = false;

                if (_pathTimer <= 0f)
                {
                    agent.SetDestination(player.position);
                    _pathTimer = 0.2f;
                }

                if (animator != null && _hasMoveParam) animator.SetBool("Move", true);
            }
            else
            {
                agent.isStopped = true;

                if (animator != null && _hasMoveParam) animator.SetBool("Move", false);
            }

            if (distance <= data.RangeAttack || _onAttack)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                directionToPlayer.y = 0f;

                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, dt * 15f);
                }
            }

            if (distance <= data.RangeAttack && _cooldownTimer >= data.AttackCooldown)
            {
                _onAttack = true;
                _cooldownTimer = 0f;
                if (animator != null) animator.SetTrigger("Shoot");

                CancelInvoke(nameof(AttackEnd));
                Invoke(nameof(AttackEnd), 1f);
            }
        }
    }

    public void Shoot()
    {
        if (isDead || isPaused || data == null || data.Bullet == null || firePoint == null) return;

        float finalDamage = data.AttackDamage * damageMultiplier;

        if (ProjectilePoolManager.Instance != null)
        {
            NEWBulletEnemy pooledBullet = ProjectilePoolManager.Instance.GetProjectile<NEWBulletEnemy>(
                data.Bullet,
                firePoint.position,
                firePoint.rotation);

            if (pooledBullet != null)
            {
                pooledBullet.Setup(finalDamage, player);
            }
        }
        else
        {
            GameObject obj = Instantiate(data.Bullet, firePoint.position, firePoint.rotation);
            if (obj.TryGetComponent(out NEWBulletEnemy newBullet))
            {
                newBullet.Setup(finalDamage, player);
            }
        }
    }

    public void AttackEnd()
    {
        _onAttack = false;
    }
}
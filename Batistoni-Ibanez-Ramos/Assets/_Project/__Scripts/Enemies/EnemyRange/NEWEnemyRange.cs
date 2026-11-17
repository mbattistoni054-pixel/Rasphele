using UnityEngine;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class NEWEnemyRange : EnemyBaseRefactored
{
    private float _cooldownTimer;
    private float _pathTimer;
    private float _attackLockTimer;
    private bool _onAttack;

    [SerializeField] private Animator animator;
    [SerializeField] private Transform firePoint;

    private bool _hasMoveParam;

    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int ShootHash = Animator.StringToHash("Shoot");

    public override void ResetStats()
    {
        base.ResetStats();
        _onAttack = false;
        _pathTimer = 0f;
        _attackLockTimer = 0f;
        _cooldownTimer = data != null ? data.AttackCooldown : 0f;
        if (animator != null) animator.speed = isPaused ? 0f : 1f;
    }

    protected override void Start()
    {
        base.Start();
        if (animator == null) animator = GetComponent<Animator>();

        if (animator != null)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].nameHash == MoveHash)
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

    public override void OnPause()
    {
        base.OnPause();
        if (animator != null) animator.speed = 0f;
    }

    public override void OnResume()
    {
        base.OnResume();
        if (animator != null) animator.speed = 1f;
    }

    protected override void Update()
    {
        base.Update();
        if (isDead || isPaused || data == null) return;

        float dt = Time.deltaTime;
        _cooldownTimer += dt;
        _pathTimer -= dt;

        if (_onAttack)
        {
            _attackLockTimer -= dt;
            if (_attackLockTimer <= 0f)
            {
                _onAttack = false;
            }
        }

        if (player != null && !isStunned && agent != null && agent.enabled && agent.isOnNavMesh)
        {
            float sqrDistance = (player.position - transform.position).sqrMagnitude;
            float rangeAttackSqr = data.RangeAttack * data.RangeAttack;

            if (!_onAttack && sqrDistance > rangeAttackSqr)
            {
                agent.isStopped = false;

                if (_pathTimer <= 0f)
                {
                    agent.SetDestination(player.position);
                    _pathTimer = 0.2f;
                }

                if (animator != null && _hasMoveParam) animator.SetBool(MoveHash, true);
            }
            else
            {
                agent.isStopped = true;

                if (animator != null && _hasMoveParam) animator.SetBool(MoveHash, false);
            }

            if (sqrDistance <= rangeAttackSqr || _onAttack)
            {
                RotateTowardsPlayer(dt, 15f);
            }

            if (sqrDistance <= rangeAttackSqr && _cooldownTimer >= data.AttackCooldown)
            {
                _onAttack = true;
                _attackLockTimer = 1f;
                _cooldownTimer = 0f;
                if (animator != null) animator.SetTrigger(ShootHash);
            }
        }
    }

    public void Shoot()
    {
        if (isDead || isPaused || isStunned || data == null || data.Bullet == null || firePoint == null) return;

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
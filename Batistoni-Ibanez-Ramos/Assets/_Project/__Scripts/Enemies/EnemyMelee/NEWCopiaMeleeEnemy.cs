using UnityEngine;
using PatronesAplicados;

public class NEWCopiaMeleeEnemy : EnemyBaseRefactored
{
    private const float StopDistance = 1.5f;
    private const float StopDistanceSqr = StopDistance * StopDistance;
    private const float HitRange = 2f;
    private const float HitRangeSqr = HitRange * HitRange;
    private const float AttackHitDelay = 0.3f;

    private float _attackCooldownTimer;
    private float _attackHitTimer;
    private float _pathTimer;
    private bool _onAttack;

    [SerializeField] private Animator animator;
    private bool _hasMoveParam;

    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    public override void ResetStats()
    {
        base.ResetStats();
        _onAttack = false;
        _pathTimer = 0f;
        _attackHitTimer = 0f;
        _attackCooldownTimer = data != null ? data.AttackCooldown : 0f;
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

        if (agent != null)
        {
            agent.stoppingDistance = StopDistance;
            agent.acceleration = 60f;
            agent.angularSpeed = 600f;
        }

        _attackCooldownTimer = data != null ? data.AttackCooldown : 0f;
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
        _attackCooldownTimer += dt;
        _pathTimer -= dt;

        if (player != null && !isStunned && agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            float sqrDistance = (player.position - transform.position).sqrMagnitude;

            if (!_onAttack && sqrDistance > StopDistanceSqr)
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

            if (sqrDistance <= HitRangeSqr || _onAttack)
            {
                RotateTowardsPlayer(dt, 15f);
            }

            if (sqrDistance <= StopDistanceSqr || _onAttack)
            {
                if (!_onAttack && _attackCooldownTimer >= data.AttackCooldown)
                {
                    if (animator != null) animator.SetTrigger(AttackHash);

                    _attackCooldownTimer = 0f;
                    _attackHitTimer = AttackHitDelay;
                    _onAttack = true;
                }

                if (_onAttack)
                {
                    _attackHitTimer -= dt;
                    if (_attackHitTimer <= 0f)
                    {
                        if (sqrDistance <= HitRangeSqr && player.TryGetComponent(out PlayerHealthRefactored newHealth))
                        {
                            newHealth.TakeDamage(data.AttackDamage * damageMultiplier);
                        }

                        _onAttack = false;
                    }
                }
            }
        }
    }
}
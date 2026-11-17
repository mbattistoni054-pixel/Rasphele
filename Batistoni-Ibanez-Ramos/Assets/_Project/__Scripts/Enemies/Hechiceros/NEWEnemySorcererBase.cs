using UnityEngine;
using PatronesAplicados;

public abstract class NEWEnemySorcererBase : EnemyBaseRefactored
{
    [Header("Animación")]
    public Animator animator;
    protected bool hasMoveParam;
    protected bool hasSpeedParam;
    protected bool hasCastParam;
    protected bool hasAttackParam;

    protected float attackCooldownTimer;
    protected bool isChargingAttack;
    protected bool isFleeing;
    protected float pathTimer;

    protected static readonly int MoveHash = Animator.StringToHash("Move");
    protected static readonly int SpeedHash = Animator.StringToHash("Speed");
    protected static readonly int CastHash = Animator.StringToHash("Cast");
    protected static readonly int AttackHash = Animator.StringToHash("Attack");

    public override void ResetStats()
    {
        base.ResetStats();
        isChargingAttack = false;
        isFleeing = false;
        pathTimer = 0f;
        attackCooldownTimer = data != null ? data.AttackCooldown : 0f;
        if (animator != null) animator.speed = isPaused ? 0f : 1f;
    }

    protected override void Start()
    {
        base.Start();
        if (animator == null) animator = GetComponent<Animator>();

        CacheAnimatorParameters();

        if (agent != null)
        {
            agent.acceleration = 40f;
            agent.angularSpeed = 400f;
        }

        attackCooldownTimer = data != null ? data.AttackCooldown : 0f;
    }

    private void CacheAnimatorParameters()
    {
        hasMoveParam = false;
        hasSpeedParam = false;
        hasCastParam = false;
        hasAttackParam = false;

        if (animator == null) return;

        AnimatorControllerParameter[] parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            int hash = parameters[i].nameHash;
            AnimatorControllerParameterType type = parameters[i].type;

            if (hash == MoveHash && type == AnimatorControllerParameterType.Bool)
                hasMoveParam = true;
            else if (hash == SpeedHash && type == AnimatorControllerParameterType.Float)
                hasSpeedParam = true;
            else if (hash == CastHash && type == AnimatorControllerParameterType.Trigger)
                hasCastParam = true;
            else if (hash == AttackHash && type == AnimatorControllerParameterType.Trigger)
                hasAttackParam = true;
        }
    }

    protected void SetMovingAnimation(bool isMoving)
    {
        if (animator == null) return;

        if (hasMoveParam)
        {
            animator.SetBool(MoveHash, isMoving);
        }

        if (hasSpeedParam)
        {
            animator.SetFloat(SpeedHash, isMoving ? currentSpeed : 0f);
        }
    }

    protected void TriggerAttackAnimation()
    {
        if (animator == null) return;

        if (hasCastParam)
        {
            animator.SetTrigger(CastHash);
        }
        else if (hasAttackParam)
        {
            animator.SetTrigger(AttackHash);
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
        if (!isChargingAttack)
        {
            attackCooldownTimer += dt;
        }
        pathTimer -= dt;

        if (player != null && !isStunned && agent != null && agent.enabled && agent.isOnNavMesh)
        {
            float sqrDistance = (player.position - transform.position).sqrMagnitude;
            float fleeDistSqr = data.FleeDistance * data.FleeDistance;
            float rangeAttackSqr = data.RangeAttack * data.RangeAttack;

            if (!isChargingAttack)
            {
                if (sqrDistance < fleeDistSqr)
                {
                    FleeFromPlayer();
                    return;
                }

                if (sqrDistance > rangeAttackSqr)
                {
                    ApproachPlayer();
                    return;
                }

                if (attackCooldownTimer >= data.AttackCooldown)
                {
                    agent.isStopped = true;
                    SetMovingAnimation(false);
                    StartAttackRoutine();
                    return;
                }

                agent.isStopped = true;
                SetMovingAnimation(false);
                LookAtPlayer(dt);
            }
        }
    }

    protected void FleeFromPlayer()
    {
        isFleeing = true;
        agent.isStopped = false;

        if (pathTimer <= 0f)
        {
            Vector3 dirAwayFromPlayer = transform.position - player.position;
            dirAwayFromPlayer.y = 0f;
            if (dirAwayFromPlayer.sqrMagnitude < 0.0001f)
            {
                dirAwayFromPlayer = -transform.forward;
            }
            else
            {
                dirAwayFromPlayer.Normalize();
            }

            Vector3 fleePosition = transform.position + dirAwayFromPlayer * 10f;
            agent.SetDestination(fleePosition);
            pathTimer = 0.2f;
        }

        SetMovingAnimation(true);
    }

    protected void ApproachPlayer()
    {
        isFleeing = false;
        agent.isStopped = false;

        if (pathTimer <= 0f)
        {
            agent.SetDestination(player.position);
            pathTimer = 0.2f;
        }

        SetMovingAnimation(true);
    }

    protected void LookAtPlayer(float deltaTime)
    {
        RotateTowardsPlayer(deltaTime, 10f);
    }

    protected abstract void StartAttackRoutine();

    private void OnDrawGizmosSelected()
    {
        if (data == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.RangeAttack);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, data.FleeDistance);
    }
}
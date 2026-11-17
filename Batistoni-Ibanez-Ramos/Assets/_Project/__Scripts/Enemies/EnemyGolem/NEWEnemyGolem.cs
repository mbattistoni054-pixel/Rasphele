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

    [Header("Control de Empuje y Stun")]
    [Tooltip("Duración del aturdimiento que este Golem aplica al jugador al golpearlo.")]
    public float knockbackStunDuration = 0.4f;

    private bool _isAttacking;
    private bool _hasMoveParam;
    private float _pathTimer;

    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private readonly Collider[] _slamHitsBuffer = new Collider[16];

    public override void ResetStats()
    {
        base.ResetStats();
        _isAttacking = false;
        _pathTimer = 0f;
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
            agent.stoppingDistance = Mathf.Max(0.5f, data.RangeAttack - 0.5f);
            agent.acceleration = 40f;
            agent.angularSpeed = 300f;
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
        _pathTimer -= dt;

        if (player != null && !isStunned && agent != null && agent.enabled && agent.isOnNavMesh)
        {
            Vector3 toPlayer = player.position - transform.position;
            float sqrDistance = toPlayer.sqrMagnitude;
            float rangeAttackSqr = data.RangeAttack * data.RangeAttack;

            if (!_isAttacking)
            {
                if (sqrDistance > rangeAttackSqr)
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

                    StartCoroutine(SlamAttackRoutine());
                }
            }

            float maxRotateRange = data.RangeAttack * 2f;
            if (_isAttacking && sqrDistance <= maxRotateRange * maxRotateRange)
            {
                RotateTowardsPlayer(dt, 5f);
            }
        }
    }

    private IEnumerator SlamAttackRoutine()
    {
        _isAttacking = true;

        if (animator != null) animator.SetTrigger(AttackHash);

        float hitDelayTimer = data.HitDelay;
        while (hitDelayTimer > 0f)
        {
            if (isDead || isStunned)
            {
                _isAttacking = false;
                yield break;
            }

            if (!isPaused && !PauseManager.IsPaused)
            {
                hitDelayTimer -= Time.deltaTime;
            }
            yield return null;
        }

        if (isDead || isStunned)
        {
            _isAttacking = false;
            yield break;
        }

        Vector3 impactCenter = transform.position + (transform.forward * 2.5f);
        GameObject visualPrefabToUse = slamVisualPrefab != null ? slamVisualPrefab : data.SlamVisualPrefab;

        if (visualPrefabToUse != null)
        {
            GameObject visual = VfxPoolManager.Instance != null
                ? VfxPoolManager.Instance.GetVfx(visualPrefabToUse, impactCenter, Quaternion.identity, 1.5f)
                : Instantiate(visualPrefabToUse, impactCenter, Quaternion.identity);

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

                if (hit.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(finalDamage, false, DamageType.Fisico);
                }
                else if (hit.TryGetComponent(out PlayerHealthRefactored refHealth))
                {
                    refHealth.TakeDamage(finalDamage);
                }

                Vector3 pushDir = hit.transform.position - transform.position;
                pushDir.y = 0f;
                pushDir.Normalize();
                Vector3 finalKnockback = (pushDir * data.KnockbackHorizontal) + (Vector3.up * data.KnockbackUpward);

                if (hit.TryGetComponent(out IKnockbackable knockbackable))
                {
                    knockbackable.ApplyKnockback(finalKnockback, knockbackStunDuration);
                }
                else if (hit.TryGetComponent(out PlayerControllerRefactored pcRefactored))
                {
                    pcRefactored.ApplyKnockback(finalKnockback, knockbackStunDuration);
                }
            }
        }

        float cooldownTimer = data.AttackCooldown;
        while (cooldownTimer > 0f)
        {
            if (isDead)
            {
                _isAttacking = false;
                yield break;
            }

            if (!isPaused && !PauseManager.IsPaused)
            {
                cooldownTimer -= Time.deltaTime;
            }
            yield return null;
        }

        _isAttacking = false;
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
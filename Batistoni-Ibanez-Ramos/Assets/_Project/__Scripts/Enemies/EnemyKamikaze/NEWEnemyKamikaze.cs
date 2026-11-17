using UnityEngine;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class NEWEnemyKamikaze : EnemyBaseRefactored
{
    private bool _isTriggered;
    private float _explosionTimer;
    private int _lastBlinkPhase = -1;

    private Renderer _rend;
    [SerializeField] private Animator animator;

    private bool _hasMoveParam;
    private float _pathTimer;

    private MaterialPropertyBlock _propBlock;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int MoveHash = Animator.StringToHash("Move");

    private readonly Collider[] _explosionHitsBuffer = new Collider[16];

    public override void ResetStats()
    {
        base.ResetStats();
        _isTriggered = false;
        _explosionTimer = 0f;
        _lastBlinkPhase = -1;
        _pathTimer = 0f;
        ClearBlinkColor();
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

        _rend = GetComponentInChildren<Renderer>();
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        if (agent != null && data != null)
        {
            agent.stoppingDistance = Mathf.Max(0.5f, data.RangeAttack - 0.5f);
            agent.acceleration = 60f;
            agent.angularSpeed = 600f;
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

        if (_isTriggered)
        {
            TickKamikazeCountdown(dt);
            if (isDead) return;
        }

        if (player != null && !isStunned && agent != null && agent.enabled && agent.isOnNavMesh)
        {
            float sqrDistance = (player.position - transform.position).sqrMagnitude;
            float rangeAttackSqr = data.RangeAttack * data.RangeAttack;

            if (!_isTriggered)
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
                    StartKamikazeCountdown();
                }
            }

            if (sqrDistance <= rangeAttackSqr || _isTriggered)
            {
                RotateTowardsPlayer(dt, 30f);
            }
        }
    }

    private void StartKamikazeCountdown()
    {
        _isTriggered = true;
        _explosionTimer = data.ExplosionDelay;
        _lastBlinkPhase = -1;

        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
        if (animator != null && _hasMoveParam) animator.SetBool(MoveHash, false);
    }

    private void TickKamikazeCountdown(float deltaTime)
    {
        _explosionTimer -= deltaTime;

        if (_explosionTimer <= 0f)
        {
            Explode();
            return;
        }

        int currentPhase = Mathf.FloorToInt(_explosionTimer * 8f) % 2;
        if (currentPhase != _lastBlinkPhase)
        {
            _lastBlinkPhase = currentPhase;
            SetBlinkColor(currentPhase == 0 ? Color.red : Color.yellow);
        }
    }

    private void SetBlinkColor(Color c)
    {
        if (_rend == null) return;
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        _rend.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(BaseColorId, c);
        _propBlock.SetColor(ColorId, c);
        _rend.SetPropertyBlock(_propBlock);
    }

    private void ClearBlinkColor()
    {
        if (_rend == null) return;
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        _propBlock.Clear();
        _rend.SetPropertyBlock(_propBlock);
    }

    private void Explode()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();
        ClearBlinkColor();

        if (enemyCollider != null) enemyCollider.enabled = false;
        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;

        if (data.ExplosionVisualPrefab != null)
        {
            GameObject visual = VfxPoolManager.Instance != null
                ? VfxPoolManager.Instance.GetVfx(data.ExplosionVisualPrefab, transform.position, Quaternion.identity, 1.5f)
                : Instantiate(data.ExplosionVisualPrefab, transform.position, Quaternion.identity);

            if (visual != null)
            {
                float visualScale = data.ExplosionRadius * 2f;
                visual.transform.localScale = new Vector3(visualScale, visualScale, visualScale);
            }
        }

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, data.ExplosionRadius, _explosionHitsBuffer);
        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _explosionHitsBuffer[i];
            if (hit != null && hit.CompareTag("Player"))
            {
                if (hit.TryGetComponent(out PlayerHealthRefactored refHealth))
                {
                    refHealth.TakeDamage(data.AttackDamage * damageMultiplier);
                }
            }
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.TriggerEvent(GameEventKeys.EnemyDied);
        }

        ReturnEnemy();
    }

    private void OnDrawGizmosSelected()
    {
        if (data == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.RangeAttack);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.ExplosionRadius);
    }
}
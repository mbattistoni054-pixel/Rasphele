using System.Collections;
using UnityEngine;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class NEWEnemyKamikaze : EnemyBaseRefactored
{
    private bool isTriggered = false;
    private Renderer rend;
    [SerializeField] private Animator animator;

    private bool hasMoveParam = false;
    private float pathTimer = 0f;

    private MaterialPropertyBlock _propBlock;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private readonly Collider[] _explosionHitsBuffer = new Collider[16];

    public override void ResetStats()
    {
        base.ResetStats();
        isTriggered = false;
        pathTimer = 0f;
        ClearBlinkColor();
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

        rend = GetComponentInChildren<Renderer>();
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        if (agent != null && data != null)
        {
            agent.stoppingDistance = Mathf.Max(0.5f, data.RangeAttack - 0.5f);
            agent.acceleration = 60f;
            agent.angularSpeed = 600f;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (isDead || data == null) return;

        if (player != null && !isStunned && agent != null && agent.enabled && agent.isOnNavMesh)
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
                directionToPlayer.y = 0f;

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

        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
        if (animator != null && hasMoveParam) animator.SetBool("Move", false);

        float timer = data.ExplosionDelay;

        while (timer > 0f)
        {
            if (isDead) yield break;

            Color blinkColor = (Mathf.FloorToInt(timer * 8f) % 2 == 0) ? Color.red : Color.yellow;
            SetBlinkColor(blinkColor);

            yield return new WaitForSeconds(0.125f);
            timer -= 0.125f;
        }

        if (!isDead)
        {
            Explode();
        }
    }

    private void SetBlinkColor(Color c)
    {
        if (rend == null) return;
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        rend.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(BaseColorId, c);
        _propBlock.SetColor(ColorId, c);
        rend.SetPropertyBlock(_propBlock);
    }

    private void ClearBlinkColor()
    {
        if (rend == null) return;
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        _propBlock.Clear();
        rend.SetPropertyBlock(_propBlock);
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
            GameObject visual;
            if (VfxPoolManager.Instance != null)
            {
                visual = VfxPoolManager.Instance.GetVfx(data.ExplosionVisualPrefab, transform.position, Quaternion.identity, 1.5f);
            }
            else
            {
                visual = Instantiate(data.ExplosionVisualPrefab, transform.position, Quaternion.identity);
            }

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
            EventManager.Instance.TriggerEvent("EnemyDied");
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
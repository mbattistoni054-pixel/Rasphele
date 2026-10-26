using System.Collections;
using UnityEngine;
using PatronesAplicados;

[RequireComponent(typeof(LineRenderer))]
public class NEWEnemyMagicSorcerer : NEWEnemySorcererBase
{
    [Tooltip("El punto desde donde sale el láser (ej: la punta del báculo)")]
    public Transform firePoint;

    private LineRenderer _lineRenderer;
    private float _blinkAccumulator;

    private static Material _fallbackBeamMaterial;
    private readonly RaycastHit[] _laserHitsBuffer = new RaycastHit[16];

    protected override void Awake()
    {
        base.Awake();
        _lineRenderer = GetComponent<LineRenderer>();
    }

    protected override void Start()
    {
        base.Start();

        if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.enabled = false;
        _lineRenderer.positionCount = 2;

        if (data != null && data.BeamMaterial != null)
        {
            _lineRenderer.sharedMaterial = data.BeamMaterial;
        }
        else
        {
            if (_fallbackBeamMaterial == null)
            {
                Shader defaultShader = Shader.Find("Sprites/Default");
                if (defaultShader != null) _fallbackBeamMaterial = new Material(defaultShader);
            }
            if (_fallbackBeamMaterial != null)
            {
                _lineRenderer.sharedMaterial = _fallbackBeamMaterial;
            }
        }
    }

    public override void ResetStats()
    {
        base.ResetStats();
        DisableLaser();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        DisableLaser();
    }

    protected override void Die()
    {
        DisableLaser();
        base.Die();
    }

    protected override void OnStunStateChanged(bool stunState)
    {
        base.OnStunStateChanged(stunState);
        if (stunState)
        {
            DisableLaser();
            isChargingAttack = false;
        }
    }

    private void DisableLaser()
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
        }
    }

    protected override void StartAttackRoutine()
    {
        StartCoroutine(PerformAttackRoutine());
    }

    private IEnumerator PerformAttackRoutine()
    {
        isChargingAttack = true;
        attackCooldownTimer = 0f;

        if (animator != null) animator.SetTrigger(CastHash);

        if (firePoint == null || player == null || data == null)
        {
            isChargingAttack = false;
            yield break;
        }

        _lineRenderer.enabled = true;
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;

        float timer = 0f;
        _blinkAccumulator = 0f;
        Vector3 targetPos = player.position + Vector3.up * 1f;
        float trackingDuration = Mathf.Max(0.01f, data.TrackingTime);

        while (timer < trackingDuration)
        {
            if (isDead || isStunned || player == null)
            {
                DisableLaser();
                isChargingAttack = false;
                yield break;
            }

            if (!isPaused && !PauseManager.IsPaused)
            {
                float dt = Time.deltaTime;
                LookAtPlayer(dt);

                targetPos = player.position + Vector3.up * 1f;
                _lineRenderer.SetPosition(0, firePoint.position);
                _lineRenderer.SetPosition(1, targetPos);

                float blinkSpeed = Mathf.Lerp(5f, 25f, timer / trackingDuration);
                _blinkAccumulator += dt * blinkSpeed;
                Color blinkColor = Color.Lerp(Color.white, Color.red, Mathf.PingPong(_blinkAccumulator, 1f));

                _lineRenderer.startColor = blinkColor;
                _lineRenderer.endColor = blinkColor;

                timer += dt;
            }

            yield return null;
        }

        _lineRenderer.startColor = Color.white;
        _lineRenderer.endColor = Color.white;

        float lockedTimer = data.LockedTime;
        while (lockedTimer > 0f)
        {
            if (isDead || isStunned)
            {
                DisableLaser();
                isChargingAttack = false;
                yield break;
            }

            if (!isPaused && !PauseManager.IsPaused)
            {
                lockedTimer -= Time.deltaTime;
            }
            yield return null;
        }

        if (isDead || isStunned || firePoint == null)
        {
            DisableLaser();
            isChargingAttack = false;
            yield break;
        }

        _lineRenderer.startWidth = 0.8f;
        _lineRenderer.endWidth = 0.8f;
        _lineRenderer.startColor = Color.red;
        _lineRenderer.endColor = Color.red;

        Vector3 origin = firePoint.position;
        Vector3 shootDirection = (targetPos - origin).normalized;
        if (shootDirection == Vector3.zero) shootDirection = transform.forward;
        const float shootDistance = 50f;

        _lineRenderer.SetPosition(0, origin);
        _lineRenderer.SetPosition(1, origin + shootDirection * shootDistance);

        int hitCount = Physics.SphereCastNonAlloc(origin, 0.5f, shootDirection, _laserHitsBuffer, shootDistance, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hitCount; i++)
        {
            Collider col = _laserHitsBuffer[i].collider;
            if (col != null && col.CompareTag("Player"))
            {
                if (col.TryGetComponent(out PlayerHealthRefactored refHealth))
                {
                    refHealth.TakeDamage(data.AttackDamage * damageMultiplier);
                }
                break;
            }
        }

        float laserTimer = data.LaserDuration;
        while (laserTimer > 0f)
        {
            if (isDead || isStunned)
            {
                DisableLaser();
                isChargingAttack = false;
                yield break;
            }

            if (!isPaused && !PauseManager.IsPaused)
            {
                laserTimer -= Time.deltaTime;
            }
            yield return null;
        }

        DisableLaser();
        attackCooldownTimer = 0f;
        isChargingAttack = false;
    }
}
using System.Collections;
using UnityEngine;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class NEWEnemyFireLine : MonoBehaviour
{
    [Header("Visuales (Hijos)")]
    public Transform warningVisual;
    public Transform fireVisual;

    [Header("Ajustes de Duración y Dimensiones")]
    [Tooltip("Tiempo que dura la advertencia antes de encender el fuego.")]
    public float warningDuration = 1f;
    [Tooltip("Duración total del fuego activo en el suelo.")]
    public float activeDuration = 10f;
    [Tooltip("Cada cuántos segundos aplica daño mientras el jugador esté encima.")]
    public float damageTickInterval = 1f;
    [Tooltip("Ancho del rectángulo de fuego.")]
    public float width = 3f;

    private float _damage;
    private float _length = 50f;
    private float _currentWarningDuration;
    private float _currentActiveDuration;
    private bool _isReturned;

    private static readonly Collider[] _hitBuffer = new Collider[16];

    private void OnEnable()
    {
        _isReturned = false;
    }

    public void Setup(float dmg, float lineLength, float customActiveDuration = -1f, float customWarningDuration = -1f)
    {
        _damage = dmg;
        _length = lineLength > 0f ? lineLength : 50f;
        _currentActiveDuration = customActiveDuration > 0f ? customActiveDuration : activeDuration;
        _currentWarningDuration = customWarningDuration > 0f ? customWarningDuration : warningDuration;
        _isReturned = false;

        if (warningVisual != null)
        {
            warningVisual.localScale = new Vector3(width, 0.1f, _length);
            warningVisual.localPosition = new Vector3(0f, 0f, _length / 2f);
        }

        if (fireVisual != null)
        {
            fireVisual.localScale = new Vector3(width, 0.5f, _length);
            fireVisual.localPosition = new Vector3(0f, 0f, _length / 2f);
        }

        StopAllCoroutines();
        StartCoroutine(LineRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator LineRoutine()
    {
        if (warningVisual != null) warningVisual.gameObject.SetActive(true);
        if (fireVisual != null) fireVisual.gameObject.SetActive(false);

        float warningTimer = _currentWarningDuration;
        while (warningTimer > 0f)
        {
            if (!PauseManager.IsPaused) warningTimer -= Time.deltaTime;
            yield return null;
        }

        if (warningVisual != null) warningVisual.gameObject.SetActive(false);
        if (fireVisual != null) fireVisual.gameObject.SetActive(true);

        float activeTimer = _currentActiveDuration;
        float damageTickTimer = 0f;
        float tickInterval = Mathf.Max(0.1f, damageTickInterval);

        while (activeTimer > 0f)
        {
            if (!PauseManager.IsPaused)
            {
                damageTickTimer -= Time.deltaTime;

                if (damageTickTimer <= 0f)
                {
                    damageTickTimer = tickInterval;
                    DealDamage();
                }

                activeTimer -= Time.deltaTime;
            }
            yield return null;
        }

        ReturnToPool();
    }

    private void DealDamage()
    {
        Vector3 boxCenter = transform.position + transform.forward * (_length / 2f);
        Vector3 halfExtents = new Vector3(width / 2f, 2f, _length / 2f);

        int hitCount = Physics.OverlapBoxNonAlloc(boxCenter, halfExtents, _hitBuffer, transform.rotation);
        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _hitBuffer[i];
            if (hit != null && hit.CompareTag("Player"))
            {
                if (hit.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(_damage, false, DamageType.Fuego);
                    break;
                }
                if (hit.TryGetComponent(out PlayerHealthRefactored refHealth))
                {
                    refHealth.TakeDamage(_damage);
                    break;
                }
            }
        }
    }

    private void ReturnToPool()
    {
        if (_isReturned) return;
        _isReturned = true;

        if (ProjectilePoolManager.Instance != null)
        {
            ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawCube(new Vector3(0f, 0f, _length / 2f), new Vector3(width, 2f, _length));
    }
}
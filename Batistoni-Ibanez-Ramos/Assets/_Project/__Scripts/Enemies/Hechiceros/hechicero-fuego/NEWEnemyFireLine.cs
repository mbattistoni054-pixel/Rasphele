using System.Collections;
using UnityEngine;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class NEWEnemyFireLine : MonoBehaviour
{
    [Header("Visuales (Hijos)")]
    public Transform warningVisual;
    public Transform fireVisual;

    private float _damage;
    private float _length;
    private const float Width = 3f;
    private bool _isReturned;

    private static readonly Collider[] _hitBuffer = new Collider[16];

    private void OnEnable()
    {
        _isReturned = false;
    }

    public void Setup(float dmg, float lineLength)
    {
        _damage = dmg;
        _length = lineLength;
        _isReturned = false;

        if (warningVisual != null)
        {
            warningVisual.localScale = new Vector3(Width, 0.1f, _length);
            warningVisual.localPosition = new Vector3(0f, 0f, _length / 2f);
        }

        if (fireVisual != null)
        {
            fireVisual.localScale = new Vector3(Width, 0.5f, _length);
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

        float warningTimer = 1f;
        while (warningTimer > 0f)
        {
            if (!PauseManager.IsPaused) warningTimer -= Time.deltaTime;
            yield return null;
        }

        if (warningVisual != null) warningVisual.gameObject.SetActive(false);
        if (fireVisual != null) fireVisual.gameObject.SetActive(true);

        float activeTimer = 10f;
        float damageTickTimer = 0f;

        while (activeTimer > 0f)
        {
            if (!PauseManager.IsPaused)
            {
                damageTickTimer -= Time.deltaTime;

                if (damageTickTimer <= 0f)
                {
                    damageTickTimer = 1f;
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
        Vector3 halfExtents = new Vector3(Width / 2f, 2f, _length / 2f);

        int hitCount = Physics.OverlapBoxNonAlloc(boxCenter, halfExtents, _hitBuffer, transform.rotation);
        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _hitBuffer[i];
            if (hit != null && hit.CompareTag("Player"))
            {
                if (hit.TryGetComponent(out PlayerHealthRefactored refHealth))
                {
                    refHealth.TakeDamage(_damage);
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
        Gizmos.DrawCube(new Vector3(0f, 0f, _length / 2f), new Vector3(Width, 2f, _length));
    }
}
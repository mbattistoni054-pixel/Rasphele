using UnityEngine;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class NEWBossOrb : MonoBehaviour, IPausable
{
    public float speed = 18f;
    public float lifeTime = 4f;

    [Header("Colisiones")]
    [Tooltip("Capas con las que el orbe choca y explota (ej: Default, Wall)")]
    public LayerMask obstacleMask;

    [Header("Efectos")]
    public GameObject impactVisualPrefab;
    public float impactVisualDuration = 0.5f;

    private float _damage;
    private float _timer;
    private bool _hasExploded;
    private bool _isPaused;

    public void Setup(float dmg)
    {
        _damage = dmg;
        _timer = 0f;
        _hasExploded = false;
    }

    private void OnEnable()
    {
        _timer = 0f;
        _hasExploded = false;
        _isPaused = PauseManager.IsPaused;
        PauseManager.Register(this);
    }

    private void OnDisable()
    {
        PauseManager.Unregister(this);
    }

    private void Update()
    {
        if (_hasExploded || _isPaused) return;

        transform.position += transform.forward * (speed * Time.deltaTime);

        _timer += Time.deltaTime;
        if (_timer >= lifeTime)
        {
            Explode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasExploded || _isPaused || PauseManager.IsPaused) return;

        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(_damage);
            }
            else if (other.TryGetComponent(out PlayerHealthRefactored refHealth))
            {
                refHealth.TakeDamage(_damage);
            }
            Explode();
        }
        else if (((1 << other.gameObject.layer) & obstacleMask) != 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (_hasExploded) return;
        _hasExploded = true;

        if (impactVisualPrefab != null)
        {
            if (VfxPoolManager.Instance != null)
            {
                VfxPoolManager.Instance.GetVfx(impactVisualPrefab, transform.position, Quaternion.identity, impactVisualDuration);
            }
            else
            {
                GameObject vfx = Instantiate(impactVisualPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, impactVisualDuration);
            }
        }

        if (ProjectilePoolManager.Instance != null)
        {
            ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnPause() => _isPaused = true;
    public void OnResume() => _isPaused = false;
}
using UnityEngine;
using PatronesAplicados.RealImplementation;

public class PooledExplosionVisual : MonoBehaviour
{
    [Tooltip("Tiempo en segundos antes de devolver el efecto visual al Pool")]
    public float lifetime = 1f;
    private float _timer = 0f;

    protected virtual void OnEnable()
    {
        _timer = 0f;
    }

    protected virtual void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= lifetime)
        {
            ReturnToPool();
        }
    }

    protected void ReturnToPool()
    {
        if (VfxPoolManager.Instance != null)
        {
            VfxPoolManager.Instance.ReturnVfx(gameObject);
        }
        else if (ProjectilePoolManager.Instance != null)
        {
            ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
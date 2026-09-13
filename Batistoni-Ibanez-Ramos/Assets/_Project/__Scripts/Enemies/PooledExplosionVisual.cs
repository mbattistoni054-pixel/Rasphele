using PatronesAplicados.RealImplementation;
using UnityEngine;

public class PooledExplosionVisual : MonoBehaviour
{
    [Tooltip("Tiempo antes de devolver el visual al pool")]
    public float lifetime = 1f;
    private float timer = 0f;

    private void OnEnable()
    {
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            if (ProjectilePoolManager.Instance != null)
                ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
            else
                Destroy(gameObject);
        }
    }
}

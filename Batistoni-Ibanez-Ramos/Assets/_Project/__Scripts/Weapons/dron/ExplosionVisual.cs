using UnityEngine;

public class ExplosionVisual : MonoBehaviour
{
    [Tooltip("Cunto tiempo dura la esfera visible en pantalla antes de desaparecer")]
    public float lifetime = 1f;

    void OnEnable()
    {
        StartCoroutine(ReturnToPoolRoutine());
    }

    private System.Collections.IEnumerator ReturnToPoolRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        if (PatronesAplicados.RealImplementation.ProjectilePoolManager.Instance != null)
        {
            PatronesAplicados.RealImplementation.ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

using System.Collections;
using UnityEngine;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class NEWBossOrb : MonoBehaviour
{
    public float speed = 18f;
    public float lifeTime = 4f;

    [Header("Colisiones")]
    [Tooltip("Capas con las que el orbe choca y explota (ej: Default, Wall)")]
    public LayerMask obstacleMask;

    [Header("Efectos")]
    public GameObject impactVisualPrefab;
    public float impactVisualDuration = 0.5f;

    private float damage;
    private float timer;
    private bool hasExploded;

    public void Setup(float dmg)
    {
        damage = dmg;
        timer = 0f;
        hasExploded = false;
    }

    private void OnEnable()
    {
        timer = 0f;
        hasExploded = false;
    }

    private void Update()
    {
        if (hasExploded) return;

        transform.position += transform.forward * (speed * Time.deltaTime);

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Explode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        if (other.CompareTag("Player"))
        {
            ApplyDamageToPlayer(other.gameObject, damage);
            Explode();
        }
        else if (((1 << other.gameObject.layer) & obstacleMask) != 0)
        {
            Explode();
        }
    }

    private static void ApplyDamageToPlayer(GameObject playerObj, float dmg)
    {
        if (playerObj.TryGetComponent(out PlayerHealthRefactored refHealth))
        {
            refHealth.TakeDamage(dmg);
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        if (impactVisualPrefab != null)
        {
            if (ProjectilePoolManager.Instance != null)
            {
                GameObject vfx = ProjectilePoolManager.Instance.GetProjectile(impactVisualPrefab, transform.position, Quaternion.identity);
                if (vfx != null && vfx.GetComponent<ExplosionVisual>() == null)
                {
                    ProjectilePoolManager.Instance.StartCoroutine(ReturnVfxAfterDelay(vfx, impactVisualDuration));
                }
            }
            else
            {
                Instantiate(impactVisualPrefab, transform.position, Quaternion.identity);
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

    private static IEnumerator ReturnVfxAfterDelay(GameObject vfxObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (vfxObj != null && vfxObj.activeInHierarchy && ProjectilePoolManager.Instance != null)
        {
            ProjectilePoolManager.Instance.ReturnProjectile(vfxObj);
        }
    }
}
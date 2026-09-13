using PatronesAplicados.RealImplementation;
using UnityEngine;

public class NEWBossOrb : MonoBehaviour
{
    public float speed = 18f;
    public float lifeTime = 4f;

    [Header("Colisiones")]
    [Tooltip("Capas con las que el orbe choca y explota (ej: Default, Wall)")]
    public LayerMask obstacleMask;

    [Header("Efectos")]
    public GameObject impactVisualPrefab;

    private float damage;
    private float timer;

    public void Setup(float dmg)
    {
        damage = dmg;
    }

    private void OnEnable()
    {
        timer = 0f;
    }

    void Update()
    {
        // Avanza en la direccin hacia la que fue rotado
        transform.position += transform.forward * speed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Explode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Choca contra el jugador
        if (other.CompareTag("Player"))
        {
            var refHealth = other.GetComponent<PatronesAplicados.PlayerHealthRefactored>();
                if (refHealth != null) refHealth.TakeDamage(damage);
                else {
                    PlayerHealth hp = other.GetComponent<PlayerHealth>();
                    if (hp != null) hp.TakeDamage(damage);
                }

            Explode();
        }
        // NUEVO: Choca contra obstculos o el suelo usando la Mscara de Capas
        else if (((1 << other.gameObject.layer) & obstacleMask) != 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (impactVisualPrefab != null)
        {
            if (ProjectilePoolManager.Instance != null)
            {
                ProjectilePoolManager.Instance.GetProjectile(impactVisualPrefab, transform.position, Quaternion.identity);
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
}



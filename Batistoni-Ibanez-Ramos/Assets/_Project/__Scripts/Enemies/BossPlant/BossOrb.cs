using UnityEngine;

public class BossOrb : MonoBehaviour
{
    public float speed = 18f;
    public float lifeTime = 4f;

    [Header("Colisiones")]
    [Tooltip("Capas con las que el orbe choca y explota (ej: Default, Wall)")]
    public LayerMask obstacleMask;

    [Header("Efectos")]
    public GameObject impactVisualPrefab;

    private float damage;

    public void Setup(float dmg)
    {
        damage = dmg;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Avanza en la dirección hacia la que fue rotado
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Choca contra el jugador
        if (other.CompareTag("Player"))
        {
            PlayerHealth hp = other.GetComponent<PlayerHealth>();
            if (hp != null) hp.TakeDamage(damage);

            Explode();
        }
        // NUEVO: Choca contra obstáculos o el suelo usando la Máscara de Capas
        else if (((1 << other.gameObject.layer) & obstacleMask) != 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (impactVisualPrefab != null)
        {
            Instantiate(impactVisualPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
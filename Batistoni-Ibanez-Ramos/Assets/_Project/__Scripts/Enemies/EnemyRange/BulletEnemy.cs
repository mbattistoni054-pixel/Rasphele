using UnityEngine;

public class BulletEnemy : MonoBehaviour
{
    Transform player;
    public float offset;
    public float damageMultiplier = 1;
    public EnemyData data;

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        // Solo calculamos la dirección si el jugador realmente existe
        if (p != null)
        {
            player = p.transform;
            Vector3 direction = (new Vector3(player.position.x, player.position.y + offset, player.position.z) - transform.position).normalized;
            transform.up = direction;
        }

        Destroy(gameObject, 4);
    }

    private void Update()
    {
        // Se sigue moviendo hacia la dirección que calculó al nacer
        transform.position += transform.up * data.BulletSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerHealth pHealth = other.gameObject.GetComponent<PlayerHealth>();

            if (pHealth != null)
            {
                pHealth.TakeDamage(data.AttackDamage * damageMultiplier);
                Destroy(gameObject);
            }
        }
    }
}
using PatronesAplicados.RealImplementation;
using UnityEngine;

public class NEWBulletEnemy : MonoBehaviour
{
    Transform player;
    public float damage;
    public float offset;
    [SerializeField] int speed = 80;
    
    private float lifeTimer;
    private float maxLifeTime = 4f;

    private void OnEnable()
    {
        lifeTimer = 0f;
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
        {
            player = p.transform;
            Vector3 direction = (new Vector3(player.position.x, player.position.y + offset, player.position.z) - transform.position).normalized;
            transform.up = direction;
        }
    }

    private void Update()
    {
        transform.position += transform.up * speed * Time.deltaTime;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifeTime)
        {
            if (ProjectilePoolManager.Instance != null)
                ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
            else
                Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var pHealthRef = other.gameObject.GetComponent<PatronesAplicados.PlayerHealthRefactored>();
            if (pHealthRef != null)
            {
                pHealthRef.TakeDamage(damage);
            }
            else
            {
                PlayerHealth pHealth = other.gameObject.GetComponent<PlayerHealth>();
                if (pHealth != null)
                {
                    pHealth.TakeDamage(damage);
                }
            }
            
            if (ProjectilePoolManager.Instance != null)
                ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
            else
                Destroy(gameObject);
        }
    }
}


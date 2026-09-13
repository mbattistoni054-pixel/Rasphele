using UnityEngine;
using PatronesAplicados.RealImplementation;

public class NEWExperienceOrb : MonoBehaviour
{
    [Header("Ajustes de la Gema")]
    public float xpAmount = 20f;
    public float magnetRadius = 5f;   // Distancia a la que empieza a ser atrada
    public float baseMoveSpeed = 10f; // Velocidad base
    private float currentMoveSpeed;

    private Transform player;
    private bool isAttracted = false;

    private void OnEnable()
    {
        isAttracted = false;
        currentMoveSpeed = baseMoveSpeed;
        
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        // Si el jugador est cerca, la gema empieza a ser atrada
        if (!isAttracted && Vector3.Distance(transform.position, player.position) <= magnetRadius)
        {
            isAttracted = true;
        }

        // Si est siendo atrada, vuela hacia el jugador
        if (isAttracted)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, currentMoveSpeed * Time.deltaTime);
        }
    }

    public void ForceAttract(Transform targetPlayer)
    {
        player = targetPlayer;
        isAttracted = true;
        currentMoveSpeed = baseMoveSpeed * 2f; // Que vuelen ms rpido cuando se usa el imn
    }

    // Cuando la gema choca fsicamente con el jugador
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PatronesAplicados.PlayerExperienceRefactored newXp = other.GetComponent<PatronesAplicados.PlayerExperienceRefactored>();
            if (newXp != null)
            {
                newXp.AddExperience(xpAmount);
                ReturnToPool();
            }
            else
            {
                PlayerExperience xpScript = other.GetComponent<PlayerExperience>();
                if (xpScript != null)
                {
                    xpScript.AddExperience(xpAmount);
                    ReturnToPool();
                }
            }
        }
    }

    private void ReturnToPool()
    {
        if (ProjectilePoolManager.Instance != null)
        {
            ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject); // Respaldo por si se rompe el pool
        }
    }
}

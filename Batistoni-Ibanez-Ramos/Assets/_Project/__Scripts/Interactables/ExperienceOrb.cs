using UnityEngine;

public class ExperienceOrb : MonoBehaviour
{
    [Header("Ajustes de la Gema")]
    public float xpAmount = 20f;
    public float magnetRadius = 5f;   // Distancia a la que empieza a ser atraída
    public float moveSpeed = 10f;     // Velocidad a la que vuela hacia el jugador

    private Transform player;
    private bool isAttracted = false;

    void Start()
    {
        // Buscamos al jugador al nacer
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        // Si el jugador está cerca, la gema empieza a ser atraída
        if (!isAttracted && Vector3.Distance(transform.position, player.position) <= magnetRadius)
        {
            isAttracted = true;
        }

        // Si está siendo atraída, vuela hacia el jugador
        if (isAttracted)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    public void ForceAttract(Transform targetPlayer)
    {
        player = targetPlayer;
        isAttracted = true;
        moveSpeed *= 2f; // Que vuelen más rápido cuando se usa el imán
    }

    // Cuando la gema choca físicamente con el jugador
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerExperience xpScript = other.GetComponent<PlayerExperience>();
            if (xpScript != null)
            {
                xpScript.AddExperience(xpAmount);
                Destroy(gameObject); // La gema desaparece
            }
        }
    }
}
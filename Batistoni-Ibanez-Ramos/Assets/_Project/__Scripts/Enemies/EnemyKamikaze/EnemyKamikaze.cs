using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyKamikaze : EnemyBase
{
    [Header("Estadísticas Kamikaze")]
    public float explosionDamage = 40f;
    public float explosionRadius = 4f;
    public float triggerDistance = 2.5f;
    public float explosionDelay = 1.5f;
    public float rotationForce = 30f;

    [Header("Efectos")]
    public GameObject explosionVisualPrefab;

    private bool isTriggered = false;
    private Renderer rend;
    [SerializeField] Animator animator;

    private bool hasMoveParam = false;
    private float pathTimer = 0f; // Reloj para no saturar el NavMesh 

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            foreach (var param in animator.parameters)
            {
                if (param.name == "Move") hasMoveParam = true;
            }
        }

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        rend = GetComponentInChildren<Renderer>();

        if (agent != null)
        {
            agent.stoppingDistance = triggerDistance - 0.5f;
            agent.acceleration = 60f;      // Frena en seco para no patinar hacia ti
            agent.angularSpeed = 600f;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;

        }
        // LÓGICA DE MOVIMIENTO 
        // Solo se mueve si la bomba aún no se ha activado
        if (player != null && !isStunned && agent != null)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            if (!isTriggered)
            {
                if (distance > triggerDistance)
                {
                    agent.isStopped = false;

                    // Solo pedimos ruta nueva cada 0.2 segundos 
                    if (Time.time >= pathTimer)
                    {
                        agent.SetDestination(player.position);
                        pathTimer = Time.time + 0.2f;
                    }

                    if (animator != null && hasMoveParam) animator.SetBool("Move", true);
                }
                else
                {
                    if (animator != null && hasMoveParam) animator.SetBool("Move", false);
                    // Llegó al rango, se activa la bomba
                    StartCoroutine(KamikazeRoutine());

                }
            }

            // Sigue mirándote fijamente incluso si ya se detuvo a explotar
            if (distance <= triggerDistance || isTriggered)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                directionToPlayer.y = 0;

                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationForce);
                }
            }

        }



    }

    private IEnumerator KamikazeRoutine()
    {
        isTriggered = true;

        // Detenemos al agente para que explote en el lugar
        if (agent != null) agent.isStopped = true;
        if (animator != null && hasMoveParam) animator.SetBool("Move", false);

        float timer = explosionDelay;

        while (timer > 0)
        {
            if (rend != null)
            {
                rend.material.color = (Mathf.FloorToInt(timer * 8) % 2 == 0) ? Color.red : Color.yellow;
            }
            yield return new WaitForSeconds(0.125f);
            timer -= 0.125f;
        }

        Explode();
    }

    private void Explode()
    {
        if (explosionVisualPrefab != null)
        {
            GameObject visual = Instantiate(explosionVisualPrefab, transform.position, Quaternion.identity);
            float visualScale = explosionRadius * 2f;
            visual.transform.localScale = new Vector3(visualScale, visualScale, visualScale);
        }

        Collider[] hitObjects = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hitObjects)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth pHealth = hit.GetComponent<PlayerHealth>();
                if (pHealth != null) pHealth.TakeDamage(explosionDamage * damageMultiplier);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
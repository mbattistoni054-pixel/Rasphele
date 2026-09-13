using PatronesAplicados;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class NEWEnemyMagicSorcerer : NEWEnemySorcererBase
{
    [Header("Ataque Lser")]
    public float attackDamage = 60f;

    [Tooltip("El punto desde donde sale el lser (ej: la punta del bculo)")]
    public Transform firePoint;

    [Tooltip("Asigna aqu un Material (ej. Sprites/Default) para evitar que el rayo desaparezca en la Build")]
    public Material beamMaterial;

    [Header("Tiempos del Lser")]
    public float trackingTime = 1.5f; // Tiempo persiguiendo al jugador (parpadeo)
    public float lockedTime = 0.5f;   // Tiempo congelado antes de disparar (blanco)
    public float laserDuration = 0.4f;// Cunto dura el rayo rojo visible

    private LineRenderer lineRenderer;

    protected override void Start()
    {
        base.Start();

        maxHealth = 30f;
        goldReward = 10;

        if (attackCooldown < 5f) attackCooldown = 5f;
        if (attackRange < 40f) attackRange = 40f;
        if (fleeDistance < 20f) fleeDistance = 20f;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 2;

        if (beamMaterial != null)
        {
            lineRenderer.material = beamMaterial;
        }
        else
        {
            Debug.LogWarning("Aviso! El Hechicero Mgico no tiene un Beam Material asignado");
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
    }
    
    protected override void StartAttackRoutine()
    {
        StartCoroutine(PerformAttackRoutine());
    }

    private IEnumerator PerformAttackRoutine()
    {
        isChargingAttack = true;

        if (animator != null) animator.SetTrigger("Cast");

        if (firePoint == null || player == null)
        {
            isChargingAttack = false;
            yield break;
        }

        lineRenderer.enabled = true;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        float timer = 0f;
        Vector3 targetPos = player.position;

        // TRACKING Y PARPADEO 
        while (timer < trackingTime)
        {
            if (player != null)
            {
                targetPos = player.position + Vector3.up * 1f;
            }

            lineRenderer.SetPosition(0, firePoint.position);
            lineRenderer.SetPosition(1, targetPos);

            float blinkSpeed = Mathf.Lerp(5f, 25f, timer / trackingTime);
            Color blinkColor = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * blinkSpeed, 1f));

            lineRenderer.startColor = blinkColor;
            lineRenderer.endColor = blinkColor;

            timer += Time.deltaTime;
            yield return null;
        }

        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        yield return new WaitForSeconds(lockedTime);

        lineRenderer.startWidth = 0.8f;
        lineRenderer.endWidth = 0.8f;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        Vector3 shootDirection = (targetPos - firePoint.position).normalized;
        float shootDistance = 50f;

        lineRenderer.SetPosition(1, firePoint.position + shootDirection * shootDistance);

        if (Physics.SphereCast(firePoint.position, 0.5f, shootDirection, out RaycastHit hit, shootDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                var refHealth = hit.collider.GetComponent<PatronesAplicados.PlayerHealthRefactored>();
                if (refHealth != null) refHealth.TakeDamage(attackDamage * damageMultiplier);
                else {
                    PlayerHealth hp = hit.collider.GetComponent<PlayerHealth>();
                    if (hp != null) hp.TakeDamage(attackDamage * damageMultiplier);
                }
            }
        }

        yield return new WaitForSeconds(laserDuration);

        lineRenderer.enabled = false;
        lastAttackTime = Time.time;
        isChargingAttack = false;
    }
}




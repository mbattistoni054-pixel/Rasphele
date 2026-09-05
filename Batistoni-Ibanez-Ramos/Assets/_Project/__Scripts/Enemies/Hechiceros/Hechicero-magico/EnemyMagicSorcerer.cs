using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class EnemyMagicSorcerer : EnemySorcererBase
{
    [Header("Ataque Láser")]
    public float attackDamage = 60f;

    [Tooltip("El punto desde donde sale el láser (ej: la punta del báculo)")]
    public Transform firePoint;

    [Tooltip("Asigna aquí un Material (ej. Sprites/Default) para evitar que el rayo desaparezca en la Build")]
    public Material beamMaterial;

    [Header("Tiempos del Láser")]
    public float trackingTime = 1.5f; // Tiempo persiguiendo al jugador (parpadeo)
    public float lockedTime = 0.5f;   // Tiempo congelado antes de disparar (blanco)
    public float laserDuration = 0.4f;// Cuánto dura el rayo rojo visible

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
            // Fallback por si se te olvida asignarlo
            Debug.LogWarning("¡Aviso! El Hechicero Mágico no tiene un Beam Material asignado. Puede fallar en la Build.");
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    protected override IEnumerator PerformAttackRoutine()
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

            // Nota: El material asignado debe soportar Color (como Sprites/Default) para que el parpadeo funcione
            lineRenderer.startColor = blinkColor;
            lineRenderer.endColor = blinkColor;

            timer += Time.deltaTime;
            yield return null;
        }

        // BLOQUEO (0.5s) 
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        yield return new WaitForSeconds(lockedTime);

        // DISPARO Y DAÑO 
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
                PlayerHealth hp = hit.collider.GetComponent<PlayerHealth>();
                if (hp != null) hp.TakeDamage(attackDamage * damageMultiplier);
            }
        }

        yield return new WaitForSeconds(laserDuration);

        lineRenderer.enabled = false;
        lastAttackTime = Time.time;
        isChargingAttack = false;
    }
}
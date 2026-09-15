using PatronesAplicados;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class NEWEnemyMagicSorcerer : NEWEnemySorcererBase
{

    [Tooltip("El punto desde donde sale el lser (ej: la punta del bculo)")]
    public Transform firePoint;

    private LineRenderer lineRenderer;

    protected override void Start()
    {
        base.Start();

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 2;

        if (data.BeamMaterial != null)
        {
            lineRenderer.material = data.BeamMaterial;
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

        while (timer < data.TrackingTime)
        {
            if (player != null)
            {
                targetPos = player.position + Vector3.up * 1f;
            }

            lineRenderer.SetPosition(0, firePoint.position);
            lineRenderer.SetPosition(1, targetPos);

            float blinkSpeed = Mathf.Lerp(5f, 25f, timer / data.TrackingTime);
            Color blinkColor = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * blinkSpeed, 1f));

            lineRenderer.startColor = blinkColor;
            lineRenderer.endColor = blinkColor;

            timer += Time.deltaTime;
            yield return null;
        }

        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        yield return new WaitForSeconds(data.LockedTime);

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
                if (refHealth != null) refHealth.TakeDamage(data.AttackDamage * damageMultiplier);
                else {
                    PlayerHealth hp = hit.collider.GetComponent<PlayerHealth>();
                    if (hp != null) hp.TakeDamage(data.AttackDamage * damageMultiplier);
                }
            }
        }

        yield return new WaitForSeconds(data.LaserDuration);

        lineRenderer.enabled = false;
        lastAttackTime = Time.time;
        isChargingAttack = false;
    }
}




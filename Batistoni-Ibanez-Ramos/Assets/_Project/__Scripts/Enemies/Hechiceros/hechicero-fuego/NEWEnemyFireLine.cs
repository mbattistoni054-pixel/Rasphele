using PatronesAplicados.RealImplementation;
using UnityEngine;
using System.Collections;

public class NEWEnemyFireLine : MonoBehaviour
{
    [Header("Visuales (Hijos)")]
    public Transform warningVisual;
    public Transform fireVisual;

    private float damage;
    private float length;
    private float width = 3f;

    public void Setup(float dmg, float lineLength)
    {
        damage = dmg;
        length = lineLength;

        if (warningVisual != null) warningVisual.localScale = new Vector3(width, 0.1f, length);
        if (fireVisual != null) fireVisual.localScale = new Vector3(width, 0.5f, length);

        if (warningVisual != null) warningVisual.localPosition = new Vector3(0, 0, length / 2f);
        if (fireVisual != null) fireVisual.localPosition = new Vector3(0, 0, length / 2f);

        StartCoroutine(LineRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator LineRoutine()
    {
        if (warningVisual != null) warningVisual.gameObject.SetActive(true);
        if (fireVisual != null) fireVisual.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        if (warningVisual != null) warningVisual.gameObject.SetActive(false);
        if (fireVisual != null) fireVisual.gameObject.SetActive(true);

        float activeTimer = 10f;
        float damageTickTimer = 0f;

        while (activeTimer > 0)
        {
            damageTickTimer -= Time.deltaTime;

            if (damageTickTimer <= 0)
            {
                damageTickTimer = 1f; 
                DealDamage();
            }

            activeTimer -= Time.deltaTime;
            yield return null;
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

    private void DealDamage()
    {
        Vector3 boxCenter = transform.position + transform.forward * (length / 2f);
        Vector3 halfExtents = new Vector3(width / 2f, 2f, length / 2f);

        Collider[] hits = Physics.OverlapBox(boxCenter, halfExtents, transform.rotation);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var refHealth = hit.GetComponent<PatronesAplicados.PlayerHealthRefactored>();
                if (refHealth != null) refHealth.TakeDamage(damage);
                else {
                    PlayerHealth hp = hit.GetComponent<PlayerHealth>();
                    if (hp != null) hp.TakeDamage(damage);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawCube(new Vector3(0, 0, length / 2f), new Vector3(width, 2f, length));
    }
}



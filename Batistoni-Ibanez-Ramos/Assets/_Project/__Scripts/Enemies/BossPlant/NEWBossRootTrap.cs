using PatronesAplicados.RealImplementation;
using UnityEngine;
using System.Collections;

public class NEWBossRootTrap : MonoBehaviour
{
    [Header("Tiempos del Ataque")]
    public float followTime = 2f;        
    public float lockWarningTime = 0.5f; 
    public float rootDuration = 2f;     

    [Header("rea y Efectos")]
    public float trapRadius = 4f;
    public Transform warningVisual;      
    public Transform rootVisual;         

    private Transform playerTarget;
    private float damage;
    private CopiaPlayerController2 trappedPlayerCtrl; 

    public void Setup(Transform target, float dmg)
    {
        playerTarget = target;
        damage = dmg;

        if (warningVisual != null) warningVisual.localScale = new Vector3(trapRadius * 2f, 0.1f, trapRadius * 2f);

        if (rootVisual != null) rootVisual.gameObject.SetActive(false);

        StartCoroutine(TrapRoutine());
    }
    
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator TrapRoutine()
    {

        float timer = 0f;
        while (timer < followTime)
        {
            if (playerTarget != null)
            {
                Vector3 targetPos = new Vector3(playerTarget.position.x, playerTarget.position.y + 0.1f, playerTarget.position.z);
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (warningVisual != null)
        {
            if (warningVisual.gameObject.scene.IsValid())
            {
                Renderer warnRend = warningVisual.GetComponent<Renderer>();
                if (warnRend != null) warnRend.material.color = Color.red;
            }
        }

        yield return new WaitForSeconds(lockWarningTime);


        if (warningVisual != null) warningVisual.gameObject.SetActive(false);
        if (rootVisual != null) rootVisual.gameObject.SetActive(true);


        Collider[] hits = Physics.OverlapSphere(transform.position, trapRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("Player909");
                var refHealth = hit.GetComponent<PatronesAplicados.PlayerHealthRefactored>();
                if (refHealth != null) refHealth.TakeDamage(damage);
                else {
                    PlayerHealth hp = hit.GetComponent<PlayerHealth>();
                    if (hp != null) hp.TakeDamage(damage);
                }

                CopiaPlayerController2 playerV2 = hit.GetComponent<CopiaPlayerController2>();
                if (playerV2 != null)
                {
                    playerV2.ApplyKnockback(Vector3.zero, rootDuration);
                }

            }
        }

        yield return new WaitForSeconds(rootDuration);

        if (ProjectilePoolManager.Instance != null)
        {
            ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        ReleasePlayer();
    }
    
    private void ReleasePlayer()
    {
        if (trappedPlayerCtrl != null)
        {
            trappedPlayerCtrl.enabled = true;
            trappedPlayerCtrl = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, trapRadius);
    }
}



using UnityEngine;
using System.Collections;

public class BossRootTrap : MonoBehaviour
{
    [Header("Tiempos del Ataque")]
    public float followTime = 2f;        // Tiempo persiguiendo al jugador
    public float lockWarningTime = 0.5f; // Tiempo congelada antes de salir
    public float rootDuration = 2f;      // Tiempo que el jugador queda atrapado

    [Header("�rea y Efectos")]
    public float trapRadius = 4f;
    public Transform warningVisual;      // C�rculo rojo/transparente
    public Transform rootVisual;         // Objeto con las ra�ces modelo 3D

    private Transform playerTarget;
    private float damage;
    private CopiaPlayerController2 trappedPlayerCtrl; // Guardamos al jugador si lo atrapamos

    public void Setup(Transform target, float dmg)
    {
        playerTarget = target;
        damage = dmg;

        // Ajustamos la escala del c�rculo al radio de la trampa
        if (warningVisual != null) warningVisual.localScale = new Vector3(trapRadius * 2f, 0.1f, trapRadius * 2f);

        // Escondemos las ra�ces al principio
        if (rootVisual != null) rootVisual.gameObject.SetActive(false);

        StartCoroutine(TrapRoutine());
    }

    private IEnumerator TrapRoutine()
    {

        float timer = 0f;
        while (timer < followTime)
        {
            if (playerTarget != null)
            {
                // Ahora copia la altura (Y) del jugador, subi�ndola un pel�n (0.1) para que no raspe
                Vector3 targetPos = new Vector3(playerTarget.position.x, playerTarget.position.y + 0.1f, playerTarget.position.z);
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (warningVisual != null)
        {
            // Comprobamos que el objeto exista en la escena y no sea un archivo
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

                // Aplicamos Da�o
                PlayerHealth hp = hit.GetComponent<PlayerHealth>();
                if (hp != null) hp.TakeDamage(damage);

                // Intentamos usar el Stun Inteligente de la Versi�n 2 del Player
                CopiaPlayerController2 playerV2 = hit.GetComponent<CopiaPlayerController2>();
                if (playerV2 != null)
                {
                    // Le mandamos "0" fuerza, pero "rootDuration" de tiempo de aturdimiento.
                    // El jugador no saldr� volando, pero su movimiento se anular� durante 2 segundos.
                    playerV2.ApplyKnockback(Vector3.zero, rootDuration);
                }

            }
        }


        yield return new WaitForSeconds(rootDuration);



        Destroy(gameObject);
    }

    // Seguro contra bugs: Si la trampa es destruida por error (ej. al terminar el nivel), 
    // nos aseguramos de liberar al jugador si estaba atrapado.
    private void OnDestroy()
    {
        if (trappedPlayerCtrl != null)
        {
            trappedPlayerCtrl.enabled = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, trapRadius);
    }
}
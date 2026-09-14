using PatronesAplicados.RealImplementation;
using UnityEngine;
using System.Collections;

public class NEWEnemyFireSorcerer : NEWEnemySorcererBase
{
   // [Header("Ataque de Fuego")]
   // public float attackDamage = 20f;
 //   [Tooltip("Arrastra aqu el Prefab del rectngulo de fuego")]
   // public GameObject fireLinePrefab;

    protected override void Start()
    {
        base.Start();

        // Forzamos las estadsticas
       // maxHealth = 50f;
       // goldReward = 10;

       // if (attackCooldown < 10f) attackCooldown = 10f;
        //if (attackRange < 40f) attackRange = 40f;
        //if (fleeDistance < 20f) fleeDistance = 20f;
    }

    protected override void StartAttackRoutine()
    {
        StartCoroutine(PerformAttackRoutine());
    }

    private IEnumerator PerformAttackRoutine()
    {
        isChargingAttack = true; // Activa el bloqueo de movimiento

        if (animator != null) animator.SetTrigger("Cast");

        if (data.FireLinePrefab != null && player != null)
        {
            // Creamos la zona en los pies del mago, mirando fijamente hacia el jugador
            Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;

            GameObject lineObj;
            if (ProjectilePoolManager.Instance != null)
            {
                lineObj = ProjectilePoolManager.Instance.GetProjectile(data.FireLinePrefab, spawnPos, Quaternion.LookRotation(direction));
            }
            else
            {
                lineObj = Instantiate(data.FireLinePrefab, spawnPos, Quaternion.LookRotation(direction));
            }
            
            // Reemplazo a NEWEnemyFireLine
            NEWEnemyFireLine newFireLine = lineObj.GetComponent<NEWEnemyFireLine>();
            if (newFireLine != null)
            {
                newFireLine.Setup(data.AttackDamage * damageMultiplier, 50f);
            }
            else
            {
                // Respaldo por si usan el viejo script
                EnemyFireLine fireLine = lineObj.GetComponent<EnemyFireLine>();
                if (fireLine != null) fireLine.Setup(data.AttackDamage * damageMultiplier, 50f);
            }
        }

        // El hechicero se queda quieto "invocando" un ratito para que concuerde con la advertencia
        yield return new WaitForSeconds(1.5f);

        lastAttackTime = Time.time;
        isChargingAttack = false; // Se puede volver a mover o huir
    }
}

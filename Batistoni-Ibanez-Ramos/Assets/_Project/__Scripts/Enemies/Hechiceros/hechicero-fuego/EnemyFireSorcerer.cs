using UnityEngine;
using System.Collections;

public class EnemyFireSorcerer : EnemySorcererBase
{
    [Header("Ataque de Fuego")]
    public float attackDamage = 20f;
    [Tooltip("Arrastra aquí el Prefab del rectángulo de fuego")]
    public GameObject fireLinePrefab;

    protected override void Start()
    {
        base.Start();

        // Forzamos las estadísticas que pediste en el diseño
        maxHealth = 50f;
        goldReward = 10;

        if (attackCooldown < 10f) attackCooldown = 10f;
        if (attackRange < 40f) attackRange = 40f;
        if (fleeDistance < 20f) fleeDistance = 20f;
    }

    protected override IEnumerator PerformAttackRoutine()
    {
        isChargingAttack = true; // Activa el bloqueo de movimiento

        if (animator != null) animator.SetTrigger("Cast");

        if (fireLinePrefab != null && player != null)
        {
            // Creamos la zona en los pies del mago, mirando fijamente hacia el jugador
            Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;

            GameObject lineObj = Instantiate(fireLinePrefab, spawnPos, Quaternion.LookRotation(direction));
            EnemyFireLine fireLine = lineObj.GetComponent<EnemyFireLine>();

            if (fireLine != null)
            {
                // El rectángulo medirá 50 metros (así atraviesa al jugador aunque esté a 40m)
                fireLine.Setup(attackDamage * damageMultiplier, 50f);
            }
        }

        // El hechicero se queda quieto "invocando" un ratito para que concuerde con la advertencia
        yield return new WaitForSeconds(1.5f);

        lastAttackTime = Time.time;
        isChargingAttack = false; // Se puede volver a mover o huir
    }
}
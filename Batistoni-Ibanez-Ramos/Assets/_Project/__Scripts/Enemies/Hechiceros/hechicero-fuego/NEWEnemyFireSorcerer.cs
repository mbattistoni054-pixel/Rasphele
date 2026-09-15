using PatronesAplicados.RealImplementation;
using UnityEngine;
using System.Collections;

public class NEWEnemyFireSorcerer : NEWEnemySorcererBase
{
    protected override void Start()
    {
        base.Start();

    }

    protected override void StartAttackRoutine()
    {
        StartCoroutine(PerformAttackRoutine());
    }

    private IEnumerator PerformAttackRoutine()
    {
        isChargingAttack = true;

        if (animator != null) animator.SetTrigger("Cast");

        if (data.FireLinePrefab != null && player != null)
        {
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
            
            NEWEnemyFireLine newFireLine = lineObj.GetComponent<NEWEnemyFireLine>();
            if (newFireLine != null)
            {
                newFireLine.Setup(data.AttackDamage * damageMultiplier, 50f);
            }
            else
            {
                EnemyFireLine fireLine = lineObj.GetComponent<EnemyFireLine>();
                if (fireLine != null) fireLine.Setup(data.AttackDamage * damageMultiplier, 50f);
            }
        }

        yield return new WaitForSeconds(1.5f);

        lastAttackTime = Time.time;
        isChargingAttack = false; 
    }
}

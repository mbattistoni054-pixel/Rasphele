using System.Collections;
using UnityEngine;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class NEWEnemyFireSorcerer : NEWEnemySorcererBase
{
    private const float CastRecoveryDuration = 1.5f;
    private const float FireLineLength = 50f;

    protected override void StartAttackRoutine()
    {
        StartCoroutine(PerformAttackRoutine());
    }

    private IEnumerator PerformAttackRoutine()
    {
        isChargingAttack = true;
        attackCooldownTimer = 0f;

        if (animator != null) animator.SetTrigger(CastHash);

        if (data != null && data.FireLinePrefab != null && player != null)
        {
            Vector3 spawnPos = transform.position;
            Vector3 direction = player.position - spawnPos;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = transform.forward;
            }
            else
            {
                direction.Normalize();
            }

            Quaternion spawnRotation = Quaternion.LookRotation(direction);
            transform.rotation = spawnRotation;

            NEWEnemyFireLine fireLine = null;
            if (ProjectilePoolManager.Instance != null)
            {
                fireLine = ProjectilePoolManager.Instance.GetProjectile<NEWEnemyFireLine>(data.FireLinePrefab, spawnPos, spawnRotation);
            }
            else
            {
                GameObject lineObj = Instantiate(data.FireLinePrefab, spawnPos, spawnRotation);
                lineObj.TryGetComponent(out fireLine);
            }

            if (fireLine != null)
            {
                fireLine.Setup(data.AttackDamage * damageMultiplier, FireLineLength);
            }
        }

        float recoveryTimer = CastRecoveryDuration;
        while (recoveryTimer > 0f)
        {
            if (isDead || isStunned)
            {
                isChargingAttack = false;
                yield break;
            }

            if (!isPaused && !PauseManager.IsPaused)
            {
                recoveryTimer -= Time.deltaTime;
            }
            yield return null;
        }

        attackCooldownTimer = 0f;
        isChargingAttack = false;
    }
}
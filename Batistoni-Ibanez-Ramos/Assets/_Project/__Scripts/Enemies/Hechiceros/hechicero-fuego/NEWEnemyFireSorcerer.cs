using System.Collections;
using UnityEngine;
using PatronesAplicados;
using PatronesAplicados.RealImplementation;

public class NEWEnemyFireSorcerer : NEWEnemySorcererBase
{
    [Header("Ajustes de Línea de Fuego (Inspector)")]
    [Tooltip("Si es mayor a 0, sobrescribe la duración de la línea de fuego. Si es 0, usa la del EnemyData o la del Prefab.")]
    public float fireLineDurationOverride = 0f;
    [Tooltip("Longitud de la línea de fuego (si EnemyData no define una).")]
    public float fireLineLength = 50f;
    [Tooltip("Tiempo que el hechicero se queda quieto recuperándose tras lanzar el ataque.")]
    public float castRecoveryDuration = 1.5f;

    protected override void StartAttackRoutine()
    {
        StartCoroutine(PerformAttackRoutine());
    }

    private IEnumerator PerformAttackRoutine()
    {
        isChargingAttack = true;
        attackCooldownTimer = 0f;

        TriggerAttackAnimation();

        if (data != null && data.FireLinePrefab != null && player != null)
        {
            Vector3 spawnPos = transform.position;

            Vector3 aimDirection3D = player.position - spawnPos;

            Vector3 flatDirection = new Vector3(aimDirection3D.x, 0f, aimDirection3D.z);
            if (flatDirection.sqrMagnitude < 0.0001f)
            {
                flatDirection = transform.forward;
            }
            else
            {
                flatDirection.Normalize();
            }
            transform.rotation = Quaternion.LookRotation(flatDirection);

            if (aimDirection3D.sqrMagnitude < 0.0001f)
            {
                aimDirection3D = flatDirection;
            }
            else
            {
                aimDirection3D.Normalize();
            }
            Quaternion fireLineRotation = Quaternion.LookRotation(aimDirection3D, Vector3.up);

            NEWEnemyFireLine fireLine = null;
            if (ProjectilePoolManager.Instance != null)
            {
                fireLine = ProjectilePoolManager.Instance.GetProjectile<NEWEnemyFireLine>(data.FireLinePrefab, spawnPos, fireLineRotation);
            }
            else
            {
                GameObject lineObj = Instantiate(data.FireLinePrefab, spawnPos, fireLineRotation);
                lineObj.TryGetComponent(out fireLine);
            }

            if (fireLine != null)
            {
                float lengthToUse = data.FireLineLength > 0f ? data.FireLineLength : fireLineLength;
                float durationToUse = fireLineDurationOverride > 0f
                    ? fireLineDurationOverride
                    : data.FireLineDuration;

                fireLine.Setup(
                    data.AttackDamage * damageMultiplier,
                    lengthToUse,
                    durationToUse,
                    data.FireLineWarningTime
                );
            }
        }

        float recoveryTimer = castRecoveryDuration;
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
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OrbWeapon : WeaponBase
{
    public Transform firePoint;
    public LayerMask enemyLayer;

    protected override void Attack()
    {
        List<Transform> enemies = GetAllEnemiesInRange();

        if (enemies.Count > 0)
        {
            StartCoroutine(ShootRoutine());
        }
        else
        {
            attackSuccessful = false;
        }
    }

    private List<Transform> GetAllEnemiesInRange()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, GetFinalRange(), enemyLayer);
        List<Transform> validEnemies = new List<Transform>();

        foreach (Collider enemy in enemiesInRange)
        {
            if (enemy != null) validEnemies.Add(enemy.transform);
        }

        validEnemies.Sort((a, b) =>
        {
            float distA = Vector3.Distance(transform.position, a.position);
            float distB = Vector3.Distance(transform.position, b.position);
            return distA.CompareTo(distB);
        });

        return validEnemies;
    }

    private IEnumerator ShootRoutine()
    {
        int enemyIndex = 0;

        for (int i = 0; i < currentMultipleShots; i++)
        {
            List<Transform> enemies = GetAllEnemiesInRange();

            if (enemies.Count == 0) break;

            Transform targetForThisBullet = enemies[enemyIndex % enemies.Count];
            enemyIndex++;

            GameObject bulletObj = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.identity);
            HomingProjectile projectile = bulletObj.GetComponent<HomingProjectile>();

            if (projectile != null)
            {
                projectile.Setup(targetForThisBullet, GetFinalDamage(), currentChainBounces, currentExplosiveRadius, enemyLayer, currentEffects, WeaponID, DamageType);
            }

            if (currentMultipleShots > 1) yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (data != null && PlayerStats.Instance != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, GetFinalRange());
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

namespace PatronesAplicados.RealImplementation
{
    public class ShurikenWeaponRefactored : WeaponBaseRealRefactored
    {
        [Header("Ajustes del Shuriken")]
        public LayerMask enemyLayer;
        public float orbitRadius = 2f;
        public float orbitSpeed = 120f;

        [HideInInspector] public bool isMagicMode = false;

        private List<ShurikenProjectileRefactored> shurikens = new List<ShurikenProjectileRefactored>();
        private float currentOrbitAngle = 0f;
        private float internalFireTimer = 0f;

        protected override void Start()
        {
            base.Start();
            UpdateShurikenCount();
        }

        protected override void Update()
        {
            if (data == null) return;

            CurrentDamageType = isMagicMode ? DamageType.Magico : data.damageType;

            currentCooldownTimer += Time.deltaTime;
            float actualCooldown = CurrentBaseCooldown * cachedGlobalFireRateMult;

            UpdateShurikenCount();

            currentOrbitAngle += orbitSpeed * Time.deltaTime;
            if (currentOrbitAngle >= 360f) currentOrbitAngle -= 360f;

            float angleStep = 360f / Mathf.Max(1, shurikens.Count);

            for (int i = 0; i < shurikens.Count; i++)
            {
                if (shurikens[i] != null)
                {
                    float angle = currentOrbitAngle + (i * angleStep);
                    Vector3 offset = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad)) * orbitRadius;
                    Vector3 targetOrbitPos = transform.position + Vector3.up * 1f + offset; 

                    shurikens[i].UpdateOrbitPosition(targetOrbitPos);
                }
            }

            internalFireTimer += Time.deltaTime;

            if (internalFireTimer >= 0.05f)
            {
                Attack();
                internalFireTimer = 0f;
            }
        }

        protected override void Attack()
        {
            Transform nearestEnemy = FindNearestEnemy();
            if (nearestEnemy == null)
            {
                attackSuccessful = false;
                return;
            }

            bool fired = false;

            foreach (var shuriken in shurikens)
            {
                if (shuriken != null && shuriken.IsIdle)
                {
                    shuriken.Fire(nearestEnemy, GetFinalDamage(), CurrentEffects, WeaponID, CurrentDamageType, enemyLayer, CurrentProjectileSpeed);
                    fired = true;
                    break; 
                }
            }

            if (!fired) attackSuccessful = false;
        }

        private void UpdateShurikenCount()
        {
            if (data == null || data.projectilePrefab == null) return;

            shurikens.RemoveAll(s => s == null || !s.gameObject.activeInHierarchy);

            while (shurikens.Count < CurrentMultipleShots)
            {
                GameObject obj = ProjectilePoolManager.Instance.GetProjectile(data.projectilePrefab, transform.position, Quaternion.identity);
                ShurikenProjectileRefactored proj = obj.GetComponent<ShurikenProjectileRefactored>();
                
                if (proj != null) 
                {
                    shurikens.Add(proj);
                    proj.ForceIdleState();
                }
                else 
                {
                    ProjectilePoolManager.Instance.ReturnProjectile(obj);
                }
            }

            while (shurikens.Count > CurrentMultipleShots)
            {
                int lastIndex = shurikens.Count - 1;
                if (shurikens[lastIndex] != null) 
                {
                    ProjectilePoolManager.Instance.ReturnProjectile(shurikens[lastIndex].gameObject);
                }
                shurikens.RemoveAt(lastIndex);
            }
        }

        private Transform FindNearestEnemy()
        {
            Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, GetFinalRange(), enemyLayer);
            float shortestDist = Mathf.Infinity;
            Transform nearest = null;

            foreach (Collider col in enemiesInRange)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    nearest = col.transform;
                }
            }
            return nearest;
        }

        public override void ResetWeaponStats()
        {
            base.ResetWeaponStats();
            isMagicMode = false;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class OrbWeaponRefactored : WeaponBaseRealRefactored
    {
        public Transform firePoint;
        public LayerMask enemyLayer;

        private static readonly WaitForSeconds _burstWait = new WaitForSeconds(0.1f);

        private readonly Collider[] _overlapBuffer = new Collider[64];
        private readonly List<Transform> _validEnemiesBuffer = new List<Transform>(64);
        private Comparison<Transform> _distanceComparison;

        protected override void Awake()
        {
            base.Awake();
            _distanceComparison = CompareDistanceToWeapon;
        }

        protected override void Attack()
        {
            RefreshEnemiesInRange();

            if (_validEnemiesBuffer.Count > 0)
            {
                StartCoroutine(ShootRoutine());
            }
            else
            {
                attackSuccessful = false;
            }
        }

        private void RefreshEnemiesInRange()
        {
            _validEnemiesBuffer.Clear();

            int count = Physics.OverlapSphereNonAlloc(transform.position, GetFinalRange(), _overlapBuffer, enemyLayer);
            for (int i = 0; i < count; i++)
            {
                Collider col = _overlapBuffer[i];
                if (col != null && col.gameObject.activeInHierarchy)
                {
                    if (DamageableRegistry.TryGet(col, out _, out Transform rootTransform))
                    {
                        if (!_validEnemiesBuffer.Contains(rootTransform))
                        {
                            _validEnemiesBuffer.Add(rootTransform);
                        }
                    }
                    else
                    {
                        _validEnemiesBuffer.Add(col.transform);
                    }
                }
            }

            if (_validEnemiesBuffer.Count > 1)
            {
                if (_distanceComparison == null) _distanceComparison = CompareDistanceToWeapon;
                _validEnemiesBuffer.Sort(_distanceComparison);
            }
        }

        private void PruneInactiveTargets()
        {
            for (int i = _validEnemiesBuffer.Count - 1; i >= 0; i--)
            {
                Transform t = _validEnemiesBuffer[i];
                if (t == null || !t.gameObject.activeInHierarchy)
                {
                    _validEnemiesBuffer.RemoveAt(i);
                }
            }
        }

        private int CompareDistanceToWeapon(Transform a, Transform b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;

            Vector3 myPos = transform.position;
            float sqrDistA = (a.position - myPos).sqrMagnitude;
            float sqrDistB = (b.position - myPos).sqrMagnitude;
            return sqrDistA.CompareTo(sqrDistB);
        }

        private IEnumerator ShootRoutine()
        {
            int shotsToFire = runtimeStats.multipleShots;
            int enemyIndex = 0;

            for (int i = 0; i < shotsToFire; i++)
            {
                if (i > 0)
                {
                    PruneInactiveTargets();
                    if (_validEnemiesBuffer.Count == 0)
                    {
                        RefreshEnemiesInRange();
                    }
                }

                if (_validEnemiesBuffer.Count == 0) yield break;

                Transform targetForThisBullet = _validEnemiesBuffer[enemyIndex % _validEnemiesBuffer.Count];
                enemyIndex++;

                SpawnProjectile(targetForThisBullet);

                if (shotsToFire > 1 && i < shotsToFire - 1)
                {
                    yield return _burstWait;
                }
            }
        }

        private void SpawnProjectile(Transform target)
        {
            if (data.projectilePrefab == null) return;

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            IProjectile projectile = null;

            if (ProjectilePoolManager.Instance != null)
            {
                projectile = ProjectilePoolManager.Instance.GetProjectileInterface(data.projectilePrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                GameObject bulletObj = Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);
                bulletObj.TryGetComponent(out projectile);
            }

            if (projectile != null)
            {
                ProjectilePayload payload = CreateProjectilePayload(target, enemyLayer);
                projectile.Initialize(in payload);
            }
        }
    }
}
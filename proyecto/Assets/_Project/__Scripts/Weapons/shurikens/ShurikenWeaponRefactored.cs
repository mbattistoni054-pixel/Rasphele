using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class ShurikenWeaponRefactored : WeaponBaseRealRefactored
    {
        [Header("Ajustes del Shuriken")]
        public LayerMask enemyLayer;
        public float orbitRadius = 2f;
        public float orbitSpeed = 120f;

        [HideInInspector] public bool isMagicMode;

        private readonly List<ShurikenProjectileRefactored> _shurikens = new List<ShurikenProjectileRefactored>(8);
        private float _currentOrbitAngle;
        private float _internalFireTimer;

        private static readonly Collider[] HitBuffer = new Collider[64];

        protected override void Start()
        {
            base.Start();
            UpdateShurikenCount();
        }

        protected override void Update()
        {
            if (data == null || isPaused) return;

            CurrentDamageType = isMagicMode ? DamageType.Magico : data.damageType;

            float dt = Time.deltaTime;
            currentCooldownTimer += dt;

            UpdateShurikenCount();

            _currentOrbitAngle += orbitSpeed * dt;
            if (_currentOrbitAngle >= 360f) _currentOrbitAngle -= 360f;

            int activeCount = _shurikens.Count;
            float angleStep = 360f / Mathf.Max(1, activeCount);
            Vector3 orbitCenter = transform.position + Vector3.up;

            for (int i = 0; i < activeCount; i++)
            {
                ShurikenProjectileRefactored shuriken = _shurikens[i];
                if (shuriken != null)
                {
                    float angleRad = (_currentOrbitAngle + (i * angleStep)) * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(Mathf.Sin(angleRad), 0f, Mathf.Cos(angleRad)) * orbitRadius;
                    shuriken.UpdateOrbitPosition(orbitCenter + offset);
                }
            }

            _internalFireTimer += dt;
            if (_internalFireTimer >= 0.05f)
            {
                Attack();
                _internalFireTimer = 0f;
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

            for (int i = 0; i < _shurikens.Count; i++)
            {
                ShurikenProjectileRefactored shuriken = _shurikens[i];
                if (shuriken != null && shuriken.IsIdle)
                {
                    shuriken.Fire(nearestEnemy, GetFinalDamage(), CurrentEffects, WeaponID, CurrentDamageType, enemyLayer, CurrentProjectileSpeed);
                    fired = true;
                    break;
                }
            }

            if (!fired)
            {
                attackSuccessful = false;
            }
        }

        private void UpdateShurikenCount()
        {
            if (data == null || data.projectilePrefab == null || ProjectilePoolManager.Instance == null) return;

            for (int i = _shurikens.Count - 1; i >= 0; i--)
            {
                if (_shurikens[i] == null || !_shurikens[i].gameObject.activeInHierarchy)
                {
                    _shurikens.RemoveAt(i);
                }
            }

            while (_shurikens.Count < CurrentMultipleShots)
            {
                ShurikenProjectileRefactored proj = ProjectilePoolManager.Instance.GetProjectile<ShurikenProjectileRefactored>(data.projectilePrefab, transform.position, Quaternion.identity);

                if (proj != null)
                {
                    _shurikens.Add(proj);
                    proj.ForceIdleState();
                }
                else
                {
                    break;
                }
            }

            while (_shurikens.Count > CurrentMultipleShots)
            {
                int lastIndex = _shurikens.Count - 1;
                if (_shurikens[lastIndex] != null)
                {
                    ProjectilePoolManager.Instance.ReturnProjectile(_shurikens[lastIndex].gameObject);
                }
                _shurikens.RemoveAt(lastIndex);
            }
        }

        private Transform FindNearestEnemy()
        {
            Vector3 myPos = transform.position;
            int count = Physics.OverlapSphereNonAlloc(myPos, GetFinalRange(), HitBuffer, enemyLayer);

            float shortestSqrDist = Mathf.Infinity;
            Transform nearest = null;

            for (int i = 0; i < count; i++)
            {
                Collider col = HitBuffer[i];
                if (col == null || !col.gameObject.activeInHierarchy) continue;

                Transform candidate = DamageableRegistry.TryGet(col, out _, out Transform root) && root != null
                    ? root
                    : col.transform;

                float sqrDist = (candidate.position - myPos).sqrMagnitude;
                if (sqrDist < shortestSqrDist)
                {
                    shortestSqrDist = sqrDist;
                    nearest = candidate;
                }
            }

            return nearest;
        }

        public override void ResetWeaponStats()
        {
            base.ResetWeaponStats();
            isMagicMode = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (ProjectilePoolManager.Instance != null)
            {
                for (int i = _shurikens.Count - 1; i >= 0; i--)
                {
                    if (_shurikens[i] != null)
                    {
                        ProjectilePoolManager.Instance.ReturnProjectile(_shurikens[i].gameObject);
                    }
                }
            }
            _shurikens.Clear();
        }
    }
}
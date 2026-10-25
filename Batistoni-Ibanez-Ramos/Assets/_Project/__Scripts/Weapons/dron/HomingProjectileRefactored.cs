using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class HomingProjectileRefactored : MonoBehaviour, IProjectile, IPausable
    {
        public float speed = 15f;
        public float maxLifeTime = 5f;
        public float bounceSearchRadius = 15f;

        [Header("Efectos Visuales")]
        public GameObject explosionVisualPrefab;

        private Transform _target;
        private float _damage;
        private float _currentSpeed;
        private int _bouncesLeft;
        private float _expRadius;
        private LayerMask _enemyMask;
        private WeaponData.ImpactEffects _effects;
        private int _myWeaponID;
        private DamageType _myDamageType;

        private float _lifeTimer;
        private bool _isReturned;
        private bool _isPaused;

        private readonly HashSet<Transform> _alreadyHitEnemies = new HashSet<Transform>();
        private readonly HashSet<IDamageable> _explosionDamagedTargets = new HashSet<IDamageable>();
        private static readonly Collider[] _hitBuffer = new Collider[32];

        public void Initialize(in ProjectilePayload payload)
        {
            _target = payload.Target;
            _damage = payload.FinalDamage;
            _currentSpeed = payload.Speed > 0f ? payload.Speed : speed;
            _bouncesLeft = payload.ChainBounces;
            _expRadius = payload.ExplosiveRadius;
            _enemyMask = payload.EnemyMask;
            _effects = payload.Effects;
            _myWeaponID = payload.WeaponID;
            _myDamageType = payload.DamageType;

            _lifeTimer = 0f;
            _isReturned = false;
            _alreadyHitEnemies.Clear();
        }

        public void Setup(Transform enemyTarget, float finalDamage, int chain, float explosionRad, LayerMask mask, WeaponData.ImpactEffects weaponEffects, int wID, DamageType dType)
        {
            ProjectilePayload payload = new ProjectilePayload(enemyTarget, finalDamage, speed, chain, explosionRad, mask, weaponEffects, wID, dType);
            Initialize(in payload);
        }

        private void OnEnable()
        {
            _lifeTimer = 0f;
            _isReturned = false;
            _currentSpeed = speed;
            _isPaused = PauseManager.IsPaused;
            PauseManager.Register(this);
        }

        private void OnDisable()
        {
            PauseManager.Unregister(this);
        }

        private void Update()
        {
            if (_isReturned || _isPaused) return;

            _lifeTimer += Time.deltaTime;
            if (_lifeTimer >= maxLifeTime)
            {
                ReturnToPool();
                return;
            }

            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                _target = FindNextTarget();
                if (_target == null)
                {
                    ReturnToPool();
                    return;
                }
            }

            Vector3 aimTarget = _target.position + Vector3.up;
            Vector3 direction = (aimTarget - transform.position).normalized;

            transform.position += direction * (_currentSpeed * Time.deltaTime);

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isReturned || _isPaused) return;

            if (!DamageableRegistry.TryGet(other, out IDamageable damageableTarget, out Transform targetRoot))
            {
                return;
            }

            _alreadyHitEnemies.Add(targetRoot);

            bool isCrit = Random.Range(0f, 100f) <= _effects.critChance;

            if (_expRadius > 0f)
            {
                SpawnExplosionVisual();
                ExplodeAreaDamage(isCrit);
            }
            else
            {
                ApplyDamageAndEffects(damageableTarget, isCrit);
            }

            if (_bouncesLeft > 0)
            {
                _bouncesLeft--;
                Transform newTarget = FindNextTarget();

                if (newTarget != null)
                {
                    _target = newTarget;
                    return;
                }
            }

            ReturnToPool();
        }

        private void SpawnExplosionVisual()
        {
            if (explosionVisualPrefab == null) return;

            GameObject visualObj = VfxPoolManager.Instance != null
                ? VfxPoolManager.Instance.GetVfx(explosionVisualPrefab, transform.position, Quaternion.identity, 2f)
                : Instantiate(explosionVisualPrefab, transform.position, Quaternion.identity);

            if (visualObj != null)
            {
                float visualScale = _expRadius * 2f;
                visualObj.transform.localScale = new Vector3(visualScale, visualScale, visualScale);
            }
        }

        private void ExplodeAreaDamage(bool isCrit)
        {
            _explosionDamagedTargets.Clear();
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _expRadius, _hitBuffer, _enemyMask);

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _hitBuffer[i];
                if (hit == null) continue;

                if (DamageableRegistry.TryGet(hit, out IDamageable hitDamageable, out _) &&
                    _explosionDamagedTargets.Add(hitDamageable))
                {
                    ApplyDamageAndEffects(hitDamageable, isCrit);
                }
            }
        }

        private void ApplyDamageAndEffects(IDamageable damageable, bool isCrit)
        {
            damageable.TakeDamage(_damage, isCrit, _myDamageType);

            if (damageable is IEffectReceivable effectReceivable)
            {
                effectReceivable.ApplyEffects(_effects, _myWeaponID);
            }
        }

        private Transform FindNextTarget()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, bounceSearchRadius, _hitBuffer, _enemyMask);
            float shortestSqr = Mathf.Infinity;
            Transform nextTarget = null;
            Vector3 myPos = transform.position;

            for (int i = 0; i < count; i++)
            {
                Collider col = _hitBuffer[i];
                if (col == null || !col.gameObject.activeInHierarchy) continue;

                if (!DamageableRegistry.TryGet(col, out _, out Transform candidateTransform)) continue;
                if (candidateTransform == null || !candidateTransform.gameObject.activeInHierarchy) continue;
                if (_alreadyHitEnemies.Contains(candidateTransform)) continue;

                float sqrDist = (candidateTransform.position - myPos).sqrMagnitude;
                if (sqrDist < shortestSqr)
                {
                    shortestSqr = sqrDist;
                    nextTarget = candidateTransform;
                }
            }

            return nextTarget;
        }

        private void ReturnToPool()
        {
            if (_isReturned) return;
            _isReturned = true;

            if (ProjectilePoolManager.Instance != null)
            {
                ProjectilePoolManager.Instance.ReturnProjectile(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void OnPause() => _isPaused = true;
        public void OnResume() => _isPaused = false;
    }
}
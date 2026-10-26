using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class MushroomEntityRefactored : MonoBehaviour, IPausable
    {
        [Header("Efectos Visuales")]
        public Transform sporeVisualArea;

        private float _damage;
        private float _radius;
        private float _duration;
        private WeaponData.ImpactEffects _effects;
        private float _healAmount;
        private int _weaponID;
        private DamageType _damageType;
        private LayerMask _enemyMask;

        private float _tickTimer;
        private float _lifeTimer;
        private bool _isReturned;
        private bool _isPaused;

        private static readonly Collider[] HitBuffer = new Collider[64];
        private static readonly HashSet<IDamageable> DamagedTargets = new HashSet<IDamageable>();

        private static float _pausableClock;
        private static int _lastClockFrame = -1;
        private static float _nextAllowedPlayerHealTime;

        private static Transform _cachedPlayerTransform;
        private static IHealable _cachedPlayerHealable;

        private void OnEnable()
        {
            _tickTimer = 0f;
            _lifeTimer = 0f;
            _isReturned = false;
            _isPaused = PauseManager.IsPaused;
            PauseManager.Register(this);
        }

        private void OnDisable()
        {
            PauseManager.Unregister(this);
        }

        public void Setup(float dmg, float sporeRadius, float duration, WeaponData.ImpactEffects weaponEffects, float heal, int wID, DamageType dType, LayerMask mask)
        {
            _damage = dmg;
            _radius = sporeRadius;
            _duration = duration;
            _effects = weaponEffects;
            _healAmount = heal;
            _weaponID = wID;
            _damageType = dType;
            _enemyMask = mask;

            _tickTimer = 0f;
            _lifeTimer = 0f;
            _isReturned = false;

            if (sporeVisualArea != null)
            {
                float diameter = _radius * 2f;
                sporeVisualArea.localScale = new Vector3(diameter, diameter, diameter);
            }
        }

        private void Update()
        {
            if (_isReturned || _isPaused) return;

            float dt = Time.deltaTime;

            if (Time.frameCount != _lastClockFrame)
            {
                _lastClockFrame = Time.frameCount;
                _pausableClock += dt;
            }

            _lifeTimer += dt;
            if (_lifeTimer >= _duration)
            {
                ReturnToPool();
                return;
            }

            _tickTimer += dt;
            if (_tickTimer >= 1f)
            {
                _tickTimer -= 1f;
                SporeTick();
            }
        }

        private void SporeTick()
        {
            DamagedTargets.Clear();
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _radius, HitBuffer, _enemyMask);

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = HitBuffer[i];
                if (hit == null) continue;

                if (DamageableRegistry.TryGet(hit, out IDamageable enemy, out _) && DamagedTargets.Add(enemy))
                {
                    bool isCrit = Random.Range(0f, 100f) <= _effects.critChance;
                    enemy.TakeDamage(_damage, isCrit, _damageType);
                    enemy.ApplyEffects(_effects, _weaponID);
                }
            }

            if (_healAmount > 0f && _pausableClock >= _nextAllowedPlayerHealTime)
            {
                TryHealPlayerInRadius();
            }
        }

        private void TryHealPlayerInRadius()
        {
            Transform playerTransform = GameManagerRefactored.Instance != null
                ? GameManagerRefactored.Instance.Player
                : null;

            if (playerTransform == null) return;

            float sqrDist = (playerTransform.position - transform.position).sqrMagnitude;
            if (sqrDist > _radius * _radius) return;

            if (_cachedPlayerTransform != playerTransform || _cachedPlayerHealable == null)
            {
                _cachedPlayerTransform = playerTransform;
                playerTransform.TryGetComponent(out _cachedPlayerHealable);
            }

            if (_cachedPlayerHealable != null)
            {
                _nextAllowedPlayerHealTime = _pausableClock + 0.95f;
                _cachedPlayerHealable.Heal(_healAmount);
            }
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
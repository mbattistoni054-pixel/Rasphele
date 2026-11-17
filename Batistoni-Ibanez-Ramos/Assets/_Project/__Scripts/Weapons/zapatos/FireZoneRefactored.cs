using System.Collections.Generic;
using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class FireZoneRefactored : MonoBehaviour, IPausable
    {
        private float _damage;
        private WeaponData.ImpactEffects _effects;
        private int _myWeaponID;
        private DamageType _myDamageType;
        private float _playerHealAmount;
        private float _duration;

        private float _lifeTimer;
        private float _checkTimer;
        private bool _isReturned;
        private bool _isPaused;

        private static readonly Collider[] HitBuffer = new Collider[64];
        private static readonly Dictionary<int, float> LastDamageTimes = new Dictionary<int, float>(128);
        private static readonly List<int> KeysToRemoveBuffer = new List<int>(64);

        private static float _pausableClock;
        private static int _lastClockFrame = -1;
        private static float _nextCleanupTime;
        private static float _nextPlayerHealTime;

        private static Transform _cachedPlayerTransform;
        private static IHealable _cachedPlayerHealable;

        private void OnEnable()
        {
            _lifeTimer = 0f;
            _checkTimer = 0f;
            _isReturned = false;
            _isPaused = PauseManager.IsPaused;
            PauseManager.Register(this);
        }

        private void OnDisable()
        {
            PauseManager.Unregister(this);
        }

        public void Setup(float weaponDamage, WeaponData.ImpactEffects weaponEffects, float weaponRange, int wID, DamageType dType, float pHealAmount, float duration)
        {
            _damage = weaponDamage;
            _effects = weaponEffects;
            _myWeaponID = wID;
            _myDamageType = dType;
            _playerHealAmount = pHealAmount;
            _duration = duration;

            _lifeTimer = 0f;
            _checkTimer = 0.1f; 
            _isReturned = false;

            transform.localScale = new Vector3(weaponRange, 0.2f, weaponRange);
        }

        private void Update()
        {
            if (_isReturned || _isPaused) return;

            float dt = Time.deltaTime;

            if (Time.frameCount != _lastClockFrame)
            {
                _lastClockFrame = Time.frameCount;
                _pausableClock += dt;

                if (_pausableClock > _nextCleanupTime)
                {
                    CleanUpDictionary();
                }
            }

            _lifeTimer += dt;
            if (_lifeTimer >= _duration)
            {
                ReturnToPool();
                return;
            }

            _checkTimer += dt;
            if (_checkTimer >= 0.1f)
            {
                _checkTimer = 0f;
                EvaluateFireZone();
            }
        }

        private void EvaluateFireZone()
        {
            Vector3 halfExtents = transform.localScale * 0.5f;
            int hitCount = Physics.OverlapBoxNonAlloc(transform.position, halfExtents, HitBuffer, transform.rotation);

            for (int i = 0; i < hitCount; i++)
            {
                Collider other = HitBuffer[i];
                if (other == null) continue;

                if (DamageableRegistry.TryGet(other, out IDamageable targetDamageable, out Transform targetRoot))
                {

                    if (targetRoot != null && targetRoot.CompareTag("Player")) continue;

                    int enemyId = targetRoot != null ? targetRoot.GetInstanceID() : other.gameObject.GetInstanceID();

                    if (!LastDamageTimes.TryGetValue(enemyId, out float lastHitTime) || _pausableClock >= lastHitTime + 1f)
                    {
                        LastDamageTimes[enemyId] = _pausableClock;
                        targetDamageable.TakeDamage(_damage, false, _myDamageType);
                        targetDamageable.ApplyEffects(_effects, _myWeaponID);
                    }
                }
            }

            if (_playerHealAmount > 0f && _pausableClock >= _nextPlayerHealTime)
            {
                TryHealPlayerInZone(halfExtents);
            }
        }

        private void TryHealPlayerInZone(Vector3 halfExtents)
        {
            Transform playerTransform = GameManagerRefactored.Instance != null
                ? GameManagerRefactored.Instance.Player
                : null;

            if (playerTransform == null) return;

            Vector3 localOffset = Quaternion.Inverse(transform.rotation) * (playerTransform.position - transform.position);
            if (Mathf.Abs(localOffset.x) > halfExtents.x ||
                Mathf.Abs(localOffset.y) > halfExtents.y + 1.5f ||
                Mathf.Abs(localOffset.z) > halfExtents.z)
            {
                return;
            }

            if (_cachedPlayerTransform != playerTransform || _cachedPlayerHealable == null)
            {
                _cachedPlayerTransform = playerTransform;
                playerTransform.TryGetComponent(out _cachedPlayerHealable);
            }

            if (_cachedPlayerHealable != null)
            {
                _nextPlayerHealTime = _pausableClock + 1f;
                _cachedPlayerHealable.Heal(_playerHealAmount);
            }
        }

        private static void CleanUpDictionary()
        {
            _nextCleanupTime = _pausableClock + 30f;
            KeysToRemoveBuffer.Clear();

            foreach (var kvp in LastDamageTimes)
            {
                if (_pausableClock >= kvp.Value + 5f)
                {
                    KeysToRemoveBuffer.Add(kvp.Key);
                }
            }

            for (int i = 0; i < KeysToRemoveBuffer.Count; i++)
            {
                LastDamageTimes.Remove(KeysToRemoveBuffer[i]);
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
    }
}
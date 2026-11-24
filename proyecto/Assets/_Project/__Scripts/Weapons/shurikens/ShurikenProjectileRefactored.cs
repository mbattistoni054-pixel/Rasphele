using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class ShurikenProjectileRefactored : MonoBehaviour, IPausable
    {
        public enum State { Orbiting, Attacking, Returning }
        public State currentState = State.Orbiting;

        [Header("Visuales")]
        public Transform visualTransform;
        public Vector3 rotationSpeed = new Vector3(0f, 1000f, 0f);

        private Vector3 _targetOrbitPos;
        private Transform _targetEnemy;
        private IDamageable _targetDamageable;

        private float _damage;
        private WeaponData.ImpactEffects _effects;
        private int _weaponID;
        private DamageType _damageType;
        private LayerMask _enemyMask;
        private float _speed = 20f;
        private bool _isPaused;

        public bool IsIdle => currentState == State.Orbiting;

        private void OnEnable()
        {
            _isPaused = PauseManager.IsPaused;
            PauseManager.Register(this);
        }

        private void OnDisable()
        {
            PauseManager.Unregister(this);
        }

        public void ForceIdleState()
        {
            currentState = State.Orbiting;
            _targetEnemy = null;
            _targetDamageable = null;
        }

        public void UpdateOrbitPosition(Vector3 pos)
        {
            _targetOrbitPos = pos;
        }

        public void Fire(Transform enemy, float dmg, WeaponData.ImpactEffects fx, int wID, DamageType dType, LayerMask mask, float flightSpeed)
        {
            _targetEnemy = enemy;
            _targetDamageable = null;

            if (_targetEnemy != null)
            {
                if (_targetEnemy.TryGetComponent(out Collider col))
                {
                    DamageableRegistry.TryGet(col, out _targetDamageable, out _);
                }
                if (_targetDamageable == null)
                {
                    _targetEnemy.TryGetComponent(out _targetDamageable);
                }
            }

            _damage = dmg;
            _effects = fx;
            _weaponID = wID;
            _damageType = dType;
            _enemyMask = mask;
            _speed = flightSpeed > 0f ? flightSpeed : 20f;

            currentState = State.Attacking;
        }

        private void Update()
        {
            if (_isPaused) return;

            float dt = Time.deltaTime;

            if (visualTransform != null)
            {
                visualTransform.Rotate(rotationSpeed * dt);
            }

            switch (currentState)
            {
                case State.Orbiting:
                    transform.position = Vector3.Lerp(transform.position, _targetOrbitPos, 15f * dt);
                    break;

                case State.Attacking:
                    if (_targetEnemy == null || !_targetEnemy.gameObject.activeInHierarchy)
                    {
                        currentState = State.Returning;
                        return;
                    }

                    Vector3 toTarget = _targetEnemy.position - transform.position;
                    if (toTarget.sqrMagnitude <= 1.0f)
                    {
                        if (_targetDamageable == null)
                        {
                            _targetEnemy.TryGetComponent(out _targetDamageable);
                        }

                        ApplyDamage(_targetDamageable);
                        currentState = State.Returning;
                        return;
                    }

                    transform.position += toTarget.normalized * (_speed * dt);
                    break;

                case State.Returning:
                    Vector3 toOrbit = _targetOrbitPos - transform.position;
                    float returnDist = _speed * dt;

                    if (toOrbit.sqrMagnitude <= returnDist * returnDist)
                    {
                        transform.position = _targetOrbitPos;
                        currentState = State.Orbiting;
                    }
                    else
                    {
                        transform.position += toOrbit.normalized * returnDist;
                    }
                    break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isPaused || currentState != State.Attacking) return;

            if (((1 << other.gameObject.layer) & _enemyMask) != 0 || other.CompareTag("Enemy"))
            {
                if (DamageableRegistry.TryGet(other, out IDamageable dmg, out _))
                {
                    ApplyDamage(dmg);
                }

                currentState = State.Returning;
            }
        }

        private void ApplyDamage(IDamageable dmg)
        {
            if (dmg == null) return;

            bool isCrit = Random.Range(0f, 100f) <= _effects.critChance;
            dmg.TakeDamage(_damage, isCrit, _damageType);
            dmg.ApplyEffects(_effects, _weaponID);
        }

        public void OnPause() => _isPaused = true;
        public void OnResume() => _isPaused = false;
    }
}
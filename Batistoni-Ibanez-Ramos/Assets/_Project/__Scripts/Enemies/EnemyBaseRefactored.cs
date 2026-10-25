using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{
    public abstract class EnemyBaseRefactored : MonoBehaviour, IDamageable, IEffectReceivable, IObservable, IPausable
    {
        public const string ActionTakeDamage = "TakeDamage";
        public const string ActionDie = "Die";
        private const float FlashDuration = 0.1f;

        private readonly List<IObserver> _allObservers = new List<IObserver>(4);

        [SerializeField] protected EnemyData data;

        [Header("Object Pool")]
        public string poolKey;

        [Header("Estadísticas Base")]
        public float maxHealth;

        [Header("Multiplicador de Dificultad")]
        protected float damageMultiplier = 1f;

        protected float currentHealth;
        public float CurrentHealth => currentHealth;

        protected float currentSpeed;
        protected Transform player;
        protected NavMeshAgent agent;
        protected Rigidbody rb;
        protected Collider enemyCollider;

        [Header("UI y Recompensas")]
        public GameObject xpOrbPrefab;
        public GameObject damagePopupPrefab;

        [Header("Efectos Visuales")]
        public Material flashMaterial;

        [Header("Estado de Efectos")]
        protected bool isStunned;
        protected bool isDead;
        protected bool isPaused;

        [SerializeField] private EnemyVisual enemyVisual;

        protected virtual bool UsesNavigation => true;

        private readonly EnemyStatusEffectHandler _statusHandler = new EnemyStatusEffectHandler();
        private readonly EnemyFlashController _flashController = new EnemyFlashController();

        private Action<float, bool, DamageType, bool> _cachedDamageCallback;
        private Animator _cachedAnimator;
        private bool _isInitialized;

        protected virtual void Awake()
        {
            InitializeComponents();
        }

        protected virtual void OnEnable()
        {
            if (enemyCollider != null)
            {
                DamageableRegistry.Register(enemyCollider, this, transform);
            }

            PauseManager.Register(this);
        }

        protected virtual void OnDisable()
        {
            PauseManager.Unregister(this);
        }

        private void InitializeComponents()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _cachedDamageCallback = TakeDamage;
            _statusHandler.BindCallbacks(RecalculateSpeed, OnStunStateChanged);

            agent = GetComponent<NavMeshAgent>();
            rb = GetComponent<Rigidbody>();
            enemyCollider = GetComponent<Collider>();
            _cachedAnimator = GetComponentInChildren<Animator>();

            if (enemyCollider != null)
            {
                DamageableRegistry.Register(enemyCollider, this, transform);
            }

            _flashController.Initialize(this);

            if (data != null)
            {
                maxHealth = data.MaxHealth;
                currentHealth = maxHealth;
                currentSpeed = UsesNavigation ? data.BaseSpeed : 0f;
            }
        }

        protected virtual void Start()
        {
            InitializeComponents();

            if (player == null && GameManagerRefactored.Instance != null)
            {
                player = GameManagerRefactored.Instance.Player;
            }

            ConfigureInitialNavigation();
        }

        protected virtual void ConfigureInitialNavigation()
        {
            if (agent == null) return;

            if (UsesNavigation)
            {
                agent.speed = currentSpeed;
            }
            else
            {
                agent.speed = 0f;
                if (agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
            }
        }

        protected virtual void OnDestroy()
        {
            PauseManager.Unregister(this);

            if (enemyCollider != null)
            {
                DamageableRegistry.Unregister(enemyCollider);
            }
        }

        public virtual void ResetStats()
        {
            InitializeComponents();
            StopAllCoroutines();

            isDead = false;
            isStunned = false;
            isPaused = PauseManager.IsPaused;
            _statusHandler.ResetAll();
            _flashController.RestoreOriginalMaterials();

            currentHealth = maxHealth;
            currentSpeed = UsesNavigation ? (data != null ? data.BaseSpeed : currentSpeed) : 0f;

            if (enemyCollider != null)
            {
                enemyCollider.enabled = true;
                DamageableRegistry.Register(enemyCollider, this, transform);
            }

            if (agent != null)
            {
                agent.enabled = true;
                ConfigureInitialNavigation();
                if (UsesNavigation && agent.isOnNavMesh)
                {
                    agent.isStopped = isPaused;
                }
            }
        }

        public void ApplyDifficulty(float hpMult, float dmgMult)
        {
            InitializeComponents();

            float baseHp = data != null ? data.MaxHealth : maxHealth;
            maxHealth = baseHp * hpMult;
            currentHealth = maxHealth;
            damageMultiplier = dmgMult;
        }

        public void SetTarget(Transform newTarget)
        {
            player = newTarget;
        }

        protected virtual void Update()
        {
            if (isDead || isPaused) return;

            if (player == null && GameManagerRefactored.Instance != null)
            {
                player = GameManagerRefactored.Instance.Player;
            }

            float dt = Time.deltaTime;
            _flashController.Tick(dt);
            _statusHandler.Tick(dt, currentHealth, _cachedDamageCallback);
        }

        public virtual void TakeDamage(float amount, bool isCrit = false, DamageType type = DamageType.Fisico, bool isBleed = false)
        {
            if (isDead) return;

            float multiplier = LevelResistanceManager.Instance != null
                ? LevelResistanceManager.Instance.GetDamageMultiplier(type)
                : 1f;

            float finalDamage = amount * multiplier;
            if (isCrit) finalDamage *= 2f;

            currentHealth -= finalDamage;

            NotifyObservers(ActionTakeDamage);
            EnemyFeedbackService.SpawnDamagePopup(damagePopupPrefab, transform.position, finalDamage, type, isCrit, isBleed);

            if (currentHealth <= 0f)
            {
                Die();
                return;
            }

            _flashController.TriggerFlash(flashMaterial, FlashDuration);
        }

        public Transform GetTransform() => transform;

        public void ApplyEffects(WeaponData.ImpactEffects effects, int sourceWeaponID)
        {
            if (isDead) return;

            float baseSpd = data != null ? data.BaseSpeed : currentSpeed;
            _statusHandler.ApplyAllEffects(in effects, sourceWeaponID, baseSpd);
        }

        protected virtual void RecalculateSpeed()
        {
            if (!UsesNavigation) return;

            float baseSpd = data != null ? data.BaseSpeed : currentSpeed;
            currentSpeed = _statusHandler.CalculateCurrentSpeed(baseSpd);
            if (agent != null && agent.enabled) agent.speed = currentSpeed;
        }

        protected virtual void OnStunStateChanged(bool stunState)
        {
            isStunned = stunState;
            if (UsesNavigation && agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = stunState || isPaused;
            }
        }

        public virtual void OnPause()
        {
            isPaused = true;
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
            if (_cachedAnimator != null)
            {
                _cachedAnimator.speed = 0f;
            }
        }

        public virtual void OnResume()
        {
            isPaused = false;
            if (UsesNavigation && !isDead && !isStunned && agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
            if (_cachedAnimator != null)
            {
                _cachedAnimator.speed = 1f;
            }
        }

        protected virtual void Die()
        {
            if (isDead) return;
            isDead = true;

            StopAllCoroutines();
            _flashController.RestoreOriginalMaterials();

            if (enemyCollider != null) enemyCollider.enabled = false;
            if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;

            NotifyObservers(ActionDie);
            EnemyFeedbackService.SpawnDeathRewards(xpOrbPrefab, transform.position, data);

            if (enemyVisual == null && _allObservers.Count == 0)
            {
                ReturnEnemy();
            }
        }

        public virtual void ReturnEnemy()
        {
            if (EnemyPool.Instance != null && !string.IsNullOrEmpty(poolKey))
            {
                EnemyPool.Instance.ReturnEnemy(this, poolKey);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Subscribe(IObserver observer)
        {
            if (observer != null && !_allObservers.Contains(observer))
            {
                _allObservers.Add(observer);
            }
        }

        public void Unsubscribe(IObserver observer)
        {
            if (observer != null)
            {
                _allObservers.Remove(observer);
            }
        }

        public void NotifyObservers(string action)
        {
            for (int i = _allObservers.Count - 1; i >= 0; i--)
            {
                _allObservers[i]?.OnNotify(action);
            }
        }
    }
}
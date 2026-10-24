using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{
    public abstract class EnemyBaseRefactored : MonoBehaviour, IDamageable, IObservable
    {
        private readonly List<IObserver> _allObservers = new List<IObserver>();

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
        private Material[] originalMaterials;
        private Renderer[] renderers;

        [Header("Estado de Efectos")]
        protected bool isStunned = false;
        protected bool isDead = false;

        [SerializeField] private EnemyVisual enemyVisual;

        private readonly EnemyStatusEffectHandler statusHandler = new EnemyStatusEffectHandler();
        private System.Action<float, bool, DamageType, bool> cachedDamageCallback;
        private bool isInitialized = false;

        protected virtual void Awake()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            if (isInitialized) return;
            isInitialized = true;

            cachedDamageCallback = TakeDamage;
            agent = GetComponent<NavMeshAgent>();
            rb = GetComponent<Rigidbody>();
            enemyCollider = GetComponent<Collider>();

            renderers = GetComponentsInChildren<Renderer>();
            if (renderers != null && renderers.Length > 0)
            {
                originalMaterials = new Material[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null)
                    {
                        originalMaterials[i] = renderers[i].sharedMaterial;
                    }
                }
            }

            if (data != null)
            {
                maxHealth = data.MaxHealth;
                currentHealth = maxHealth;
                currentSpeed = data.BaseSpeed;
            }
        }

        protected virtual void Start()
        {
            InitializeComponents();

            if (player == null && GameManagerRefactored.Instance != null)
            {
                player = GameManagerRefactored.Instance.Player;
            }

            if (agent != null) agent.speed = currentSpeed;
        }

        public virtual void ResetStats()
        {
            InitializeComponents();
            StopAllCoroutines();

            isDead = false;
            isStunned = false;
            statusHandler.ResetAll();

            currentHealth = maxHealth;
            currentSpeed = data != null ? data.BaseSpeed : currentSpeed;

            if (enemyCollider != null) enemyCollider.enabled = true;

            if (agent != null)
            {
                agent.enabled = true;
                agent.speed = currentSpeed;
                if (agent.isOnNavMesh) agent.isStopped = false;
            }

            RestoreOriginalMaterials();
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
            if (isDead) return;

            if (player == null && GameManagerRefactored.Instance != null)
            {
                player = GameManagerRefactored.Instance.Player;
            }

            statusHandler.Tick(Time.deltaTime, currentHealth, cachedDamageCallback);
        }

        public virtual void TakeDamage(float amount, bool isCrit, DamageType type, bool isBleed = false)
        {
            if (isDead) return;

            float multiplier = LevelResistanceManager.Instance != null
                ? LevelResistanceManager.Instance.GetDamageMultiplier(type)
                : 1f;

            float finalDamage = amount * multiplier;
            if (isCrit) finalDamage *= 2f;

            currentHealth -= finalDamage;

            NotifyObservers("TakeDamage");
            SpawnDamagePopup(finalDamage, type, isCrit, isBleed);

            if (currentHealth <= 0f)
            {
                Die();
                return;
            }

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(FlashWhite());
            }
        }

        private void SpawnDamagePopup(float finalDamage, DamageType type, bool isCrit, bool isBleed)
        {
            if (damagePopupPrefab == null) return;

            Vector3 spawnPosition = transform.position + Vector3.up * 2.5f;
            GameObject popup = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();

            if (popupScript != null)
            {
                popupScript.Setup(finalDamage, type, isCrit, false, isBleed);
            }
        }

        public Transform GetTransform() => transform;

        public void ApplyEffects(WeaponData.ImpactEffects effects, int sourceWeaponID)
        {
            if (isDead) return;

            if (effects.burnDamage > 0f) statusHandler.ApplyBurn(effects.burnDamage, sourceWeaponID);
            if (effects.bleedPercent > 0f) statusHandler.ApplyBleed(effects.bleedPercent, sourceWeaponID);
            if (effects.poisonDamage > 0f) statusHandler.ApplyPoison(effects.poisonDamage, sourceWeaponID);
            if (effects.freezePercent > 0f) ApplyFreeze(effects.freezePercent);
            if (effects.stunChance > 0f) ApplyStun(effects.stunChance);
        }

        private void ApplyFreeze(float slowPercent)
        {
            if (!statusHandler.CanApplyFreeze() || !gameObject.activeInHierarchy) return;

            float baseSpd = data != null ? data.BaseSpeed : currentSpeed;
            float speedReduction = baseSpd * (slowPercent / 100f);
            StartCoroutine(statusHandler.FreezeRoutine(speedReduction, RecalculateSpeed));
        }

        private void RecalculateSpeed()
        {
            float baseSpd = data != null ? data.BaseSpeed : currentSpeed;
            currentSpeed = statusHandler.CalculateCurrentSpeed(baseSpd);
            if (agent != null && agent.enabled) agent.speed = currentSpeed;
        }

        private void ApplyStun(float stunChance)
        {
            if (!gameObject.activeInHierarchy || !statusHandler.TryRollStun(stunChance)) return;

            StartCoroutine(statusHandler.StunRoutine(stunState =>
            {
                isStunned = stunState;
                if (agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.isStopped = stunState;
                }
            }));
        }

        private IEnumerator FlashWhite()
        {
            if (flashMaterial == null || renderers == null) yield break;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) renderers[i].sharedMaterial = flashMaterial;
            }

            yield return new WaitForSeconds(0.1f);

            RestoreOriginalMaterials();
        }

        private void RestoreOriginalMaterials()
        {
            if (renderers == null || originalMaterials == null) return;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && originalMaterials[i] != null)
                {
                    renderers[i].sharedMaterial = originalMaterials[i];
                }
            }
        }

        protected virtual void Die()
        {
            if (isDead) return;
            isDead = true;

            StopAllCoroutines();
            RestoreOriginalMaterials();

            if (enemyCollider != null) enemyCollider.enabled = false;
            if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;

            NotifyObservers("Die");

            if (xpOrbPrefab != null)
            {
                if (ProjectilePoolManager.Instance != null)
                {
                    ProjectilePoolManager.Instance.GetProjectile(xpOrbPrefab, transform.position, Quaternion.identity);
                }
                else
                {
                    Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
                }
            }

            if (EventManager.Instance != null)
            {
                if (data != null)
                {
                    EventManager.Instance.TriggerEvent<int>("EnemyKilled_GoldReward", data.GoldReward);
                }
                EventManager.Instance.TriggerEvent("EnemyDied");
            }

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
                _allObservers.Add(observer);
        }

        public void Unsubscribe(IObserver observer)
        {
            if (observer != null)
                _allObservers.Remove(observer);
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
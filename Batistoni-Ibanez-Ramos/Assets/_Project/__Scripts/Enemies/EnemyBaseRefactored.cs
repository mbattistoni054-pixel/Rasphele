using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

namespace PatronesAplicados
{
    public abstract class EnemyBaseRefactored : MonoBehaviour, IDamageable, IObservable
    {

        private List<IObserver> _allObservers = new();
        
        [SerializeField] protected EnemyData data;

        [Header("Object Pool")]
        public string poolKey;

        [Header("Estadisticas Base")]
        public float maxHealth;

        [Header("Multiplicador de Dificultad")]
        protected float damageMultiplier = 1f;

        protected float currentHealth;
        public float CurrentHealth => currentHealth;

        protected float currentSpeed;
        protected Transform player;
        protected NavMeshAgent agent;
        protected Rigidbody rb;

        [Header("UI y Recompensas")]
        public GameObject xpOrbPrefab;
        public GameObject damagePopupPrefab;
        
        [Header("Efectos Visuales")]
        public Material flashMaterial;
        private Material[] originalMaterials;
        private Renderer[] renderers;

        [Header("Estado de Efectos")]
        protected bool isStunned = false;
        private float stunImmunityTimer = 0f;

        protected class DoTData
        {
            public float timeLeft;
            public float tickTimer;
            public float value;
        }

        private Dictionary<int, DoTData> activeBurns = new Dictionary<int, DoTData>();
        private Dictionary<int, DoTData> activeBleeds = new Dictionary<int, DoTData>();
        private Dictionary<int, DoTData> activePoisons = new Dictionary<int, DoTData>();

        private List<float> activeSlows = new List<float>();

        protected virtual void Start()
        {
            player = GameManagerRefactored.Instance.Player;
            
            maxHealth = data.MaxHealth;
            currentHealth = data.MaxHealth;
            currentSpeed = data.BaseSpeed;

            agent = GetComponent<NavMeshAgent>();
            rb = GetComponent<Rigidbody>();

            if (agent != null) agent.speed = currentSpeed;

            renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                originalMaterials = new Material[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                {
                    originalMaterials[i] = renderers[i].material;
                }
            }
        }

        public virtual void ResetStats()
        {
            currentHealth = maxHealth;
            currentSpeed = data.BaseSpeed;

            if (agent != null)
            {
                agent.enabled = true;
                agent.speed = currentSpeed;
                agent.isStopped = false;
            }

            isStunned = false;
            stunImmunityTimer = 0f;

            activeBurns.Clear();
            activeBleeds.Clear();
            activePoisons.Clear();
            activeSlows.Clear();

            if (renderers != null && originalMaterials != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null && originalMaterials[i] != null)
                    {
                        renderers[i].material = originalMaterials[i];
                    }
                }
            }
        }

        public void ApplyDifficulty(float hpMult, float dmgMult)
        {
            maxHealth *= hpMult;
            currentHealth = maxHealth;
            damageMultiplier = dmgMult;
        }

        public void SetTarget(Transform newTarget)
        {
            player = newTarget;
        }

        protected virtual void Update()
        {
            if (player == null)
            {
                player = GameManagerRefactored.Instance.Player;
            }

            if (stunImmunityTimer > 0) stunImmunityTimer -= Time.deltaTime;

            ProcessBurnTicks();
            ProcessBleedTicks();
            ProcessPoisonTicks();
        }

        private void ProcessBurnTicks()
        {
            if (activeBurns.Count == 0) return;
            List<int> keys = new List<int>(activeBurns.Keys);
            foreach (int key in keys)
            {
                DoTData dot = activeBurns[key];
                dot.timeLeft -= Time.deltaTime;
                dot.tickTimer -= Time.deltaTime;

                if (dot.tickTimer <= 0)
                {
                    dot.tickTimer += 1f;
                    TakeDamage(dot.value, false, DamageType.Fuego);
                }

                if (dot.timeLeft <= 0) activeBurns.Remove(key);
            }
        }

        private void ProcessBleedTicks()
        {
            if (activeBleeds.Count == 0) return;
            List<int> keys = new List<int>(activeBleeds.Keys);
            foreach (int key in keys)
            {
                DoTData dot = activeBleeds[key];
                dot.timeLeft -= Time.deltaTime;
                dot.tickTimer -= Time.deltaTime;

                if (dot.tickTimer <= 0)
                {
                    dot.tickTimer += 1f;
                    float bleedDmg = currentHealth * (dot.value / 100f);
                    TakeDamage(bleedDmg, false, DamageType.Fisico, true);
                }

                if (dot.timeLeft <= 0) activeBleeds.Remove(key);
            }
        }

        private void ProcessPoisonTicks()
        {
            if (activePoisons.Count == 0) return;
            List<int> keys = new List<int>(activePoisons.Keys);
            foreach (int key in keys)
            {
                DoTData dot = activePoisons[key];
                dot.timeLeft -= Time.deltaTime;
                dot.tickTimer -= Time.deltaTime;

                if (dot.tickTimer <= 0)
                {
                    dot.tickTimer += 1f;
                    TakeDamage(dot.value, false, DamageType.Veneno);
                }

                if (dot.timeLeft <= 0) activePoisons.Remove(key);
            }
        }

        public virtual void TakeDamage(float amount, bool isCrit, DamageType type, bool isBleed = false)
        {
            float multiplier = 1f;
            if (LevelResistanceManager.Instance != null)
            {
                multiplier = LevelResistanceManager.Instance.GetDamageMultiplier(type);
            }

            float finalDamage = amount * multiplier;
            if (isCrit) finalDamage *= 2f;

            currentHealth -= finalDamage;

            NotifyObservers("Damage");

            if (damagePopupPrefab != null)
            {
                Vector3 spawnPosition = transform.position + Vector3.up * 2.5f;
                GameObject popup = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);
                DamagePopup popupScript = popup.GetComponent<DamagePopup>();

                if (popupScript != null)
                {
                    popupScript.Setup(finalDamage, type, isCrit, false, isBleed);
                }
            }

            if (gameObject.activeInHierarchy) StartCoroutine(FlashWhite());

            if (currentHealth <= 0) Die();
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public void ApplyEffects(WeaponData.ImpactEffects effects, int sourceWeaponID)
        {
            if (effects.burnDamage > 0) ApplyBurn(effects.burnDamage, sourceWeaponID);
            if (effects.bleedPercent > 0) ApplyBleed(effects.bleedPercent, sourceWeaponID);
            if (effects.poisonDamage > 0) ApplyPoison(effects.poisonDamage, sourceWeaponID);
            if (effects.freezePercent > 0) ApplyFreeze(effects.freezePercent);
            if (effects.stunChance > 0) ApplyStun(effects.stunChance);
        }

        private void ApplyBurn(float burnDamage, int weaponID)
        {
            if (!activeBurns.ContainsKey(weaponID))
                activeBurns[weaponID] = new DoTData { timeLeft = 3f, tickTimer = 1f, value = burnDamage };
            else
            {
                activeBurns[weaponID].timeLeft = 3f;
                activeBurns[weaponID].value = burnDamage;
            }
        }

        private void ApplyBleed(float bleedPercent, int weaponID)
        {
            if (!activeBleeds.ContainsKey(weaponID))
                activeBleeds[weaponID] = new DoTData { timeLeft = 3f, tickTimer = 1f, value = bleedPercent };
            else
            {
                activeBleeds[weaponID].timeLeft = 3f;
                activeBleeds[weaponID].value = bleedPercent;
            }
        }

        private void ApplyPoison(float poisonDamage, int weaponID)
        {
            if (!activePoisons.ContainsKey(weaponID))
                activePoisons[weaponID] = new DoTData { timeLeft = 3f, tickTimer = 1f, value = poisonDamage };
            else
            {
                activePoisons[weaponID].timeLeft = 3f;
                activePoisons[weaponID].value = poisonDamage;
            }
        }

        private void ApplyFreeze(float slowPercent)
        {
            if (activeSlows.Count >= 3) return;

            float speedReduction = data.BaseSpeed * (slowPercent / 100f);
            if (gameObject.activeInHierarchy) StartCoroutine(FreezeRoutine(speedReduction));
        }

        private IEnumerator FreezeRoutine(float speedReduction)
        {
            activeSlows.Add(speedReduction);
            RecalculateSpeed();

            yield return new WaitForSeconds(5f);

            activeSlows.Remove(speedReduction);
            RecalculateSpeed();
        }

        private void RecalculateSpeed()
        {
            currentSpeed = data.BaseSpeed;
            foreach (float reduction in activeSlows)
            {
                currentSpeed -= reduction;
            }

            if (currentSpeed < data.BaseSpeed * 0.1f) currentSpeed = data.BaseSpeed * 0.1f;
            if (agent != null) agent.speed = currentSpeed;
        }

        private void ApplyStun(float stunChance)
        {
            if (isStunned || stunImmunityTimer > 0) return;

            if (Random.Range(0f, 100f) <= stunChance)
            {
                if (gameObject.activeInHierarchy) StartCoroutine(StunRoutine());
            }
        }

        private IEnumerator StunRoutine()
        {
            isStunned = true;
            if (agent != null) agent.isStopped = true;

            yield return new WaitForSeconds(2f);

            isStunned = false;
            if (agent != null) agent.isStopped = false;
            stunImmunityTimer = 5f;
        }

        private IEnumerator FlashWhite()
        {
            if (flashMaterial == null || renderers == null) yield break;

            foreach (Renderer r in renderers)
            {
                if (r != null) r.material = flashMaterial;
            }

            yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && originalMaterials[i] != null)
                {
                    renderers[i].material = originalMaterials[i];
                }
            }
        }

        protected virtual void Die()
        {
            if (xpOrbPrefab != null)
            {
                if (PatronesAplicados.RealImplementation.ProjectilePoolManager.Instance != null)
                {
                    PatronesAplicados.RealImplementation.ProjectilePoolManager.Instance.GetProjectile(xpOrbPrefab, transform.position, Quaternion.identity);
                }
                else
                {
                    Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
                }
            }

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent<int>("EnemyKilled_GoldReward", data.GoldReward);
                EventManager.Instance.TriggerEvent("EnemyDied");
            }

            if (EnemyPool.Instance != null && !string.IsNullOrEmpty(poolKey))
            {
                EnemyPool.Instance.ReturnEnemy(gameObject, poolKey);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Subscribe(IObserver observer)
        {
            if (!_allObservers.Contains(observer))
                _allObservers.Add(observer);
        }

        public void Unsubscribe(IObserver observer)
        {
            if (_allObservers.Contains(observer))
                _allObservers.Remove(observer);
        }

        public void NotifyObservers(string action)
        {
          foreach(var observer in _allObservers)
            {
                observer.OnNotify(action);
            }
        }
    }
}

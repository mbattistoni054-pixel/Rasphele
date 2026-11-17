using System.Collections;
using UnityEngine;
using PatronesAplicados.RealImplementation;

namespace PatronesAplicados
{
    public class BossPlantRefactored : EnemyBaseRefactored
    {
        [Header("Ajustes de Escena (Todo lo demás se configura en EnemyData)")]
        public string bossDisplayName = "Planta Mutante";
        public float spawnHeightOffset = 0f;
        [Tooltip("Punto de salida de los orbes en el modelo 3D del jefe")]
        public Transform firePoint;
        [Tooltip("Animator del jefe en la escena")]
        public Animator animator;

        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int ShootHash = Animator.StringToHash("Shoot");
        private static readonly int CastHash = Animator.StringToHash("Cast");

        protected override bool UsesNavigation => false;

        private bool _isPhase2;
        private float _attackTimer;
        private bool _isAttacking;
        private bool _heightOffsetApplied;

        protected override void Awake()
        {
            base.Awake();
            if (data == null)
            {
                Debug.LogWarning($"[BossPlantRefactored] '{name}' no tiene asignado un EnemyData en el campo 'Data'.", this);
            }
        }

        protected override void Start()
        {
            base.Start();

            if (animator == null) animator = GetComponent<Animator>();

            if (!_heightOffsetApplied && spawnHeightOffset != 0f)
            {
                transform.position += Vector3.up * spawnHeightOffset;
                _heightOffsetApplied = true;
            }

            NotifyBossSpawned();
        }

        public override void ResetStats()
        {
            base.ResetStats();

            _isPhase2 = false;
            _isAttacking = false;
            _attackTimer = 0f;

            NotifyBossSpawned();
        }

        private void NotifyBossSpawned()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent(GameEventKeys.BossSpawned, bossDisplayName, currentHealth, maxHealth);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (isDead || isPaused || data == null || player == null || isStunned || _isAttacking) return;

            RotateTowardsPlayer(Time.deltaTime, 3f);

            _attackTimer += Time.deltaTime;
            if (_attackTimer >= data.AttackCooldown)
            {
                _attackTimer = 0f;

                float sqrDistance = (player.position - transform.position).sqrMagnitude;
                float meleeRange = data.MeleeRange;

                if (sqrDistance <= meleeRange * meleeRange)
                {
                    StartCoroutine(MeleeAttackRoutine());
                }
                else
                {
                    StartCoroutine(DecideAttackRoutine());
                }
            }
        }

        public override void TakeDamage(float amount, bool isCrit = false, DamageType type = DamageType.Fisico, bool isBleed = false)
        {
            if (isDead) return;

            if (_isPhase2)
            {
                amount *= 0.5f;
            }

            base.TakeDamage(amount, isCrit, type, isBleed);

            if (!isDead)
            {
                if (!_isPhase2 && currentHealth <= maxHealth * 0.5f)
                {
                    _isPhase2 = true;
                }

                if (EventManager.Instance != null)
                {
                    EventManager.Instance.TriggerEvent(GameEventKeys.BossHealthChanged, currentHealth, maxHealth);
                }
            }
        }

        public override void OnPause()
        {
            base.OnPause();
            if (animator != null) animator.speed = 0f;
        }

        public override void OnResume()
        {
            base.OnResume();
            if (animator != null) animator.speed = 1f;
        }

        private IEnumerator WaitPausableSeconds(float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                if (!isPaused && !PauseManager.IsPaused)
                {
                    elapsed += Time.deltaTime;
                }
                yield return null;
            }
        }

        private IEnumerator MeleeAttackRoutine()
        {
            _isAttacking = true;

            if (animator != null) animator.SetTrigger(AttackHash);

            yield return StartCoroutine(WaitPausableSeconds(data.MeleeHitDelay));

            float hitRadius = data.MeleeRange + 1f;
            if (!isDead && !isStunned && player != null &&
                (player.position - transform.position).sqrMagnitude <= hitRadius * hitRadius)
            {
                float finalMeleeDmg = data.MeleeDamage * damageMultiplier;

                if (player.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(finalMeleeDmg);
                }
                else if (player.TryGetComponent(out PlayerHealthRefactored hpRefactored))
                {
                    hpRefactored.TakeDamage(finalMeleeDmg);
                }

                Vector3 pushDir = player.position - transform.position;
                pushDir.y = 0f;
                pushDir.Normalize();
                Vector3 finalKnockback = (pushDir * data.BossKnockbackHorizontal) + (Vector3.up * data.BossKnockbackUpward);

                if (player.TryGetComponent(out IKnockbackable knockbackable))
                {
                    knockbackable.ApplyKnockback(finalKnockback, 0.4f);
                }
                else if (player.TryGetComponent(out PlayerControllerRefactored pcRefactored))
                {
                    pcRefactored.ApplyKnockback(finalKnockback, 0.4f);
                }
            }

            yield return StartCoroutine(WaitPausableSeconds(1f));
            _isAttacking = false;
        }

        private IEnumerator DecideAttackRoutine()
        {
            _isAttacking = true;
            float randomRoll = Random.Range(0f, 100f);

            if (randomRoll <= 70f)
            {
                yield return StartCoroutine(AttackA_Burst());
            }
            else
            {
                yield return StartCoroutine(AttackB_Roots_Routine());
            }

            _isAttacking = false;
        }

        private IEnumerator AttackA_Burst()
        {
            if (data == null || data.OrbPrefab == null) yield break;
            if (animator != null) animator.SetTrigger(ShootHash);

            int orbsToShoot = _isPhase2 ? data.OrbsPerBurstPhase2 : data.OrbsPerBurst;
            float finalDamage = data.AttackDamage * damageMultiplier;

            for (int i = 0; i < orbsToShoot; i++)
            {
                while (isPaused || PauseManager.IsPaused) yield return null;
                if (isDead || isStunned || player == null) break;

                Vector3 spawnOrigin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1.5f;
                Vector3 aimDirection = ((player.position + Vector3.up * 1f) - spawnOrigin).normalized;
                if (aimDirection == Vector3.zero) aimDirection = transform.forward;
                Quaternion rot = Quaternion.LookRotation(aimDirection);

                if (ProjectilePoolManager.Instance != null)
                {
                    NEWBossOrb orbScript = ProjectilePoolManager.Instance.GetProjectile<NEWBossOrb>(data.OrbPrefab, spawnOrigin, rot);
                    if (orbScript != null)
                    {
                        orbScript.Setup(finalDamage);
                    }
                }
                else
                {
                    GameObject orbObj = Instantiate(data.OrbPrefab, spawnOrigin, rot);
                    if (orbObj.TryGetComponent(out NEWBossOrb orbScript))
                    {
                        orbScript.Setup(finalDamage);
                    }
                }

                yield return StartCoroutine(WaitPausableSeconds(data.TimeBetweenOrbs));
            }
        }

        private IEnumerator AttackB_Roots_Routine()
        {
            if (data == null || data.RootTrapPrefab == null) yield break;

            int rootsToSpawn = _isPhase2 ? Mathf.Max(1, data.RootsPhase2) : 1;
            float finalDamage = data.AttackDamage * damageMultiplier;

            for (int i = 0; i < rootsToSpawn; i++)
            {
                while (isPaused || PauseManager.IsPaused) yield return null;
                if (isDead || isStunned || player == null) break;

                if (animator != null) animator.SetTrigger(CastHash);

                if (ProjectilePoolManager.Instance != null)
                {
                    NEWBossRootTrap trapScript = ProjectilePoolManager.Instance.GetProjectile<NEWBossRootTrap>(data.RootTrapPrefab, player.position, Quaternion.identity);
                    if (trapScript != null)
                    {
                        trapScript.Setup(player, finalDamage);
                    }
                }
                else
                {
                    GameObject trapObj = Instantiate(data.RootTrapPrefab, player.position, Quaternion.identity);
                    if (trapObj.TryGetComponent(out NEWBossRootTrap trapScript))
                    {
                        trapScript.Setup(player, finalDamage);
                    }
                }

                if (i < rootsToSpawn - 1)
                {
                    yield return StartCoroutine(WaitPausableSeconds(data.DelayBetweenRootsPhase2));
                }
            }

            yield return StartCoroutine(WaitPausableSeconds(1.5f));
        }

        protected override void Die()
        {
            if (isDead) return;
            _isAttacking = false;

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerEvent(GameEventKeys.BossDefeated);
                EventManager.Instance.TriggerEvent(GameEventKeys.LevelComplete);
            }

            base.Die();
        }

        private void OnDrawGizmosSelected()
        {
            if (data == null) return;

            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, data.MeleeRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, data.MeleeRange);
        }
    }
}
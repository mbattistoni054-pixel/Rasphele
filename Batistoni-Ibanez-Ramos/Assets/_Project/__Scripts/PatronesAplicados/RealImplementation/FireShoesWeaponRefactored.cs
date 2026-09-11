using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class FireShoesWeaponRefactored : WeaponBaseRealRefactored
    {
        [Header("Ajustes del Rastro")]
        public LayerMask groundMask;

        private Vector3 lastDropPosition;

        protected override void Start()
        {
            base.Start();
            lastDropPosition = transform.position;
        }

        protected override void Update()
        {
            if (data == null) return;

            currentCooldownTimer += Time.deltaTime;
            
            // Usamos CurrentBaseCooldown del Builder y el multiplicador en cach
            float actualCooldown = CurrentBaseCooldown * cachedGlobalFireRateMult;

            if (currentCooldownTimer >= actualCooldown)
            {
                bool isMoving = Vector3.Distance(transform.position, lastDropPosition) > 0.1f;
                bool isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, 1f, groundMask);

                if (isMoving && isGrounded)
                {
                    Attack();
                    lastDropPosition = transform.position;
                    currentCooldownTimer = 0f;
                }
            }
        }

        protected override void Attack()
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.05f;
            
            // ! PATRN POOL: Obtenemos el fuego del Pool
            GameObject fireObj = ProjectilePoolManager.Instance.GetProjectile(data.projectilePrefab, spawnPos, Quaternion.identity);

            FireZoneRefactored fireZone = fireObj.GetComponent<FireZoneRefactored>();
            if (fireZone != null)
            {
                float finalDuration = CurrentDuration > 0 ? CurrentDuration : 3f;

                // Usamos las estadsticas inyectadas por el Builder
                fireZone.Setup(
                    GetFinalDamage(), 
                    CurrentEffects, 
                    GetFinalRange(), 
                    WeaponID, 
                    CurrentDamageType, 
                    CurrentHeatHeal, 
                    finalDuration
                ); 
            }
        }
    }
}

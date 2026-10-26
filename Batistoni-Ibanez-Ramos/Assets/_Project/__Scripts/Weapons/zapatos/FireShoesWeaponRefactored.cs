using UnityEngine;

namespace PatronesAplicados.RealImplementation
{
    public class FireShoesWeaponRefactored : WeaponBaseRealRefactored
    {
        [Header("Ajustes del Rastro")]
        public LayerMask groundMask;

        private Vector3 _lastDropPosition;

        protected override void Start()
        {
            base.Start();
            _lastDropPosition = transform.position;
        }

        protected override void Attack()
        {
            if (data == null || data.projectilePrefab == null)
            {
                attackSuccessful = false;
                return;
            }

            bool isMoving = (transform.position - _lastDropPosition).sqrMagnitude > 0.01f;
            bool isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, 1f, groundMask, QueryTriggerInteraction.Ignore);

            if (!isMoving || !isGrounded)
            {
                attackSuccessful = false;
                return;
            }

            Vector3 spawnPos = transform.position + Vector3.up * 0.05f;
            FireZoneRefactored fireZone = null;

            if (ProjectilePoolManager.Instance != null)
            {
                fireZone = ProjectilePoolManager.Instance.GetProjectile<FireZoneRefactored>(
                    data.projectilePrefab,
                    spawnPos,
                    Quaternion.identity);
            }
            else
            {
                GameObject fireObj = Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);
                fireObj.TryGetComponent(out fireZone);
            }

            if (fireZone != null)
            {
                fireZone.Setup(GetFinalDamage(), CurrentEffects, GetFinalRange(), WeaponID, CurrentDamageType, CurrentHeatHeal, CurrentDuration);
            }

            _lastDropPosition = transform.position;
        }
    }
}
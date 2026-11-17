using UnityEngine;

namespace PatronesAplicados
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Config/Player Movement Config")]
    public class PlayerConfigSO : ScriptableObject
    {
        [Header("Ajustes de Velocidad")]
        public float walkSpeed = 4f;
        public float runSpeed = 8f;
        public float turnSmoothTime = 0.1f;

        [Header("Ajustes de Inclinación (Anti-Spiderman)")]
        [Tooltip("Ángulo máximo que el jugador puede subir caminando o corriendo.")]
        public float maxWalkableAngle = 45f;

        [Header("Ajustes de Salto")]
        public float jumpForce = 7f;
        public float fallMultiplier = 5f;
        public float lowJumpMultiplier = 4f;
        public float airControl = 5f;
        public float groundDistance = 0.2f;
        public LayerMask groundMask;

        [Header("Ajustes de Deslizamiento")]
        public float minSlideSpeed = 4f;
        public float slideBoost = 5f;
        public float slideDrag = 15f;
        public float slopeAcceleration = 10f;
        public float slideHeight = 1f;

        [Header("Ajustes de Dash")]
        public float dashSpeed = 50f;
        public float dashDuration = 0.2f;
        public float dashCooldown = 2f;

        [Header("Ajustes de Wall Jump")]
        public LayerMask wallMask;
        public float wallCheckDistance = 0.8f;
        public float wallSlideSpeed = -2f;
        public float wallJumpUpForce = 8f;
        public float wallJumpSideForce = 8f;

        [Header("Timers")]
        public float inputBufferTime = 0.2f;
        public float coyoteTime = 0.15f;
    }
}
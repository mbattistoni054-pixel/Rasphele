using UnityEngine;
using UnityEngine.InputSystem;

namespace PatronesAplicados
{
    public class PlayerControls
    {
        private readonly InputActionReference _move;
        private readonly InputActionReference _jump;
        private readonly InputActionReference _slide;
        private readonly InputActionReference _dash;
        private readonly InputActionReference _walkToggle;
        private readonly InputActionReference _pause;

        public float jumpBuffer;
        public float dashBuffer;
        public float slideBuffer;
        private readonly float _bufferTime;

        public PlayerControls(
            InputActionReference move,
            InputActionReference jump,
            InputActionReference slide,
            InputActionReference dash,
            InputActionReference walkToggle,
            InputActionReference pause,
            float bufferTime)
        {
            _move = move;
            _jump = jump;
            _slide = slide;
            _dash = dash;
            _walkToggle = walkToggle;
            _pause = pause;
            _bufferTime = bufferTime;
        }

        public void EnableKeys()
        {
            _move?.action?.Enable();
            _jump?.action?.Enable();
            _slide?.action?.Enable();
            _dash?.action?.Enable();
            _walkToggle?.action?.Enable();
            _pause?.action?.Enable();
        }

        public void DisableKeys()
        {
            _move?.action?.Disable();
            _jump?.action?.Disable();
            _slide?.action?.Disable();
            _dash?.action?.Disable();
            _walkToggle?.action?.Disable();
            _pause?.action?.Disable();
        }

        public void ListenKeys(float deltaTime)
        {
            if (jumpBuffer > 0f) jumpBuffer -= deltaTime;
            if (dashBuffer > 0f) dashBuffer -= deltaTime;
            if (slideBuffer > 0f) slideBuffer -= deltaTime;

            if (_jump?.action?.WasPressedThisFrame() == true) jumpBuffer = _bufferTime;
            if (_dash?.action?.WasPressedThisFrame() == true) dashBuffer = _bufferTime;
            if (_slide?.action?.WasPressedThisFrame() == true) slideBuffer = _bufferTime;
        }

        public Vector2 GetMoveInput() => _move?.action?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool IsJumpPressed() => _jump?.action?.IsPressed() ?? false;
        public bool IsSlidePressed() => _slide?.action?.IsPressed() ?? false;
        public bool WasWalkToggled() => _walkToggle?.action?.WasPressedThisFrame() ?? false;
        public bool WasPausePressed() => _pause?.action?.WasPressedThisFrame() ?? false;
    }
}
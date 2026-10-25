using System;
using UnityEngine;

namespace PatronesAplicados
{
    public enum PlayerState { Walking, Airborne, Sliding, WallRiding, Dashing }

    public class PlayerModel
    {
        public PlayerConfigSO Config { get; }

        private PlayerState _currentState = PlayerState.Walking;
        private bool _isGrounded;
        private bool _isRunning = true;
        private bool _isMoving;

        public PlayerState CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState == value) return;
                PlayerState prev = _currentState;
                _currentState = value;
                OnStateChanged?.Invoke(prev, _currentState);
            }
        }

        public bool IsGrounded
        {
            get => _isGrounded;
            set
            {
                if (_isGrounded == value) return;
                _isGrounded = value;
                OnGroundedChanged?.Invoke(_isGrounded);
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning == value) return;
                _isRunning = value;
                CurrentBaseSpeed = _isRunning ? Config.runSpeed : Config.walkSpeed;
                OnRunToggled?.Invoke(_isRunning);
            }
        }

        public bool IsMoving
        {
            get => _isMoving;
            set
            {
                if (_isMoving == value) return;
                _isMoving = value;
                OnMovingChanged?.Invoke(_isMoving);
            }
        }

        public float CurrentBaseSpeed { get; private set; }
        public Vector3 MoveDirection { get; set; }
        public Vector3 SlideDirection { get; set; }
        public Vector3 DashDirection { get; set; }
        public RaycastHit LastWallHit { get; set; }

        public float JumpCooldown { get; set; }
        public float CoyoteTimer { get; set; }
        public float DashTimeLeft { get; set; }
        public float DashCooldownTimer { get; private set; }
        public float KnockbackTimer { get; set; }
        public float RegenTimer { get; set; }

        public int AdditionalJumpsLeft { get; set; }
        public int CurrentDashesAvailable { get; set; } = 1;

        public event Action<PlayerState, PlayerState> OnStateChanged;
        public event Action<bool> OnGroundedChanged;
        public event Action<bool> OnRunToggled;
        public event Action<bool> OnMovingChanged;
        public event Action OnJumped;
        public event Action OnDashed;
        public event Action<bool> OnSlideChanged;
        public event Action<float, float> OnDashCooldownUpdated;

        public PlayerModel(PlayerConfigSO config)
        {
            Config = config;
            CurrentBaseSpeed = config.runSpeed;
        }

        public float GetFinalSpeed()
        {
            if (PlayerStatsRefactored.Instance != null)
                return PlayerStatsRefactored.Instance.GetTotalSpeed(CurrentBaseSpeed);

            return CurrentBaseSpeed;
        }

        public void SetDashCooldown(float timer)
        {
            if (Mathf.Approximately(DashCooldownTimer, timer)) return;
            DashCooldownTimer = timer;
            OnDashCooldownUpdated?.Invoke(DashCooldownTimer, Config.dashCooldown);
        }

        public void NotifyJump() => OnJumped?.Invoke();
        public void NotifyDash() => OnDashed?.Invoke();
        public void NotifySlide(bool isSliding) => OnSlideChanged?.Invoke(isSliding);
    }
}
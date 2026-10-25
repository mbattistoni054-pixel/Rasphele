using UnityEngine;

namespace PatronesAplicados
{

    public abstract class PlayerStateBase : IPlayerState
    {
        protected readonly PlayerControllerRefactored Controller;
        protected readonly PlayerModel Model;
        protected readonly PlayerMovement Movement;
        protected readonly PlayerControls Controls;
        protected readonly PlayerConfigSO Config;

        protected PlayerStateBase(
            PlayerControllerRefactored controller,
            PlayerModel model,
            PlayerMovement movement,
            PlayerControls controls)
        {
            Controller = controller;
            Model = model;
            Movement = movement;
            Controls = controls;
            Config = model.Config;
        }

        public virtual void Enter() { }
        public virtual void HandleInput(float deltaTime) { }
        public virtual void UpdatePhysics(float fixedDeltaTime) { }
        public virtual void Exit() { }

        protected void UpdateMoveDirectionFromInput()
        {
            float camEulerY = Controller.CameraTransform != null ? Controller.CameraTransform.eulerAngles.y : 0f;
            Vector3 rawDir = Movement.UpdateRotation(Controls.GetMoveInput(), camEulerY);
            Model.MoveDirection = Movement.PreventSteepWallClimbing(rawDir);
            Model.IsMoving = Model.MoveDirection.magnitude >= 0.1f;
        }

        protected bool TryTriggerDash()
        {
            if (Controls.dashBuffer > 0f && Model.CurrentDashesAvailable > 0)
            {
                Controls.dashBuffer = 0f;
                Controller.TransitionToState(PlayerState.Dashing);
                return true;
            }
            return false;
        }
    }

    public class WalkingState : PlayerStateBase
    {
        public WalkingState(PlayerControllerRefactored controller, PlayerModel model, PlayerMovement movement, PlayerControls controls)
            : base(controller, model, movement, controls) { }

        public override void HandleInput(float deltaTime)
        {
            if (Controls.WasWalkToggled())
            {
                Model.IsRunning = !Model.IsRunning;
            }

            UpdateMoveDirectionFromInput();

            if (TryTriggerDash()) return;

            if (Controls.slideBuffer > 0f && Model.IsGrounded && Movement.GetHorizontalSpeed() >= Config.minSlideSpeed)
            {
                Controls.slideBuffer = 0f;
                Controller.TransitionToState(PlayerState.Sliding);
                return;
            }

            if (Controls.jumpBuffer > 0f && (Model.CoyoteTimer > 0f || Model.IsGrounded))
            {
                Controls.jumpBuffer = 0f;
                Model.CoyoteTimer = 0f;
                Model.JumpCooldown = 0.2f;

                Movement.ExecuteJump(Config.jumpForce, false);
                Model.NotifyJump();
                Controller.TransitionToState(PlayerState.Airborne);
            }
        }

        public override void UpdatePhysics(float fixedDeltaTime)
        {
            float speed = Model.GetFinalSpeed();

            if (Movement.OnSlope())
            {
                Vector3 slopeDir = Movement.GetSlopeMoveDirection(Model.MoveDirection);
                Movement.SetVelocity(Model.MoveDirection == Vector3.zero ? Vector3.zero : slopeDir * speed);
            }
            else
            {
                Movement.SetVelocity(new Vector3(Model.MoveDirection.x * speed, Movement.GetVelocity().y, Model.MoveDirection.z * speed));
            }

            Movement.SnapToGround(Model.JumpCooldown);
        }
    }

    public class AirborneState : PlayerStateBase
    {
        public AirborneState(PlayerControllerRefactored controller, PlayerModel model, PlayerMovement movement, PlayerControls controls)
            : base(controller, model, movement, controls) { }

        public override void HandleInput(float deltaTime)
        {
            UpdateMoveDirectionFromInput();

            if (TryTriggerDash()) return;

            if (Controls.jumpBuffer > 0f && Model.AdditionalJumpsLeft > 0 && Model.JumpCooldown <= 0f)
            {
                Controls.jumpBuffer = 0f;
                Model.JumpCooldown = 0.2f;
                Model.AdditionalJumpsLeft--;

                Movement.ExecuteJump(Config.jumpForce, true);
                Model.NotifyJump();
            }
        }

        public override void UpdatePhysics(float fixedDeltaTime)
        {
            Vector3 vel = Movement.GetVelocity();
            Vector3 currentHorizontal = new Vector3(vel.x, 0f, vel.z);
            Vector3 targetHorizontal = Model.MoveDirection * Model.GetFinalSpeed();

            Vector3 newHorizontal = Vector3.Lerp(currentHorizontal, targetHorizontal, Config.airControl * fixedDeltaTime);
            Movement.SetVelocity(new Vector3(newHorizontal.x, vel.y, newHorizontal.z));

            vel = Movement.GetVelocity();
            if (vel.y < 0f)
            {
                Movement.SetVelocity(vel + Vector3.up * (Physics.gravity.y * (Config.fallMultiplier - 1f) * fixedDeltaTime));
            }
            else if (vel.y > 0f && !Controls.IsJumpPressed())
            {
                Movement.SetVelocity(vel + Vector3.up * (Physics.gravity.y * (Config.lowJumpMultiplier - 1f) * fixedDeltaTime));
            }
        }
    }

    public class SlidingState : PlayerStateBase
    {
        public SlidingState(PlayerControllerRefactored controller, PlayerModel model, PlayerMovement movement, PlayerControls controls)
            : base(controller, model, movement, controls) { }

        public override void Enter()
        {
            Movement.StartSlideHitbox();
            Model.SlideDirection = Movement.Transform.forward;
            Movement.ApplyForce(Model.SlideDirection * Config.slideBoost, ForceMode.Impulse);
            Model.NotifySlide(true);
        }

        public override void Exit()
        {
            Movement.StopSlideHitbox();
            Model.NotifySlide(false);
        }

        public override void HandleInput(float deltaTime)
        {
            if (TryTriggerDash()) return;

            if (Controls.jumpBuffer > 0f && (Model.CoyoteTimer > 0f || Model.IsGrounded))
            {
                Controls.jumpBuffer = 0f;
                Model.CoyoteTimer = 0f;
                Model.JumpCooldown = 0.2f;

                Controller.TransitionToState(PlayerState.Airborne);
                Movement.ExecuteJump(Config.jumpForce, false);
                Model.NotifyJump();
            }
        }

        public override void UpdatePhysics(float fixedDeltaTime)
        {
            float currentSlideSpeed = Movement.GetHorizontalSpeed();

            if (Movement.OnSlope())
            {
                Vector3 slopeDir = Movement.GetSlopeMoveDirection(Model.SlideDirection);
                currentSlideSpeed += (slopeDir.y < -0.1f) ? Config.slopeAcceleration * fixedDeltaTime : -Config.slideDrag * fixedDeltaTime;
                currentSlideSpeed = Mathf.Clamp(currentSlideSpeed, 0f, 25f);
                Movement.SetVelocity(slopeDir * currentSlideSpeed);
            }
            else
            {
                currentSlideSpeed -= Config.slideDrag * fixedDeltaTime;
                Movement.SetVelocity(new Vector3(Model.SlideDirection.x * currentSlideSpeed, Movement.GetVelocity().y, Model.SlideDirection.z * currentSlideSpeed));
            }

            Movement.SnapToGround(Model.JumpCooldown);

            if (currentSlideSpeed < Config.minSlideSpeed || !Controls.IsSlidePressed())
            {
                Controller.TransitionToState(Model.IsGrounded ? PlayerState.Walking : PlayerState.Airborne);
            }
        }
    }

    public class WallRidingState : PlayerStateBase
    {
        public WallRidingState(PlayerControllerRefactored controller, PlayerModel model, PlayerMovement movement, PlayerControls controls)
            : base(controller, model, movement, controls) { }

        public override void Enter()
        {
            Movement.SetVelocity(Vector3.zero);
            if (Model.LastWallHit.normal != Vector3.zero)
            {
                Movement.Transform.rotation = Quaternion.LookRotation(Model.LastWallHit.normal);
            }
        }

        public override void HandleInput(float deltaTime)
        {
            if (Controls.jumpBuffer > 0f)
            {
                Controls.jumpBuffer = 0f;
                Model.JumpCooldown = 0.2f;

                Controller.TransitionToState(PlayerState.Airborne);
                Vector3 jumpDir = (Movement.Transform.forward * Config.wallJumpSideForce) + (Vector3.up * Config.wallJumpUpForce);
                Movement.SetVelocity(Vector3.zero);
                Movement.ApplyForce(jumpDir, ForceMode.Impulse);
                Model.NotifyJump();
            }
        }

        public override void UpdatePhysics(float fixedDeltaTime)
        {
            if (!Movement.CheckWallBackward())
            {
                Controller.TransitionToState(PlayerState.Airborne);
                return;
            }

            Movement.SetVelocity(new Vector3(0f, Config.wallSlideSpeed, 0f));
            Movement.ApplyForce(-Movement.Transform.forward * 10f, ForceMode.Force);

            if (Model.IsGrounded)
            {
                Controller.TransitionToState(PlayerState.Walking);
            }
        }
    }

    public class DashingState : PlayerStateBase
    {
        public DashingState(PlayerControllerRefactored controller, PlayerModel model, PlayerMovement movement, PlayerControls controls)
            : base(controller, model, movement, controls) { }

        public override void Enter()
        {
            Model.DashTimeLeft = Config.dashDuration;
            Model.CurrentDashesAvailable--;

            if (Model.DashCooldownTimer <= 0f)
            {
                Model.SetDashCooldown(Config.dashCooldown);
            }

            Model.DashDirection = Movement.Transform.forward;
            Model.NotifyDash();
        }

        public override void Exit()
        {
            Movement.SetVelocity(Movement.GetVelocity().normalized * Model.GetFinalSpeed());
        }

        public override void HandleInput(float deltaTime)
        {
            Model.DashTimeLeft -= deltaTime;
            if (Model.DashTimeLeft <= 0f)
            {
                Controller.TransitionToState(Model.IsGrounded ? PlayerState.Walking : PlayerState.Airborne);
            }
        }

        public override void UpdatePhysics(float fixedDeltaTime)
        {
            Movement.SetVelocity(Model.DashDirection * Config.dashSpeed);
        }
    }
}
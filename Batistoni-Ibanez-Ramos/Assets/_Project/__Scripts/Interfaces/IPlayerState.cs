using UnityEngine;

namespace PatronesAplicados
{
    public interface IPlayerState
    {
        void Enter();
        void HandleInput(float deltaTime);
        void UpdatePhysics(float fixedDeltaTime);
        void Exit();
    }
}

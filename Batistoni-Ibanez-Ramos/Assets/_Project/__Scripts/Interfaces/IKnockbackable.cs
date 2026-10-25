using System.Numerics;

public interface IKnockbackable
{
    void ApplyKnockback(UnityEngine.Vector3 force, float stunDuration = 0.4f);
}
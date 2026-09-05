using UnityEngine;

public class ExplosionVisual : MonoBehaviour
{
    [Tooltip("Cuánto tiempo dura la esfera visible en pantalla antes de desaparecer")]
    public float lifetime = 1f;

    void Start()
    {
        // Se destruye automáticamente después de un instante
        Destroy(gameObject, lifetime);
    }
}
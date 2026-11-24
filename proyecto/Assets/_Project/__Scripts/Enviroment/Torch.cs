using UnityEngine;

public class AntorchaController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Light luzAntorcha;          // El componente Light
    [SerializeField] private GameObject fuegoParticulas;  // Opcional: El sistema de partículas del fuego

    [Header("Ajustes de Parpadeo (Flicker)")]
    [SerializeField] private bool hacerParpadeo = true;
    [SerializeField] private float intensidadMinima = 0.8f;
    [SerializeField] private float intensidadMaxima = 1.5f;
    [SerializeField] private float velocidadParpadeo = 0.05f; // Qué tan rápido cambia la luz

    [Header("Ajustes de Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoFuegoLoop;

    private float timer;

    void Start()
    {
        // Si no se asignó la luz, intenta buscarla en el mismo objeto
        if (luzAntorcha == null) luzAntorcha = GetComponent<Light>();
        
        // Configurar el audio en bucle para el crujido del fuego
        if (audioSource != null && sonidoFuegoLoop != null)
        {
            audioSource.clip = sonidoFuegoLoop;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (hacerParpadeo && luzAntorcha != null && luzAntorcha.enabled)
        {
            AplicarParpadeoEstructural();
        }
    }

    private void AplicarParpadeoEstructural()
    {
        timer += Time.deltaTime;

        if (timer >= velocidadParpadeo)
        {
            // Genera una intensidad aleatoria entre el mínimo y el máximo para simular el fuego
            luzAntorcha.intensity = Random.Range(intensidadMinima, intensidadMaxima);
            timer = 0f;
        }
    }

    // Función pública para apagar/encender la antorcha desde otros scripts (ej. si se moja)
    public void SetAntorchaActiva(bool estado)
    {
        if (luzAntorcha != null) luzAntorcha.enabled = estado;
        if (fuegoParticulas != null) fuegoParticulas.SetActive(estado);
        
        if (audioSource != null)
        {
            if (estado) audioSource.Play();
            else audioSource.Stop();
        }
    }
}
using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
    // Asegúrate de que este script sea un Singleton para acceder fácil desde otros lados
    public static MenuAudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    private void Awake()
    {
        // Configuración del Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional: mantiene la música si cambias de escena
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Configurar y reproducir la música de fondo al iniciar
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // Función para reproducir el sonido cuando el cursor pasa por encima del botón
    public void PlayHoverSound()
    {
        if (sfxSource != null && hoverSound != null)
        {
            sfxSource.PlayOneShot(hoverSound);
        }
    }

    // Función para reproducir el sonido cuando se hace clic en el botón
    public void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }
}
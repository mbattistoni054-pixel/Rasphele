using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Consola Maestra")]
    public AudioMixer mainMixer;

    [Header("Fuentes de Audio")]
    [Tooltip("Arrastra aquí el AudioSource que reproducirá la música")]
    public AudioSource musicSource;
    [Tooltip("Arrastra aquí el AudioSource que reproducirá los efectos")]
    public AudioSource sfxSource;

    [Header("Música de esta escena")]
    public AudioClip sceneMusic;

    [Header("Sliders de UI")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Diccionario de AudioMixerGroup y Nombre")]
    public Dictionary<string, AudioMixerGroup> mixerGroups = new Dictionary<string, AudioMixerGroup>();

    [Header("Audio Source Pool")]
    public int audioSourceMaxPool = 10;
    public List<AudioSource> poolAudioSources = new List<AudioSource>();

    private void Awake()
    {
        // Singleton para poder llamarlo desde cualquier script
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (AudioMixerGroup group in mainMixer.FindMatchingGroups(""))
        {
            if (!mixerGroups.ContainsKey(group.name))
            {
                mixerGroups.Add(group.name, group); 
                print(group.name);
            }
        }

        for (int i = 0; i < audioSourceMaxPool; i++)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            poolAudioSources.Add(newSource);
        }

    }

    private void Start()
    {
        // Cargamos el volumen guardado (o usamos 1 por defecto, que es el máximo)
        float savedMusicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFXVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Si los sliders existen, los ajustamos al volumen guardado
        if (musicSlider != null)
        {
            musicSlider.value = savedMusicVol;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFXVol;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        // Aplicamos el volumen real al AudioMixer (con retraso para evitar bugs de Unity)
        Invoke(nameof(ApplySavedVolumes), 0.1f);

        // Reproducimos la música de fondo de esta escena
        if (sceneMusic != null) PlayMusic(sceneMusic);
    }


    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, string groupName)
    {
        if (sfxSource == null || clip == null || groupName == null) return;

       if (mixerGroups.TryGetValue(groupName, out AudioMixerGroup group))
        {

            AudioSource freeSource = FindFreeSource();

            freeSource.outputAudioMixerGroup = group;
            freeSource.PlayOneShot(clip); // PlayOneShot permite que los sonidos se solapen (ej: muchos disparos)
        }
        else
        {
            Debug.LogWarning($"{groupName} no existe en el AudioMixer");
        }

    }


    private void ApplySavedVolumes()
    {
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
    }

    public void SetMusicVolume(float sliderValue)
    {
        // Evitamos el error de Log10(0) forzando un mínimo de 0.0001
        float val = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        float decibels = Mathf.Log10(val) * 20f;

        if (mainMixer != null) mainMixer.SetFloat("MusicVol", decibels);
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float val = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        float decibels = Mathf.Log10(val) * 20f;

        if (mainMixer != null) mainMixer.SetFloat("SFXVol", decibels);
        PlayerPrefs.SetFloat("SFXVolume", sliderValue);
    }

    private AudioSource FindFreeSource()
    {
        foreach (AudioSource source in poolAudioSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        print("Se estan usando todos los AudioSources");
        return poolAudioSources[0];  
    }

}
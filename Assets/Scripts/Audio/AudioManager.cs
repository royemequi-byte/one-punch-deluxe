using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

/// <summary>
/// Singleton que controla toda la música y efectos del juego.
/// No se destruye al cambiar de escena. Máximo un AudioManager en toda la partida.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Librería de clips")]
    [SerializeField] private AudioLibrary library;
    public AudioLibrary Library => library;

    [Header("Fuentes de audio")]
    [SerializeField] private AudioSource musicSource;

    // Pool de fuentes para efectos — evita que dos golpes simultáneos se corten
    private AudioSource[] sfxPool;
    private int sfxPoolIndex = 0;
    private const int POOL_SIZE = 8;

    private void Awake()
    {
        // Singleton: si ya existe uno, destruir el duplicado
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Asignar grupo del mixer a la fuente de música si está disponible
        if (musicSource != null && musicGroup != null)
            musicSource.outputAudioMixerGroup = musicGroup;

        CrearPoolDeSFX();
    }

    private void CrearPoolDeSFX()
    {
        sfxPool = new AudioSource[POOL_SIZE];
        for (int i = 0; i < POOL_SIZE; i++)
        {
            GameObject obj = new GameObject($"SFX_Pool_{i}");
            obj.transform.SetParent(transform);
            sfxPool[i] = obj.AddComponent<AudioSource>();
            sfxPool[i].playOnAwake = false;
            if (sfxGroup != null) sfxPool[i].outputAudioMixerGroup = sfxGroup;
        }
    }

    /// <summary>
    /// Reproduce música en loop. Hace fade entre pistas si ya había una sonando.
    /// </summary>
    public void PlayMusic(AudioClip clip, float fadeTime = 0.5f)
    {
        if (clip == null) return;
        StartCoroutine(FadeMusic(clip, fadeTime));
    }

    /// <summary>
    /// Reproduce un efecto de sonido desde el pool. No interrumpe otros SFX.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        // Avanzar al siguiente AudioSource del pool (circular)
        sfxPoolIndex = (sfxPoolIndex + 1) % POOL_SIZE;
        AudioSource source = sfxPool[sfxPoolIndex];
        source.clip = clip;
        source.Play();
    }

    /// <summary>
    /// Cambia el volumen de la música. Valor entre 0 y 1.
    /// Convierte a decibelios para que el slider se sienta lineal.
    /// </summary>
    public void SetMusicVolume(float valor)
    {
        if (mixer == null) return;
        mixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(valor, 0.0001f, 1f)) * 20f);
    }

    /// <summary>
    /// Cambia el volumen de los efectos. Valor entre 0 y 1.
    /// </summary>
    public void SetSFXVolume(float valor)
    {
        if (mixer == null) return;
        mixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(valor, 0.0001f, 1f)) * 20f);
    }

    private IEnumerator FadeMusic(AudioClip nuevoClip, float fadeTime)
    {
        // Bajar volumen del clip actual
        float tiempoInicial = Time.time;
        float volumenInicial = musicSource.volume;

        while (Time.time < tiempoInicial + fadeTime)
        {
            musicSource.volume = Mathf.Lerp(volumenInicial, 0f, (Time.time - tiempoInicial) / fadeTime);
            yield return null;
        }

        // Cambiar clip y subir volumen
        musicSource.clip = nuevoClip;
        musicSource.loop = true;
        musicSource.Play();

        tiempoInicial = Time.time;
        while (Time.time < tiempoInicial + fadeTime)
        {
            musicSource.volume = Mathf.Lerp(0f, volumenInicial, (Time.time - tiempoInicial) / fadeTime);
            yield return null;
        }

        musicSource.volume = volumenInicial;
    }
}

using UnityEngine;
using System.Collections;

/// <summary>
/// Singleton que controla toda la música y efectos del juego.
/// No se destruye al cambiar de escena. Máximo un AudioManager en toda la partida.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Librería de clips")]
    [SerializeField] private AudioLibrary library;
    public AudioLibrary Library => library;

    [Header("Fuentes de audio")]
    [SerializeField] private AudioSource musicSource;

    [Header("Volúmenes (0 a 1)")]
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.7f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

    // Pool de fuentes para efectos — evita que dos golpes simultáneos se corten
    private AudioSource[] sfxPool;
    private AudioSource ambienceSource;
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
        CrearPoolDeSFX();
    }

    /// <summary>Conecta la librería cuando el manager se crea por código.</summary>
    public void Initialize(AudioLibrary lib)
    {
        library = lib;

        if (musicSource == null)
        {
            GameObject go = new GameObject("MusicSource");
            go.transform.SetParent(transform, false);
            musicSource = go.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }

        if (sfxPool == null) CrearPoolDeSFX();

        if (ambienceSource == null)
        {
            GameObject amb = new GameObject("AmbienceSource");
            amb.transform.SetParent(transform, false);
            ambienceSource = amb.AddComponent<AudioSource>();
            ambienceSource.playOnAwake = false;
            ambienceSource.loop = true;
            ambienceSource.volume = 0f;
        }

        // Extras que viven en Resources para no tener que asignarlos
        if (library != null)
        {
            if (library.sfxPaso == null) library.sfxPaso = Resources.Load<AudioClip>("Audio/footstep");
            if (library.sfxSwing == null) library.sfxSwing = Resources.Load<AudioClip>("Audio/swing");
        }
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
            sfxPool[i].volume = sfxVolume;
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
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || sfxPool == null) return;

        // Avanzar al siguiente AudioSource del pool (circular)
        sfxPoolIndex = (sfxPoolIndex + 1) % POOL_SIZE;
        AudioSource source = sfxPool[sfxPoolIndex];
        source.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
        source.volume = sfxVolume * Mathf.Clamp01(volume);
        source.clip = clip;
        source.Play();
    }

    /// <summary>Ambiente de fondo (multitud, lluvia, etc.).</summary>
    public void PlayAmbience(AudioClip clip, float volume = 0.25f)
    {
        if (ambienceSource == null) return;
        if (clip == null) { StopAmbience(); return; }

        ambienceSource.clip = clip;
        ambienceSource.loop = true;
        ambienceSource.volume = Mathf.Clamp01(volume);
        ambienceSource.Play();
    }

    public void StopAmbience()
    {
        if (ambienceSource != null) ambienceSource.Stop();
    }

    /// <summary>Detiene la música actual.</summary>
    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    /// <summary>
    /// Cambia el volumen de la música. Valor entre 0 y 1.
    /// </summary>
    public void SetMusicVolume(float valor)
    {
        musicVolume = Mathf.Clamp01(valor);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    /// <summary>
    /// Cambia el volumen de los efectos. Valor entre 0 y 1.
    /// </summary>
    public void SetSFXVolume(float valor)
    {
        sfxVolume = Mathf.Clamp01(valor);
        foreach (AudioSource s in sfxPool)
            if (s != null) s.volume = sfxVolume;
    }

    private IEnumerator FadeMusic(AudioClip nuevoClip, float fadeTime)
    {
        // Bajar volumen del clip actual
        float tiempoInicial = Time.time;
        float volumenInicial = musicSource != null ? musicSource.volume : 0f;

        while (Time.time < tiempoInicial + fadeTime)
        {
            if (musicSource != null)
                musicSource.volume = Mathf.Lerp(volumenInicial, 0f, (Time.time - tiempoInicial) / fadeTime);
            yield return null;
        }

        // Cambiar clip y subir volumen
        if (musicSource != null)
        {
            musicSource.clip = nuevoClip;
            musicSource.loop = true;
            musicSource.Play();
        }

        tiempoInicial = Time.time;
        while (Time.time < tiempoInicial + fadeTime)
        {
            if (musicSource != null)
                musicSource.volume = Mathf.Lerp(0f, musicVolume, (Time.time - tiempoInicial) / fadeTime);
            yield return null;
        }

        if (musicSource != null) musicSource.volume = musicVolume;
    }
}

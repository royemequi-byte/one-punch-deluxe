using UnityEngine;

/// <summary>
/// Contiene todos los clips de audio del juego referenciados por nombre.
/// Evita buscar archivos por ruta mágica en el código.
/// Asignar los clips desde el Inspector una sola vez.
/// </summary>
[CreateAssetMenu(fileName = "AudioLibrary", menuName = "OnePunchDeluxe/Audio Library")]
public class AudioLibrary : ScriptableObject
{
    [Header("Música")]
    public AudioClip musicaCombate;
    public AudioClip musicaVictoria;
    public AudioClip musicaMenu;

    [Header("SFX — Golpes")]
    public AudioClip sfxJab;
    public AudioClip sfxStraight;
    public AudioClip sfxHook;

    [Header("SFX — Impactos")]
    public AudioClip sfxImpacto;
    public AudioClip sfxBloqueo;
    public AudioClip sfxWhiff;

    [Header("SFX — Combate")]
    public AudioClip sfxDanioRecibido;
    public AudioClip sfxCaida;
    public AudioClip sfxCampana;

    [Header("SFX — UI")]
    public AudioClip sfxBotonClick;
    public AudioClip sfxBotonHover;
}

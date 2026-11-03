using UnityEngine;

/// <summary>
/// Voces del locutor del torneo. Los archivos viven en Assets/Resources/Voice,
/// así que se cargan por nombre y no hay que asignar nada en el Inspector.
/// Si un clip falta, el juego sigue en silencio sin romperse.
/// </summary>
public static class AnnouncerVoice
{
    private static AudioClip ko, gameOver, fight, youWin, timeUp, maximum;
    private static readonly AudioClip[] rounds = new AudioClip[3];

    public static AudioClip KO        => Cargar(ref ko,       "Voice/ko");
    public static AudioClip GameOver  => Cargar(ref gameOver, "Voice/gameover");
    public static AudioClip Fight     => Cargar(ref fight,    "Voice/fight");
    public static AudioClip YouWin    => Cargar(ref youWin,   "Voice/youwin");
    public static AudioClip TimeUp    => Cargar(ref timeUp,   "Voice/time");
    public static AudioClip Maximum   => Cargar(ref maximum,  "Voice/maximum");

    public static AudioClip Round(int numero)
    {
        int i = Mathf.Clamp(numero, 1, 3) - 1;
        if (rounds[i] == null) rounds[i] = Resources.Load<AudioClip>("Voice/round" + (i + 1));
        return rounds[i];
    }

    /// <summary>Reproduce una voz del locutor con el AudioManager del juego.</summary>
    public static void Hablar(AudioClip clip, float volumen = 0.95f)
    {
        if (clip == null) return;

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(clip, volumen);
        else AudioSource.PlayClipAtPoint(clip, Vector3.zero, volumen);
    }

    private static AudioClip Cargar(ref AudioClip cache, string ruta)
    {
        if (cache == null) cache = Resources.Load<AudioClip>(ruta);
        return cache;
    }
}

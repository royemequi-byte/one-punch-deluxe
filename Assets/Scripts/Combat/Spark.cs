using UnityEngine;

/// <summary>
/// Vida de una chispa: crece y se desvanece en menos de un cuarto de segundo.
/// La crea CombatFX y se destruye sola al terminar.
/// </summary>
public class Spark : MonoBehaviour
{
    private const float Life = 0.18f;

    private float age;
    private SpriteRenderer sr;
    private Vector3 startScale;
    private float startAlpha = 1f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
        startAlpha = sr.color.a;
    }

    private void Update()
    {
        age += Time.deltaTime;
        float k = Mathf.Clamp01(age / Life);

        transform.localScale = startScale * (1f + k * 0.9f);

        Color c = sr.color;
        c.a = startAlpha * (1f - k);
        sr.color = c;

        if (k >= 1f) Destroy(gameObject);
    }
}

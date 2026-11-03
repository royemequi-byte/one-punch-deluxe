using UnityEngine;

/// <summary>
/// Efectos de impacto sin necesitar arte: genera una chispa procedural
/// (un destello radial) cada vez que un golpe conecta. Se destruye sola.
/// </summary>
public static class CombatFX
{
    private static Sprite sparkSprite;

    /// <summary>Textura radial blanca. Sirve para chispas y para auras.</summary>
    public static Sprite Glow => GetSparkSprite();

    /// <summary>Crea una chispa en el punto de impacto con el color indicado.</summary>
    public static void Spark(Vector3 position, Color color, float size)
    {
        GameObject go = new GameObject("_Spark");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * size;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSparkSprite();
        sr.color = color;
        sr.sortingOrder = 30;

        go.AddComponent<Spark>();
    }

    /// <summary>Nube de polvo baja, para los pasos y los aterrizajes.</summary>
    public static void Dust(Vector3 position)
    {
        GameObject go = new GameObject("_Dust");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.7f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSparkSprite();
        sr.color = new Color(0.78f, 0.76f, 0.72f, 0.35f);
        sr.sortingOrder = 0;

        go.AddComponent<Spark>();
    }

    // Se genera una sola vez y se reutiliza en todas las chispas
    private static Sprite GetSparkSprite()
    {
        if (sparkSprite != null) return sparkSprite;

        const int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float center = (size - 1) * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(1f - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha));
            }
        }

        tex.Apply();
        sparkSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return sparkSprite;
    }
}

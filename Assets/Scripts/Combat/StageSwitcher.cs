using UnityEngine;

/// <summary>
/// Cambia el escenario de combate por la versión lateral del MISMO ring
/// (Resources/Profile/ring_side). El ring original de la historia no se toca.
/// También deja la composición lateral: Wally izquierda, Kirikú derecha.
/// </summary>
public class StageSwitcher : MonoBehaviour
{
    private void Start()
    {
        Sprite ringSprite = Resources.Load<Sprite>("Profile/ring_side");

        if (ringSprite == null)
        {
            Texture2D tex = Resources.Load<Texture2D>("Profile/ring_side");
            if (tex != null)
            {
                ringSprite = Sprite.Create(
                    tex,
                    new Rect(0f, 0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
            }
        }

        GameObject ring = GameObject.Find("BackgroundRing");
        if (ring != null)
        {
            SpriteRenderer sr = ring.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
                sr.sortingOrder = -10;
                if (ringSprite != null) sr.sprite = ringSprite;
            }

            ring.transform.position = new Vector3(0f, 0f, 10f);
            ring.transform.localScale = new Vector3(1.064f, 1.064f, 1f);
        }

        // Composición lateral. Kirikú usa escala negativa porque su arte mira a la derecha.
        GameObject wally = GameObject.Find("Wally");
        if (wally != null)
        {
            wally.transform.position = new Vector3(-2.2f, -3f, 10f);
            wally.transform.localScale = new Vector3(3f, 3f, 1f);
        }

        GameObject rival = GameObject.Find("Rival");
        if (rival != null)
        {
            rival.transform.position = new Vector3(2.2f, -3f, 10f);
            rival.transform.localScale = new Vector3(-3f, 3f, 1f);
        }
    }
}

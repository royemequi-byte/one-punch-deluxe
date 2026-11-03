using UnityEngine;

/// <summary>
/// Aura dorada que aparece cuando el medidor MAXIMUM está lleno.
/// Es procedural: un resplandor radial que late detrás del peleador.
/// </summary>
public class FighterAura : MonoBehaviour
{
    private PowerMeter power;
    private SpriteRenderer source;
    private SpriteRenderer aura;
    private Transform auraT;
    private float pulse;

    private void Awake()
    {
        power = GetComponent<PowerMeter>();
        source = GetComponent<SpriteRenderer>();

        if (source == null) return;

        GameObject go = new GameObject("MaximumAura");
        go.transform.SetParent(transform, false);

        aura = go.AddComponent<SpriteRenderer>();
        aura.sprite = CombatFX.Glow;
        aura.color = new Color(1f, 0.82f, 0.15f, 0f);
        aura.sortingLayerID = source.sortingLayerID;
        aura.sortingOrder = source.sortingOrder - 3;
        aura.enabled = false;
        auraT = go.transform;
    }

    private void LateUpdate()
    {
        if (aura == null || power == null) return;

        bool full = power.IsFull;
        aura.enabled = full;
        if (!full) return;

        pulse += Time.deltaTime * 4.5f;
        float wave = 0.5f + 0.5f * Mathf.Sin(pulse);

        aura.color = new Color(1f, 0.82f, 0.15f, 0.18f + wave * 0.22f);

        float scale = 2.0f + wave * 0.25f;
        auraT.localScale = new Vector3(scale, scale, 1f);
        auraT.localPosition = new Vector3(0f, 1.1f, 0f);
    }
}

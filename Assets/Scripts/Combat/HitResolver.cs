using UnityEngine;

/// <summary>
/// Decide si un golpe impacta, cuánto daño hace y qué efectos produce.
/// Está aparte para que el CombatController se quede corto y fácil de explicar.
/// El impacto se mide por distancia en X, como en los juegos de pelea 2D.
/// </summary>
public static class HitResolver
{
    /// <summary>
    /// Devuelve true si el golpe conectó (con o sin bloqueo).
    /// Aplica daño, stun, empuje, chispa y sacudida de cámara.
    /// </summary>
    public static bool Resolve(
        AttackData attack,
        Transform attacker,
        Health targetHealth,
        CombatController targetCombat,
        FighterBody attackerBody,
        float blockMultiplier)
    {
        if (attack == null || targetHealth == null || targetHealth.IsDead) return false;

        AudioManager audio = AudioManager.Instance;
        AudioLibrary lib = audio != null ? audio.Library : null;

        Vector3 targetPos = targetHealth.transform.position;
        float distance = Mathf.Abs(targetPos.x - attacker.position.x);

        if (distance > attack.reach || (targetCombat != null && targetCombat.IsInvulnerable))
        {
            audio?.PlaySFX(lib?.sfxWhiff, 0.75f, Random.Range(0.95f, 1.05f));
            return false;
        }

        bool blocked = targetCombat != null && targetCombat.IsBlocking;
        int damage = blocked
            ? Mathf.Max(1, Mathf.RoundToInt(attack.damage * blockMultiplier))
            : attack.damage;

        targetHealth.TakeDamage(damage);

        Vector3 sparkPos = new Vector3(targetPos.x, targetPos.y + 1.7f, 0f);
        float dir = Mathf.Sign(targetPos.x - attacker.position.x);
        if (dir == 0f) dir = 1f;

        if (blocked)
        {
            targetCombat.ApplyBlockstun(attack.blockstun);
            CombatFX.Spark(sparkPos, new Color(0.35f, 0.75f, 1f), 1.0f);
            audio?.PlaySFX(lib?.sfxBloqueo, 0.9f, Random.Range(0.97f, 1.03f));
        }
        else
        {
            targetCombat?.ApplyHitstun(attack.hitstun);
            CombatFX.Spark(sparkPos, new Color(1f, 0.85f, 0.2f), 1.3f);
            VisualJuice.Instance?.ZoomPunch(0.1f);

            AudioClip impact = attack.hitSound != null ? attack.hitSound : lib?.sfxImpacto;
            audio?.PlaySFX(impact, 1f, Random.Range(0.95f, 1.05f));
            audio?.PlaySFX(lib?.sfxDanioRecibido, 0.35f, Random.Range(0.95f, 1.05f));
        }

        FighterBody targetBody = targetCombat != null ? targetCombat.GetComponent<FighterBody>() : null;
        float push = blocked ? attack.knockback * 0.4f : attack.knockback;
        targetBody?.Push(dir * push);
        attackerBody?.Push(-dir * push * 0.35f);

        return true;
    }
}

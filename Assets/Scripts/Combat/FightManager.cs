using UnityEngine;

/// <summary>
/// Controla el flujo de la pelea: detecta KO y declara ganador.
/// Es el árbitro — escucha los eventos de vida y decide cuándo termina la pelea.
/// </summary>
public class FightManager : MonoBehaviour
{
    [Header("Peleadores")]
    [SerializeField] private Health wallyHealth;
    [SerializeField] private Health rivalHealth;
    [SerializeField] private CombatController wallyCombat;
    [SerializeField] private CombatController rivalCombat;

    // Evita que el KO se dispare dos veces
    private bool fightOver = false;

    private void Start()
    {
        IniciarPelea();
    }

    private void IniciarPelea()
    {
        fightOver = false;

        // Restaurar vida y stamina al máximo
        wallyHealth.ResetHealth();
        rivalHealth.ResetHealth();

        // Suscribirse a los eventos de muerte de cada peleador
        wallyHealth.OnDeath += () => TerminarPelea(ganadorEsWally: false);
        rivalHealth.OnDeath += () => TerminarPelea(ganadorEsWally: true);

        Debug.Log("Pelea iniciada");
    }

    /// <summary>
    /// Se llama cuando un peleador llega a cero vida.
    /// Por ahora muestra el resultado en Console — la pantalla de resultado viene en Semana 5.
    /// </summary>
    private void TerminarPelea(bool ganadorEsWally)
    {
        if (fightOver) return;
        fightOver = true;

        if (ganadorEsWally)
            Debug.Log("KO! WALLY GANA!");
        else
            Debug.Log("KO! WALLY PIERDE!");
    }

    private void OnDestroy()
    {
        // Desuscribirse para evitar fugas de memoria
        if (wallyHealth != null)
            wallyHealth.OnDeath -= () => TerminarPelea(ganadorEsWally: false);
        if (rivalHealth != null)
            rivalHealth.OnDeath -= () => TerminarPelea(ganadorEsWally: true);
    }
}

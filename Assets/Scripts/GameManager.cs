using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum TipoBasura { Organica, Plastico, Carton, Ninguna }

    [Header("Configuración de Audio")]
    public AudioClip clipVictoria;
    public AudioClip clipDerrota;

    private AudioSource gameAudioSource;

    // --- CONFIGURACIÓN DE NIVEL 2 (SECUENCIA) ---
    [Header("Control de Niveles")]
    [Tooltip("Marcar si estamos en el Nivel 2 o superior (Activa la secuencia obligatoria).")]
    public bool isLevel2 = false;

    // ⭐ NUEVO: FLAG PARA NIVEL 3 (Se activa automáticamente en Start)
    private bool isLevel3 = false;

    [Header("Configuración Nivel 2: Secuencia")]
    [Tooltip("La secuencia de basura que el jugador debe recoger y depositar.")]
    public TipoBasura[] secuenciaRequerida = { TipoBasura.Organica, TipoBasura.Plastico, TipoBasura.Carton };

    private int indiceSecuenciaActual = 0;
    private bool tieneBasuraEnMano = false;

    // INVENTARIO DEL JUGADOR (Necesario para controlar Nivel 2)
    [Header("Inventario Actual")]
    public int invOrganico = 0;
    public int invPlastico = 0;
    public int invCarton = 0;

    [Header("UI y Paneles")]
    public GameObject panelGanador;
    public GameObject panelPerdedor;

    public TextMeshProUGUI textoTemporizador;
    public TextMeshProUGUI textoProgresoOrganica;
    public TextMeshProUGUI textoProgresoPlastico;
    public TextMeshProUGUI textoProgresoCarton;

    [Header("Configuración de Lógica")]
    public float tiempoLimite = 180f;
    private float tiempoRestante;
    private bool juegoTerminado = false;

    private const int REQUERIMIENTO = 4;
    private int organicaCount = 0;
    private int plasticoCount = 0;
    private int cartonCount = 0;

    // ⭐ NUEVO: CONTADOR Y REQUERIMIENTO PARA NIVEL 3
    private int nivel3BasuraCount = 0;
    private const int REQUERIMIENTO_NIVEL3 = 5;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // --- INICIALIZACIÓN DE COMPONENTES ---
        gameAudioSource = GetComponent<AudioSource>();
        if (gameAudioSource == null)
        {
            Debug.LogWarning("GameManager no encontró un AudioSource. Se añade uno.");
            gameAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // --- ⭐ LÓGICA NIVEL 3: DETECCIÓN DE ESCENA ---
        // Comprueba si la escena se llama "Nivel003" para activar la lógica especial.
        if (SceneManager.GetActiveScene().name == "Nivel003")
        {
            isLevel3 = true;
        }
        else
        {
            isLevel3 = false;
        }

        // --- REINICIO DE ESTADO DEL JUEGO ---
        tiempoRestante = tiempoLimite;
        Time.timeScale = 1;
        juegoTerminado = false;

        // Reiniciar contadores
        organicaCount = 0;
        plasticoCount = 0;
        cartonCount = 0;
        nivel3BasuraCount = 0; // Reiniciar contador de Nivel 3

        // Reiniciar Inventario 
        invOrganico = 0;
        invPlastico = 0;
        invCarton = 0;

        indiceSecuenciaActual = 0;
        tieneBasuraEnMano = false;

        if (panelGanador != null) panelGanador.SetActive(false);
        if (panelPerdedor != null) panelPerdedor.SetActive(false);

        ActualizarProgresoUI();
    }

    void Update()
    {
        if (juegoTerminado) return;

        tiempoRestante -= Time.deltaTime;

        // Actualizar Temporizador UI
        if (textoTemporizador != null)
        {
            string minutos = Mathf.Floor(tiempoRestante / 60).ToString("00");
            string segundos = Mathf.Floor(tiempoRestante % 60).ToString("00");
            textoTemporizador.text = minutos + ":" + segundos;
        }

        // CONDICIÓN DE DERROTA POR TIEMPO
        if (tiempoRestante <= 0)
        {
            tiempoRestante = 0;
            TerminarJuego(false);
        }
    }

    // =================================================================
    //	⭐ FUNCIÓN DE PENALIZACIÓN DE TIEMPO (Llamada por la Piedra) 💥
    // =================================================================

    public void RestarTiempo(float cantidad)
    {
        if (juegoTerminado) return;

        tiempoRestante -= cantidad;

        if (tiempoRestante <= 0)
        {
            tiempoRestante = 0;
            TerminarJuego(false);
        }
    }

    // =================================================================
    //	FUNCIÓN DE RECOLECCIÓN (Llamada desde MovimientoJugador)
    // =================================================================

    public bool TryCollectTrash(TipoBasura tipo)
    {
        if (juegoTerminado) return false;

        // --- LÓGICA NIVEL 3 (Prioridad): Recoger = Contar directamente ---
        // En el Nivel 3 se cuenta al recoger (no al depositar).
        if (isLevel3)
        {
            RecolectarBasura(tipo);
            return true;
        }

        // --- LÓGICA NIVEL 1 (Sencilla: Recoger = Contar) ---
        if (!isLevel2)
        {
            RecolectarBasura(tipo);
            return true;
        }

        // --- LÓGICA NIVEL 2 (Secuencial: Recoger = Inventario) ---
        if (tieneBasuraEnMano)
        {
            return false;
        }

        TipoBasura tipoEsperado = secuenciaRequerida[indiceSecuenciaActual];

        if (tipo == tipoEsperado)
        {
            IncrementarInventario(tipo);
            tieneBasuraEnMano = true;
            return true;
        }

        return false;
    }

    // =================================================================
    //	FUNCIÓN DE DEPÓSITO 
    // =================================================================

    public void DepositarBasura(TipoBasura tipoDeposito)
    {
        if (juegoTerminado) return;

        // Nivel 3 solo cuenta al recoger.
        if (isLevel3) return;

        // Nivel 1 no usa esta lógica de inventario.
        if (!isLevel2) return;

        // Lógica de Depósito de Nivel 2:
        if (!tieneBasuraEnMano)
        {
            return;
        }

        TipoBasura tipoEsperado = secuenciaRequerida[indiceSecuenciaActual];

        if (tipoDeposito == tipoEsperado)
        {
            if (DecrementarInventario(tipoDeposito))
            {
                RecolectarBasura(tipoDeposito);
                tieneBasuraEnMano = false;
                indiceSecuenciaActual++;
            }
        }
    }


    // =================================================================
    //	FUNCIONES DE INVENTARIO PARA NIVEL 2
    // =================================================================

    private void IncrementarInventario(TipoBasura tipo)
    {
        switch (tipo)
        {
            case TipoBasura.Organica: invOrganico++; break;
            case TipoBasura.Plastico: invPlastico++; break;
            case TipoBasura.Carton: invCarton++; break;
        }
    }

    private bool DecrementarInventario(TipoBasura tipo)
    {
        switch (tipo)
        {
            case TipoBasura.Organica:
                if (invOrganico > 0) { invOrganico--; return true; }
                break;
            case TipoBasura.Plastico:
                if (invPlastico > 0) { invPlastico--; return true; }
                break;
            case TipoBasura.Carton:
                if (invCarton > 0) { invCarton--; return true; }
                break;
        }
        return false;
    }

    // =================================================================
    //	FUNCIONES BASE DE CONTEO Y UI
    // =================================================================

    // Esta función incrementa los contadores finales.
    public void RecolectarBasura(TipoBasura tipo)
    {
        if (juegoTerminado) return;

        // Lógica de conteo de TOTAL para el Nivel 3 (solo si es Nivel 3)
        if (isLevel3)
        {
            nivel3BasuraCount++; // Seguimos contando el total para la condición de victoria
        }

        // Lógica de conteo por tipo (Aplica para todos los niveles para llevar registro individual)
        // **IMPORTANTE:** Se eliminó la condición 'if (!isLevel3)' para que los contadores 
        // individuales se actualicen en Nivel 3, permitiendo mostrar el progreso por tipo.
        switch (tipo)
        {
            case TipoBasura.Organica:
                organicaCount++;
                break;
            case TipoBasura.Plastico:
                plasticoCount++;
                break;
            case TipoBasura.Carton:
                cartonCount++;
                break;
        }

        ActualizarProgresoUI();
        CheckVictory();
    }

    private void ActualizarProgresoUI()
    {
        // ⭐ LÓGICA NIVEL 3: Muestra el progreso INDIVIDUAL sobre el REQUERIMIENTO TOTAL
        if (isLevel3)
        {
            // Ahora mostramos el conteo individual sobre el requerimiento total de Nivel 3.
            if (textoProgresoOrganica != null) textoProgresoOrganica.text = organicaCount + "/" + REQUERIMIENTO_NIVEL3;
            if (textoProgresoPlastico != null) textoProgresoPlastico.text = plasticoCount + "/" + REQUERIMIENTO_NIVEL3;
            if (textoProgresoCarton != null) textoProgresoCarton.text = cartonCount + "/" + REQUERIMIENTO_NIVEL3;
        }
        else
        {
            // Lógica original para Niveles 1/2: Muestra el conteo por tipo
            if (textoProgresoOrganica != null) textoProgresoOrganica.text = organicaCount + "/" + REQUERIMIENTO;
            if (textoProgresoPlastico != null) textoProgresoPlastico.text = plasticoCount + "/" + REQUERIMIENTO;
            if (textoProgresoCarton != null) textoProgresoCarton.text = cartonCount + "/" + REQUERIMIENTO;
        }
    }

    private void CheckVictory()
    {
        // CONDICIÓN DE VICTORIA BASE (Niveles 1 y 2)
        bool victoriaBase = (organicaCount >= REQUERIMIENTO &&
                             plasticoCount >= REQUERIMIENTO &&
                             cartonCount >= REQUERIMIENTO);

        // ⭐ CONDICIÓN DE VICTORIA NIVEL 3 (5 ítems en total)
        bool victoriaNivel3 = isLevel3 && (nivel3BasuraCount >= REQUERIMIENTO_NIVEL3);

        if (victoriaBase || victoriaNivel3)
        {
            TerminarJuego(true);
        }
    }

    public void TerminarJuego(bool gano)
    {
        if (juegoTerminado) return;

        juegoTerminado = true;
        Time.timeScale = 0; // Pausa el juego

        // --- LÓGICA DE AUDIO DE VICTORIA/DERROTA ---
        if (gano)
        {
            if (clipVictoria != null && gameAudioSource != null)
            {
                gameAudioSource.PlayOneShot(clipVictoria);
            }
            if (panelGanador != null) panelGanador.SetActive(true);
        }
        else
        {
            if (clipDerrota != null && gameAudioSource != null)
            {
                gameAudioSource.PlayOneShot(clipDerrota);
            }
            if (panelPerdedor != null) panelPerdedor.SetActive(true);
        }
    }

    public void CargarSiguienteNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Time.timeScale = 1;
    }

    public void ReintentarNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;
    }

    public void VolverAlMenu()
    {
        SceneManager.LoadScene("MenuNiveles");
        Time.timeScale = 1;
    }
}
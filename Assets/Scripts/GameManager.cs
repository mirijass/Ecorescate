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

    // Variable para el Nivel 3
    private bool isLevel3 = false;

    [Header("Configuración Nivel 2: Secuencia")]
    [Tooltip("La secuencia de basura que el jugador debe recoger y depositar.")]
    public TipoBasura[] secuenciaRequerida = { TipoBasura.Organica, TipoBasura.Plastico, TipoBasura.Carton };

    private int indiceSecuenciaActual = 0;
    private bool tieneBasuraEnMano = false;

    // INVENTARIO DEL JUGADOR
    [Header("Inventario Actual")]
    public int invOrganico = 0;
    public int invPlastico = 0;
    public int invCarton = 0;

    [Header("UI y Paneles")]
    public GameObject panelGanador;
    public GameObject panelPerdedor;
    
    [Header("Control de Pausa")]
    public GameObject panelPausa; // Campo para enlazar el panel de pausa

    public TextMeshProUGUI textoTemporizador;
    public TextMeshProUGUI textoProgresoOrganica;
    public TextMeshProUGUI textoProgresoPlastico;
    public TextMeshProUGUI textoProgresoCarton;

    [Header("Configuración de Lógica")]
    public float tiempoLimite = 180f;
    private float tiempoRestante;
    private bool juegoTerminado = false;

    // REQUERIMIENTOS 
    private const int REQUERIMIENTO = 4; // Requerimiento Nivel 1 y 2
    private const int REQUERIMIENTO_NIVEL3_INDIVIDUAL = 5; // Requerimiento Nivel 3 (5 de cada uno)

    private int organicaCount = 0;
    private int plasticoCount = 0;
    private int cartonCount = 0;


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

        // --- LÓGICA NIVEL 3: DETECCIÓN DE ESCENA ---
        if (SceneManager.GetActiveScene().name == "Nivel003")
        {
            isLevel3 = true;
            isLevel2 = false; // Desactivamos la lógica secuencial para Nivel 3
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

        // Reiniciar Inventario 
        invOrganico = 0;
        invPlastico = 0;
        invCarton = 0;

        indiceSecuenciaActual = 0;
        tieneBasuraEnMano = false;

        if (panelGanador != null) panelGanador.SetActive(false);
        if (panelPerdedor != null) panelPerdedor.SetActive(false);
        if (panelPausa != null) panelPausa.SetActive(false); // Asegurar que el Panel de Pausa esté oculto

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
        
        // Pausa con la tecla ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0f)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // =================================================================
    //	FUNCIONES DE PAUSA Y REANUDACIÓN ⏸️
    // =================================================================

    public void PauseGame()
    {
        if (juegoTerminado) return; 

        Time.timeScale = 0f; // Detiene el tiempo
        if (panelPausa != null) panelPausa.SetActive(true); // Muestra el panel
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; // Reanuda el tiempo
        if (panelPausa != null) panelPausa.SetActive(false); // Oculta el panel
    }


    // =================================================================
    //	FUNCIÓN DE PENALIZACIÓN DE TIEMPO (Llamada por la Piedra) 💥
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

        // Lógica de recolección para Nivel 1 y 3 (Solo al inventario, NO cuenta)
        if (!isLevel2)
        {
            IncrementarInventario(tipo);
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
    //	FUNCIÓN DE DEPÓSITO 🚮
    // =================================================================

    public void DepositarBasura(TipoBasura tipoDeposito)
    {
        if (juegoTerminado) return;

        // Lógica de depósito para Nivel 1 y 3 (Se cuenta solo si se deposita correctamente)
        if (isLevel3 || !isLevel2)
        {
            // Si tiene el ítem en inventario, lo decrementa y lo cuenta.
            if (DecrementarInventario(tipoDeposito))
            {
                RecolectarBasura(tipoDeposito);
            }
            return;
        }

        // Lógica de Depósito de Nivel 2 (Secuencial):
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
    //	FUNCIONES DE INVENTARIO
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

    public void RecolectarBasura(TipoBasura tipo)
    {
        if (juegoTerminado) return;
        
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
        // Usa el requerimiento individual del Nivel 3 si aplica
        int requerimientoActual = isLevel3 ? REQUERIMIENTO_NIVEL3_INDIVIDUAL : REQUERIMIENTO;

        if (textoProgresoOrganica != null) textoProgresoOrganica.text = organicaCount + "/" + requerimientoActual;
        if (textoProgresoPlastico != null) textoProgresoPlastico.text = plasticoCount + "/" + requerimientoActual;
        if (textoProgresoCarton != null) textoProgresoCarton.text = cartonCount + "/" + requerimientoActual;
    }

    private void CheckVictory()
    {
        // Determinamos el requerimiento
        int requerimientoVictoria = isLevel3 ? REQUERIMIENTO_NIVEL3_INDIVIDUAL : REQUERIMIENTO;

        // Condición de victoria: alcanzar el requerimiento en cada tipo
        bool victoriaTotal = (organicaCount >= requerimientoVictoria &&
                             plasticoCount >= requerimientoVictoria &&
                             cartonCount >= requerimientoVictoria);

        if (victoriaTotal)
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
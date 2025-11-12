using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    // 1. DEFINICIÓN DE LA ENUMERACIÓN
    // Otros scripts (Basura.cs, BoteBasura.cs) deben referenciarla como GameManager.TipoBasura
    public enum TipoBasura { Organica, Plastico, Carton }

    // UI Y PANELES (¡Asignar en Inspector!)
    // --------------------------------------------------------------------------------
    public GameObject panelGanador;
    public GameObject panelPerdedor;
    

    // Opcional: Textos para mostrar el progreso (ej. 1/4)
    public TextMeshProUGUI textoTemporizador;
    public TextMeshProUGUI textoProgresoOrganica;
    public TextMeshProUGUI textoProgresoPlastico;
    public TextMeshProUGUI textoProgresoCarton;

    // CONTROL DE JUEGO Y TIEMPO
    // --------------------------------------------------------------------------------
    public float tiempoLimite = 180f; // 3 minutos = 180 segundos
    private float tiempoRestante;
    private bool juegoTerminado = false;

    // CONTADORES ESPECÍFICOS (4 de cada uno es el requerimiento)
    private const int REQUERIMIENTO = 4;
    private int organicaCount = 0;
    private int plasticoCount = 0;
    private int cartonCount = 0;

    void Start()
    {
        tiempoRestante = tiempoLimite;
        // Asegura que el juego NO esté pausado y que la bandera esté en falso
        Time.timeScale = 1;
        juegoTerminado = false;

        // Ocultar paneles al inicio
        if (panelGanador != null) panelGanador.SetActive(false);
        if (panelPerdedor != null) panelPerdedor.SetActive(false);

        ActualizarProgresoUI();
    }

    void Update()
    {
        if (juegoTerminado) return; // Detiene toda la lógica si el juego terminó

        tiempoRestante -= Time.deltaTime;

        // Formateo de tiempo a Minutos:Segundos
        if (textoTemporizador != null)
        {
            string minutos = Mathf.Floor(tiempoRestante / 60).ToString("00");
            string segundos = Mathf.Floor(tiempoRestante % 60).ToString("00");
            textoTemporizador.text = minutos + ":" + segundos;
        }

        // Condición de Derrota por Tiempo
        if (tiempoRestante <= 0)
        {
            tiempoRestante = 0;
            TerminarJuego(false); // Derrota
        }
    }

    // MÉTODO LLAMADO POR Basura.cs o BoteBasura.cs
    public void RecolectarBasura(TipoBasura tipo)
    {
        if (juegoTerminado) return;

        // Suma al contador correspondiente
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
        CheckVictory(); // Verificar la victoria después de cada recolección
    }

    private void ActualizarProgresoUI()
    {
        // Actualiza los textos si existen
        if (textoProgresoOrganica != null) textoProgresoOrganica.text = organicaCount + "/" + REQUERIMIENTO;
        if (textoProgresoPlastico != null) textoProgresoPlastico.text = plasticoCount + "/" + REQUERIMIENTO;
        if (textoProgresoCarton != null) textoProgresoCarton.text = cartonCount + "/" + REQUERIMIENTO;
    }

    private void CheckVictory()
    {
        // CONDICIÓN DE VICTORIA: Se requieren 4/4 en CADA categoría
        if (organicaCount >= REQUERIMIENTO && plasticoCount >= REQUERIMIENTO && cartonCount >= REQUERIMIENTO)
        {
            TerminarJuego(true); // ¡Victoria!
        }
    }

    // MÉTODOS DE FINALIZACIÓN Y PROGRESIÓN
    // --------------------------------------------------------------------------------

    public void TerminarJuego(bool gano)
    {
        if (juegoTerminado) return;

        juegoTerminado = true;
        Time.timeScale = 0; // Pausa el juego
        Debug.Log("Juego Terminado. Ganó: " + gano);

        if (gano)
        {
            panelGanador.SetActive(true);
            DesbloquearSiguienteNivel();
        }
        else
        {
            panelPerdedor.SetActive(true); // Derrota por tiempo
        }
    }

    private void DesbloquearSiguienteNivel()
    {
        int nivelActualBuildIndex = SceneManager.GetActiveScene().buildIndex;
        int siguienteNivel = nivelActualBuildIndex + 1;

        if (siguienteNivel > PlayerPrefs.GetInt("nivelDesbloqueado"))
        {
            PlayerPrefs.SetInt("nivelDesbloqueado", siguienteNivel);
            PlayerPrefs.Save();
        }
    }

    // FUNCIONES ASIGNABLES A LOS BOTONES DE UI
    public void CargarSiguienteNivel()
    {
        int nivelActualBuildIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(nivelActualBuildIndex + 1);
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
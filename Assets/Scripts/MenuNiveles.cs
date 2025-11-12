using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuNiveles : MonoBehaviour
{
    // Clase para estructurar la información de cada nivel en el Inspector
    [System.Serializable]
    public class Nivel
    {
        public Button botonDesbloqueado; // Botón visible y funcional
        public Button botonBloqueado;    // Botón gris o deshabilitado
    }

    public Nivel[] niveles; // ¡Asigna los botones y sus estados en el Inspector de Unity!

    void Start()
    {
        // 1. Obtener el último nivel que el jugador ha desbloqueado. Por defecto es el Nivel 1.
        int nivelDesbloqueado = PlayerPrefs.GetInt("nivelDesbloqueado", 1);

        for (int i = 0; i < niveles.Length; i++)
        {
            // El número de nivel es el índice más 1 (para que empiece en 1, no en 0)
            int numeroNivel = i + 1;
            bool desbloqueado = numeroNivel <= nivelDesbloqueado;

            // Mostrar/ocultar los objetos de botón según el progreso
            niveles[i].botonDesbloqueado.gameObject.SetActive(desbloqueado);
            niveles[i].botonBloqueado.gameObject.SetActive(!desbloqueado);

            if (desbloqueado)
            {
                // **¡SOLUCIÓN AL ERROR DEL FOR LOOP!**
                // Se crea una variable local aquí (nivelACargar) para "capturar" 
                // el valor correcto de numeroNivel para este botón específico.
                int nivelACargar = numeroNivel;

                // Asignar la función de carga al evento de clic
                niveles[i].botonDesbloqueado.onClick.AddListener(() =>
                {
                    // **CONSTRUCCIÓN DEL NOMBRE DE LA ESCENA:**
                    // Se usa String.Format con "{0:000}" para asegurar que 
                    // el número siempre tenga 3 dígitos (ej: 1 -> "001", 10 -> "010").
                    string nombreDeEscena = string.Format("Nivel{0:000}", nivelACargar);

                    // Cargar la escena (ej: "Nivel001", "Nivel002", etc.)
                    SceneManager.LoadScene(nombreDeEscena);
                });
            }
        }
    }
}

// ARCHIVO: BoteBasura.cs

using UnityEngine;

public class BoteBasura : MonoBehaviour
{
    // Define el tipo que acepta este bote. Usa la ruta completa del Enum.
    public GameManager.TipoBasura tipoAceptado;

    private GameManager manager;

    void Start()
    {
        // Busca y guarda la referencia al GameManager al inicio
        manager = FindObjectOfType<GameManager>();
        if (manager == null)
        {
            Debug.LogError("BoteBasura no encontró el GameManager.");
        }
    }

    // Usamos OnTriggerStay para detectar si el jugador permanece dentro del trigger
    void OnTriggerStay(Collider other)
    {
        // 1. Verificar si el objeto que toca es el jugador
        if (other.CompareTag("Player"))
        {
            // 2. Verificar si el jugador presiona la tecla 'E'
            if (Input.GetKeyDown(KeyCode.E))
            {
                // 3. Notificar al GameManager
                if (manager != null)
                {
                    // Llama al método correcto en el GameManager
                    manager.RecolectarBasura(tipoAceptado);

                    Debug.Log("¡Depósito de " + tipoAceptado.ToString() + " exitoso!");

                    // Nota: Aquí deberías añadir lógica para QUITAR la basura del inventario
                    // o para DESTRUIR el objeto de basura que el jugador "lleva".
                }
            }
        }
    }
}
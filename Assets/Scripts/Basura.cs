using UnityEngine;

public class Basura : MonoBehaviour
{
    // Usa la ruta completa para acceder al Enum definido en GameManager.cs
    public GameManager.TipoBasura tipoDeBasura;

    // Esta función se llama cuando un collider (el jugador) entra en el trigger
    private void OnTriggerEnter(Collider other)
    {
        // Asegúrate de que solo el jugador la recoja (asumiendo que el jugador tiene la etiqueta "Player")
        if (other.CompareTag("Player"))
        {
            // Busca el GameManager, necesario solo para el error log.
            GameManager manager = FindObjectOfType<GameManager>();

            if (manager != null)
            {
                // *** ¡ACCIÓN CRUCIAL! ***
                // NO llamamos a manager.RecolectarBasura aquí para evitar el doble conteo.
                // El conteo ahora solo ocurre cuando se presiona 'E' en el BoteBasura.cs.

                // Destruye la basura del mundo, asumiendo que el jugador la está "tomando".
                Destroy(gameObject);
            }
            else
            {
                // Esto ayuda a depurar si el GameManager no está en la escena
                Debug.LogError("Basura no encontró el GameManager en la escena. Asegúrate de que está activo.");
            }
        }
    }
}
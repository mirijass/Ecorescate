using UnityEngine;

public class Basura : MonoBehaviour
{
    public GameManager.TipoBasura tipoDeBasura;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager manager = FindObjectOfType<GameManager>();

            if (manager != null)
            {
                // SUMAR AL INVENTARIO DEL JUGADOR
                switch (tipoDeBasura)
                {
                    case GameManager.TipoBasura.Organica:
                        manager.invOrganico++;
                        break;
                    case GameManager.TipoBasura.Plastico:
                        manager.invPlastico++;
                        break;
                    case GameManager.TipoBasura.Carton:
                        manager.invCarton++;
                        break;
                }

                Debug.Log("Basura recogida: " + tipoDeBasura);
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("No se encontró GameManager en la escena.");
            }
        }
    }
}

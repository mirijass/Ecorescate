using UnityEngine;

public class BoteBasura : MonoBehaviour
{
    public GameManager.TipoBasura tipoAceptado;

    private GameManager manager;

    void Start()
    {
        manager = FindObjectOfType<GameManager>();

        if (manager == null)
            Debug.LogError("BoteBasura no encontró GameManager.");
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (manager == null) return;

                // VERIFICAR INVENTARIO CORRECTO
                bool tieneBasura = false;

                switch (tipoAceptado)
                {
                    case GameManager.TipoBasura.Organica:
                        tieneBasura = manager.invOrganico > 0;
                        break;
                    case GameManager.TipoBasura.Plastico:
                        tieneBasura = manager.invPlastico > 0;
                        break;
                    case GameManager.TipoBasura.Carton:
                        tieneBasura = manager.invCarton > 0;
                        break;
                }

                if (!tieneBasura)
                {
                    Debug.Log("No tienes basura de este tipo para depositar.");
                    return;
                }

                // DEPOSITAR Y RESTAR DEL INVENTARIO
                switch (tipoAceptado)
                {
                    case GameManager.TipoBasura.Organica:
                        manager.invOrganico--;
                        break;
                    case GameManager.TipoBasura.Plastico:
                        manager.invPlastico--;
                        break;
                    case GameManager.TipoBasura.Carton:
                        manager.invCarton--;
                        break;
                }

                manager.RecolectarBasura(tipoAceptado);

                Debug.Log("Depositada basura: " + tipoAceptado);
            }
        }
    }
}

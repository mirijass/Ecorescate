using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiarNivel : MonoBehaviour
{
    // Esta variable aparecerá en el Inspector para que escribas el nombre de la escena
    public string nombreDeLaEscena;

    // Método para cambiar de escena usando la variable pública
    public void CambiarEscena()
    {
        SceneManager.LoadScene(nombreDeLaEscena);
    }
}
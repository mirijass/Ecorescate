using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuNiveles : MonoBehaviour
{
    [System.Serializable]
    public class Nivel
    {
        public Button botonDesbloqueado;
        public Button botonBloqueado;
    }

    public Nivel[] niveles; // Asigna desde el Inspector

    void Start()
    {
        int nivelDesbloqueado = PlayerPrefs.GetInt("nivelDesbloqueado", 1);

        for (int i = 0; i < niveles.Length; i++)
        {
            int numeroNivel = i + 1;
            bool desbloqueado = numeroNivel <= nivelDesbloqueado;

            // Mostrar/ocultar botones según el progreso
            niveles[i].botonDesbloqueado.gameObject.SetActive(desbloqueado);
            niveles[i].botonBloqueado.gameObject.SetActive(!desbloqueado);

            if (desbloqueado)
            {
                niveles[i].botonDesbloqueado.onClick.AddListener(() =>
                {
                    SceneManager.LoadScene("Nivel" + numeroNivel);
                });
            }
        }
    }
}

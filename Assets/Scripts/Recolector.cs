using UnityEngine;
using UnityEngine.UI;

public class Recolector : MonoBehaviour
{
    public int papelRecolectado = 0;
    public int plasticoRecolectado = 0;
    public int organicoRecolectado = 0;

    public Text textoPapel;
    public Text textoPlastico;
    public Text textoOrganico;

    private void OnTriggerEnter(Collider other)
    {
        // Detecta el tipo de basura por su etiqueta
        if (other.CompareTag("Papel"))
        {
            papelRecolectado++;
            textoPapel.text = "Papel: " + papelRecolectado;
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Plastico"))
        {
            plasticoRecolectado++;
            textoPlastico.text = "Plástico: " + plasticoRecolectado;
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Organico"))
        {
            organicoRecolectado++;
            textoOrganico.text = "Orgánico: " + organicoRecolectado;
            Destroy(other.gameObject);
        }
    }
}


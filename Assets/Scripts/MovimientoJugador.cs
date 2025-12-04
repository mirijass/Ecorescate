using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para chequear el nombre de la escena

public class MovimientoJugador : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad de movimiento horizontal del jugador.")]
    public float velocidadMovimiento = 5f;

    [Tooltip("Fuerza aplicada para el salto.")]
    public float fuerzaSalto = 7f;

    [Header("Chequeo de Suelo")]
    [Tooltip("Objeto vacío para chequear si el jugador toca el suelo.")]
    public Transform chequeadorSuelo;
    [Tooltip("Radio de la esfera de chequeo de suelo.")]
    public float radioChequeo = 0.2f;
    [Tooltip("LayerMask que define qué es 'suelo'.")]
    public LayerMask capaSuelo;

    // --- CONFIGURACIÓN DE AUDIO (Recolección y Pasos) ---
    [Header("Configuración de Audio")]
    [Tooltip("El clip de audio a reproducir al recoger basura.")]
    public AudioClip clipRecoleccion;

    [Tooltip("Clip de audio para el sonido de pasos.")]
    public AudioClip clipPasos;

    [Tooltip("Intervalo de tiempo (en segundos) entre cada paso.")]
    public float intervaloPasos = 0.5f;

    // Componentes Privados y Estados
    private Rigidbody rb;
    private bool estaEnSuelo;
    private AudioSource audioSFX; // Componente para reproducir sonidos cortos (SFX)
    private float contadorPasos; // Contador interno para el ritmo de los pasos

    void Start()
    {
        // Obtener el componente Rigidbody
        rb = GetComponent<Rigidbody>();

        // Comprobar si existe el Rigidbody
        if (rb == null)
        {
            Debug.LogError("El componente Rigidbody es necesario para el script MovimientoJugador.");
            enabled = false;
            return;
        }

        // --- INICIALIZACIÓN DEL AUDIO SOURCE ---
        audioSFX = GetComponent<AudioSource>();
        if (audioSFX == null)
        {
            // Si el jugador no tiene un AudioSource, lo agregamos automáticamente
            audioSFX = gameObject.AddComponent<AudioSource>();
            audioSFX.playOnAwake = false; // Asegurar que no suene al inicio
        }

        contadorPasos = 0f;
    }

    void Update()
    {
        // 1. CHEQUEAR SUELO
        estaEnSuelo = Physics.CheckSphere(chequeadorSuelo.position, radioChequeo, capaSuelo);

        // 2. SALTO
        if (Input.GetButtonDown("Jump") && estaEnSuelo)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        }

        // 3. LÓGICA DE PASOS 
        float movimientoH = Input.GetAxis("Horizontal");
        float movimientoV = Input.GetAxis("Vertical");
        bool estaMoviendose = (Mathf.Abs(movimientoH) > 0.1f || Mathf.Abs(movimientoV) > 0.1f);

        if (estaMoviendose && estaEnSuelo)
        {
            contadorPasos -= Time.deltaTime;

            if (contadorPasos <= 0)
            {
                if (audioSFX != null && clipPasos != null)
                {
                    audioSFX.PlayOneShot(clipPasos);
                }
                contadorPasos = intervaloPasos;
            }
        }
        else
        {
            contadorPasos = 0f;
        }
    }

    // Nota: El FixedUpdate que maneja el movimiento lineal fue comentado en tu versión original.

    // 5. FUNCIÓN PARA DETECTAR RECOLECCIÓN Y PENALIZACIÓN 
    private void OnTriggerEnter(Collider other)
    {
        // LÓGICA DE COLECCIÓN DE BASURA (POSITIVA)
        if (other.CompareTag("Basura"))
        {
            // 1. REPRODUCIR SONIDO
            if (audioSFX != null && clipRecoleccion != null)
            {
                audioSFX.PlayOneShot(clipRecoleccion);
            }

            // 2. LÓGICA DE JUEGO (Manejo de inventario/conteo)
            // if (GameManager.Instance != null) { ... }

            // Destruir el objeto de basura
            Destroy(other.gameObject);
        }

        // ⭐️ LÓGICA DE LA PIEDRA (COLECCIONABLE NEGATIVO) 
        if (other.CompareTag("Piedra"))
        {
            // Aplicar la penalización de tiempo solo si la escena actual es "Nivel003"
            if (SceneManager.GetActiveScene().name == "Nivel003")
            {
                if (GameManager.Instance != null)
                {
                    // Restar 5 segundos al temporizador
                    GameManager.Instance.RestarTiempo(5f);
                }
            }

            // Destruir el objeto "Piedra" al recogerlo
            Destroy(other.gameObject);
        }
    }
}
using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    [Header("Configuraci�n de Movimiento")]
    [Tooltip("Velocidad de movimiento horizontal del jugador.")]
    public float velocidadMovimiento = 5f;

    [Tooltip("Fuerza aplicada para el salto.")]
    public float fuerzaSalto = 7f;

    [Header("Chequeo de Suelo")]
    [Tooltip("Objeto vac�o para chequear si el jugador toca el suelo.")]
    public Transform chequeadorSuelo;
    [Tooltip("Radio de la esfera de chequeo de suelo.")]
    public float radioChequeo = 0.2f;
    [Tooltip("LayerMask que define qu� es 'suelo'.")]
    public LayerMask capaSuelo;

    // Componente Rigidbody
    private Rigidbody rb;
    private bool estaEnSuelo;

    void Start()
    {
        // Obtener el componente Rigidbody adjunto al Game Object
        rb = GetComponent<Rigidbody>();

        // Comprobar si existe el Rigidbody
        if (rb == null)
        {
            Debug.LogError("El componente Rigidbody es necesario para el script MovimientoJugador.");
            enabled = false; // Desactiva el script si no hay Rigidbody
        }
    }

    void Update()
    {
        // 1. CHEQUEAR SUELO
        // Crea una esfera invisible en la posici�n del chequeadorSuelo para ver si colisiona con la capaSuelo
        estaEnSuelo = Physics.CheckSphere(chequeadorSuelo.position, radioChequeo, capaSuelo);

        // 2. SALTO (Se chequea en Update para que sea m�s responsivo)
        // Se activa si se presiona Espacio, y si el jugador est� en el suelo.
        if (Input.GetButtonDown("Jump") && estaEnSuelo)
        {
            // Aplica una fuerza instant�nea hacia arriba (eje Y)
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        // El movimiento f�sico (como el Rigidbody) se debe hacer en FixedUpdate

        // 3. MOVIMIENTO HORIZONTAL
        // Input.GetAxis("Horizontal") lee tanto A/D como Flecha Izquierda/Derecha
        float movimientoH = Input.GetAxis("Horizontal");

        // Input.GetAxis("Vertical") lee tanto W/S como Flecha Arriba/Abajo
        float movimientoV = Input.GetAxis("Vertical");

        // Crear el Vector de movimiento
        // El eje Y se mantiene con la velocidad actual del Rigidbody para preservar la gravedad/salto
        Vector3 vectorMovimiento = transform.right * movimientoH + transform.forward * movimientoV;

        // Aplicar la velocidad (ignorando el eje Y)
        Vector3 nuevaVelocidad = new Vector3(vectorMovimiento.x * velocidadMovimiento, rb.linearVelocity.y, vectorMovimiento.z * velocidadMovimiento);
        rb.linearVelocity = nuevaVelocidad;
    }
}
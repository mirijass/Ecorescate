using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // --- VARIABLES PÚBLICAS ---
    public float runSpeed = 7f;         // Usar 'f' para flotantes.
    public float rotationSpeed = 250f;

    public Animator animator;

    private float x, y;

    void Update()
    {
        // 1. OBTENER ENTRADAS
        x = Input.GetAxis("Horizontal"); // Entrada A/D o flechas izquierda/derecha
        y = Input.GetAxis("Vertical");   // Entrada W/S o flechas arriba/abajo

        // 2. ROTACIÓN (Giro)
        // Gira el objeto en el eje Y (vertical) usando la entrada horizontal (x) y rotationSpeed
        // Se multiplica por Time.deltaTime para que sea independiente de los FPS.
        transform.Rotate(0, x * rotationSpeed * Time.deltaTime, 0);

        // 3. MOVIMIENTO (Traslación)
        // Mueve el objeto hacia adelante (Vector3.forward) usando la entrada vertical (y)
        // Se multiplica por runSpeed y Time.deltaTime.
        transform.Translate(0, 0, y * runSpeed * Time.deltaTime);

        // 4. ANIMACIÓN (Opcional, si tienes un Animator configurado)
        // Si usas animación, querrás setear el parámetro 'Speed' o 'Forward'
        // para que el personaje camine. Usamos el valor absoluto de 'y' para la velocidad.
        
        animator.SetFloat("VelX", y);
        animator.SetFloat("VelY", y);
    }
}
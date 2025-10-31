using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 200f;
    public Transform cam; // assign PlayerCamera here in Inspector

    private Rigidbody rb;
    private float rotX; // vertical camera rotation (pitch)

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Kunci cursor ke tengah layar biar FPS style
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
    }

    void FixedUpdate()
    {
        HandleMove();
    }

    void HandleMove()
    {
        // ambil input WASD
        float inputX = Input.GetAxisRaw("Horizontal"); // A/D
        float inputZ = Input.GetAxisRaw("Vertical");   // W/S

        // arah gerak relatif terhadap orientasi player (bukan world)
        Vector3 moveDir = (transform.forward * inputZ + transform.right * inputX).normalized;

        // kecepatan target
        Vector3 velocity = moveDir * moveSpeed;
        velocity.y = rb.linearVelocity.y; // jaga gravitasi tetap jalan

        rb.linearVelocity = velocity;
    }

    void HandleLook()
    {
        // ambil input mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // rotasi horizontal badan player (yaw)
        transform.Rotate(Vector3.up * mouseX);

        // rotasi vertical kamera (pitch)
        rotX -= mouseY;
        rotX = Mathf.Clamp(rotX, -80f, 80f); // biar gak muter 360 derajat ke atas/bawah

        cam.localRotation = Quaternion.Euler(rotX, 0f, 0f);
    }
}

using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    [Header("Pengaturan Gerak")]
    public float kecepatanJalan = 15f; // Agak cepat karena kakinya panjang (8 meter)
    public float gravitasi = -9.81f * 2; // Gravitasi diperberat biar gak melayang
    
    [Header("Pengaturan Kamera")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera; // Drag Main Camera ke sini nanti
    
    // Private variables
    CharacterController controller;
    float xRotation = 0f;
    Vector3 velocity;
    bool isGrounded;

    void Start()
    {
        // Mengambil komponen Character Controller otomatis
        controller = GetComponent<CharacterController>();
        
        // Menyembunyikan cursor mouse saat main
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 1. PENGATURAN MOUSE (Lihat Kiri-Kanan-Atas-Bawah)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Batasi dongak atas/bawah

        // Gerakkan kamera (atas-bawah)
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        // Putar badan player (kiri-kanan)
        transform.Rotate(Vector3.up * mouseX);

        // 2. PERGERAKAN JALAN (WASD)
        // Cek apakah napak tanah
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Reset gravitasi saat napak
        }

        float x = Input.GetAxis("Horizontal"); // A - D
        float z = Input.GetAxis("Vertical");   // W - S

        // Gerak sesuai arah hadap player
        Vector3 move = transform.right * x + transform.forward * z;
        
        controller.Move(move * kecepatanJalan * Time.deltaTime);

        // 3. GRAVITASI
        velocity.y += gravitasi * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
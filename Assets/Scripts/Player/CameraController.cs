using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player; // Referensi ke pemain
    public Vector3 offset = new Vector3(0, 3, -6); // Jarak default kamera dari pemain
    public float rotationSpeed = 3.0f; // Kecepatan rotasi kamera
    public float collisionOffset = 0.2f; // Jarak offset untuk menghindari tabrakan
    public LayerMask collisionLayer; // Layer untuk objek yang bisa bertabrakan dengan kamera

    private Vector3 currentOffset; // Jarak kamera saat ini dari pemain
    private float currentZoom = 1.0f; // Faktor zoom dinamis untuk menghindari tabrakan

    void Start()
    {
        currentOffset = offset;

        // Lock Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        RotateCamera();
        HandleCameraCollision();
        UpdateCameraPosition();
    }

    // Fungsi untuk rotasi kamera berdasarkan input mouse
    void RotateCamera()
    {
        float horizontalInput = Input.GetAxis("Mouse X") * rotationSpeed;
        float verticalInput = -Input.GetAxis("Mouse Y") * rotationSpeed;

        Quaternion rotation = Quaternion.Euler(verticalInput, horizontalInput, 0);
        offset = rotation * offset;

        // Pastikan kamera tidak terlalu rendah atau tinggi
        offset = new Vector3(offset.x, Mathf.Clamp(offset.y, 1.0f, 5.0f), offset.z);
    }

    // Fungsi untuk mendeteksi dan menangani tabrakan dengan objek
    void HandleCameraCollision()
    {
        RaycastHit hit;

        // Membuat ray dari pemain ke kamera untuk mendeteksi tabrakan
        Vector3 desiredPosition = player.position + offset * currentZoom;
        if (Physics.Linecast(player.position, desiredPosition, out hit, collisionLayer))
        {
            // Kamera akan bergerak lebih dekat ke pemain jika ada objek di antara
            float distance = Vector3.Distance(player.position, hit.point) - collisionOffset;
            currentZoom = Mathf.Clamp(distance / offset.magnitude, 0.3f, 1.0f);
        }
        else
        {
            // Kembali ke zoom default jika tidak ada objek
            currentZoom = Mathf.Lerp(currentZoom, 1.0f, Time.deltaTime * 5f);
        }
    }

    // Fungsi untuk memperbarui posisi kamera mengikuti pemain
    void UpdateCameraPosition()
    {
        Vector3 targetPosition = player.position + offset * currentZoom;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
        transform.LookAt(player.position + Vector3.up * 1.5f); // Menatap pemain dengan sedikit elevasi
    }
}

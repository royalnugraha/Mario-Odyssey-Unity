using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;          // Target pemain
    public Vector3 offset = new Vector3(0, 2, -5);  // Posisi relatif kamera terhadap pemain
    public float rotationSpeed = 5f;  // Kecepatan rotasi kamera
    public float minDistance = 1f;    // Jarak minimal antara kamera dan pemain saat ada halangan
    public float maxDistance = 5f;    // Jarak maksimal dari offset asli kamera
    public LayerMask collisionLayers; // Layer untuk mendeteksi halangan

    private float currentDistance;    // Jarak saat ini dari kamera ke pemain
    private Vector3 currentOffset;    // Offset kamera yang diperbarui berdasarkan halangan

    void Start()
    {
        currentDistance = offset.magnitude;
        currentOffset = offset;

        // Cursor Lock
        Cursor.visible = false; // Menyembunyikan kursor
        Cursor.lockState = CursorLockMode.Locked; // Mengunci kursor di tengah
    }

    void LateUpdate()
    {
        RotateCamera();
        HandleCollision();
        UpdateCameraPosition();
    }

    void RotateCamera()
    {
        float horizontalInput = Input.GetAxis("Mouse X") * rotationSpeed;
        float verticalInput = -Input.GetAxis("Mouse Y") * rotationSpeed;

        // Rotasi kamera berdasarkan input mouse
        Quaternion rotation = Quaternion.Euler(verticalInput, horizontalInput, 0);
        offset = rotation * offset;
        currentOffset = offset.normalized * currentDistance;
    }

    void HandleCollision()
    {
        RaycastHit hit;

        // Cek apakah ada objek di antara kamera dan pemain
        if (Physics.Raycast(player.position, -currentOffset.normalized, out hit, maxDistance, collisionLayers))
        {
            currentDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }
        else
        {
            // Jika tidak ada halangan, kembali ke jarak maksimum
            currentDistance = maxDistance;
        }

        // Sesuaikan offset berdasarkan jarak saat ini
        currentOffset = offset.normalized * currentDistance;
    }

    void UpdateCameraPosition()
    {
        // Posisi akhir kamera dengan mempertimbangkan offset dan rotasi
        Vector3 targetPosition = player.position + currentOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * rotationSpeed);
        transform.LookAt(player.position + Vector3.up * 1.5f); // Titik pandang sedikit di atas pemain
    }
}

using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; 
    public Vector3 offset = new Vector3(0, 0, -10);
    
    [Header("Smoothness")]
    // Gunakan nilai rendah (0.05 - 0.1) untuk respon instan tapi halus
    public float smoothTime = 0.1f; 
    private Vector3 currentVelocity = Vector3.zero;

    [Header("Dynamic Zoom")]
    public float minSize = 5f;
    public float maxSize = 12f;
    public float zoomSensitivity = 0.15f;
    public float zoomSmoothTime = 0.2f; // Smoothness khusus untuk zoom
    private float currentZoomVelocity = 0f;

    private Camera cam;
    private Rigidbody2D targetRb;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (target != null)
            targetRb = target.GetComponent<Rigidbody2D>();
            
        // Paksa posisi kamera ke target saat mulai agar tidak "lompat"
        if (target != null)
            transform.position = target.position + offset;
    }

    // KUNCI UTAMA: Gunakan LateUpdate untuk kamera agar posisi Player sudah 
    // benar-benar final dihitung oleh Fisika sebelum kamera bergerak.
    void LateUpdate()
    {
        if (target == null) return;

        // 1. POSISI: Menggunakan SmoothDamp di LateUpdate adalah kombinasi terbaik
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        // 2. ZOOM: Menggunakan SmoothDamp juga agar transisi zoom tidak kaku
        if (targetRb != null)
        {
            float speed = targetRb.linearVelocity.magnitude;
            float desiredSize = Mathf.Clamp(minSize + (speed * zoomSensitivity), minSize, maxSize);
            
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, desiredSize, ref currentZoomVelocity, zoomSmoothTime);
        }
    }
}
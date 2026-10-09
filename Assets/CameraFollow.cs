using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; 
    public Vector3 offset = new Vector3(0, 0, -10);
    
    [Header("Smoothness")]
    public float smoothTime = 0.05f; // Diturunkan sedikit agar kamera lebih cepat menempel saat terbang
    private Vector3 currentVelocity = Vector3.zero;

    [Header("Dynamic Zoom")]
    public float minSize = 5f;
    public float maxSize = 12f;
    public float zoomSensitivity = 0.15f;
    public float zoomSmoothTime = 0.2f;
    private float currentZoomVelocity = 0f;

    private Camera cam;
    private Rigidbody2D targetRb;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody2D>();
            // Paksa posisi kamera langsung di target saat mulai
            transform.position = target.position + offset;
        }
    }

    // Menggunakan FixedUpdate agar posisi kamera diperbarui persis bersamaan dengan kalkulasi Rigidbody2D
    void FixedUpdate()
    {
        if (target == null) return;

        // 1. POSISI: Ikuti target di FixedUpdate
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime, Mathf.Infinity, Time.fixedDeltaTime);

        // 2. DYNAMIC ZOOM
        if (targetRb != null)
        {
            float speed = targetRb.linearVelocity.magnitude;
            float desiredSize = Mathf.Clamp(minSize + (speed * zoomSensitivity), minSize, maxSize);
            
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, desiredSize, ref currentZoomVelocity, zoomSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);
        }
    }
}
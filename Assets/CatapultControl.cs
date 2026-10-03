using UnityEngine;

public class CatapultControl : MonoBehaviour
{
    private Rigidbody2D rb;
    public float launchForce = 15f; // Tingkatkan jika terasa lambat

    // --- FITUR SLOW MOTION ---
    [Range(0f, 1f)]
    public float slowMoFactor = 0.2f; // 0.2 berarti waktu berjalan 20% dari kecepatan normal
    private float normalTimeScale = 1f;

    private bool isDragging = false;
    private Vector2 startPoint;
    private Vector2 endPoint;
    private LineRenderer lr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        lr = gameObject.AddComponent<LineRenderer>();
        lr.enabled = false;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.white;
        lr.endColor = Color.clear;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            lr.enabled = true;
            lr.SetPosition(0, startPoint);

            // MULAI SLOW MOTION
            Time.timeScale = slowMoFactor;
            // Menyesuaikan waktu fisika agar tetap halus saat slow-mo
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lr.SetPosition(1, endPoint);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            lr.enabled = false;

            // KEMBALIKAN WAKTU NORMAL
            Time.timeScale = normalTimeScale;
            Time.fixedDeltaTime = 0.02f;

            endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 forceDirection = startPoint - endPoint;
            
            rb.AddForce(forceDirection * launchForce, ForceMode2D.Impulse);
        }
    }
}
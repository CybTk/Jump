using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f; //
    public float lifetime = 3f; //
    private Rigidbody2D rb; //[cite: 7]

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); //[cite: 7]

        // Jaga-jaga jika Rigidbody2D belum diset 0 gravitasinya
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    void Start()
    {
        Destroy(gameObject, lifetime); // Hapus otomatis setelah beberapa detik[cite: 7]
    }

    public void Launch(Vector2 direction) //[cite: 7]
    {
        if (rb != null) //[cite: 7]
        {
            rb.linearVelocity = direction.normalized * speed; //[cite: 7]
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ABAIKAN jika yang disentuh adalah Player atau sesama Projectile
        if (collision.CompareTag("Player") || collision.CompareTag("Projectile"))
        {
            return;
        }

        // Hancurkan peluru hanya jika menabrak Dinding/Musuh/Objek lain
        Destroy(gameObject);
    }
}
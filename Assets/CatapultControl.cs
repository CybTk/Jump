using UnityEngine;

public class CatapultControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float launchForce = 15f;
    private Rigidbody2D rb;

    [Header("Shooting Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Slow Motion Settings")]
    [Range(0f, 1f)]
    public float slowMoFactor = 0.2f;
    private float normalTimeScale = 1f;

    [Header("Ground Check Settings")]
    public LayerMask groundLayer;
    private bool isGrounded;
    private float groundCheckBufferTimer = 0f;

    [Header("Visual & Animation")]
    public SpriteRenderer spriteRenderer;
    private Animator animator;

    // Multi-Touch Tracking (-1 berarti tidak ada sentuhan aktif)
    private int moveTouchId = -1;
    private int shootTouchId = -1;

    private Vector2 moveStartPos;
    private Vector2 shootStartPos;

    private Vector2 currentMoveDir;
    private Vector2 currentShootDir;

    // Line Renderers
    private LineRenderer movementLine;
    private LineRenderer aimingLine;

    private bool lastFacingLeft = false;

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (firePoint == null) firePoint = transform;

        movementLine = CreateLineRenderer(Color.cyan, Color.clear);
        aimingLine = CreateLineRenderer(Color.red, new Color(1f, 0.5f, 0f, 0f));
    }

    LineRenderer CreateLineRenderer(Color startCol, Color endCol)
    {
        GameObject lineObj = new GameObject("LineRendererObj");
        lineObj.transform.SetParent(transform);
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.enabled = false;
        lr.startWidth = 0.08f;
        lr.endWidth = 0.02f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = startCol;
        lr.endColor = endCol;
        return lr;
    }

    void Update()
    {
        // Update Buffer Timer
        if (groundCheckBufferTimer > 0)
        {
            groundCheckBufferTimer -= Time.unscaledDeltaTime;
            isGrounded = false;
        }

        // Input & SlowMo
        if (Input.touchCount > 0) ProcessTouches();
        else ProcessMouseFallback();

        if (moveTouchId != -1 || shootTouchId != -1) EnableSlowMotion();
        else DisableSlowMotion();

        UpdateVisualLines();

        // Update Animator Parameters
        if (animator != null)
        {
            bool isCharging = (moveTouchId != -1);
            animator.SetBool("isCharging", isCharging);
            animator.SetBool("isGrounded", isGrounded);

            if (isGrounded)
            {
                animator.ResetTrigger("doLaunch");
            }
        }
    }

    // DETEKSI FISIKA TABRAKAN DENGAN TANAH (SOLUSI PASTI UNTUK MENDARAT LURUS/KIRI)
    private void OnCollisionStay2D(Collision2D collision)
{
    // Cek apakah menyentuh tanah dari arah bawah/samping
    if (((1 << collision.gameObject.layer) & groundLayer) != 0 && groundCheckBufferTimer <= 0)
    {
        isGrounded = true;
    }
}

private void OnCollisionExit2D(Collision2D collision)
{
    if (((1 << collision.gameObject.layer) & groundLayer) != 0)
    {
        // Berikan buffer singkat agar micro-bounce di atas lantai tidak membuat isGrounded flicker
        isGrounded = false;
    }
}

void LateUpdate()
{
    if (spriteRenderer == null) return;

    bool isCharging = (moveTouchId != -1);

    // A. KONDISI DI DARAT (IDLE / SLIDE / HOLD GROUND)
    if (isGrounded)
    {
        // Kunci rotasi lewat Rigidbody agar fisika tidak memutar karakter
        rb.freezeRotation = true;
        
        // Cukup reset rotasi SEKALI lewat Rigidbody, bukan memaksa transform.rotation tiap frame
        rb.rotation = 0f;
        spriteRenderer.transform.localRotation = Quaternion.identity;
        rb.angularVelocity = 0f;

        // Reset Flip Y
        spriteRenderer.flipY = false;

        // Atur Flip X (Arah hadap)
        if (isCharging && currentMoveDir != Vector2.zero)
        {
            lastFacingLeft = (currentMoveDir.x < 0);
        }
        spriteRenderer.flipX = lastFacingLeft;
    }
    // B. KONDISI HOLD DI UDARA
    else if (isCharging && currentMoveDir != Vector2.zero)
    {
        rb.freezeRotation = false;
        spriteRenderer.flipX = false;

        float angle = Mathf.Atan2(currentMoveDir.y, currentMoveDir.x) * Mathf.Rad2Deg;
        spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, angle);

        spriteRenderer.flipY = (currentMoveDir.x < 0);
        if (currentMoveDir.x != 0) lastFacingLeft = (currentMoveDir.x < 0);
    }
    // C. KONDISI MELUNCUR DI UDARA
    else if (!isGrounded && rb.linearVelocity.sqrMagnitude > 0.1f)
    {
        rb.freezeRotation = false;
        spriteRenderer.flipX = false;

        float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, angle);

        spriteRenderer.flipY = (rb.linearVelocity.x < 0);

        if (rb.linearVelocity.x < -0.2f) lastFacingLeft = true;
        else if (rb.linearVelocity.x > 0.2f) lastFacingLeft = false;
    }
}
    void ProcessTouches()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                if (touch.position.x < Screen.width / 2f && moveTouchId == -1)
                {
                    moveTouchId = touch.fingerId;
                    moveStartPos = touch.position;
                }
                else if (touch.position.x >= Screen.width / 2f && shootTouchId == -1)
                {
                    shootTouchId = touch.fingerId;
                    shootStartPos = touch.position;
                }
            }

            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                if (touch.fingerId == moveTouchId)
                {
                    currentMoveDir = (touch.position - moveStartPos).normalized;
                }
                else if (touch.fingerId == shootTouchId)
                {
                    currentShootDir = (touch.position - shootStartPos).normalized;
                }
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (touch.fingerId == moveTouchId)
                {
                    Vector2 offset = touch.position - moveStartPos;
                    if (offset.magnitude > 10f)
                    {
                        ExecuteLaunch(offset.magnitude);
                    }
                    moveTouchId = -1;
                    movementLine.enabled = false;
                }
                else if (touch.fingerId == shootTouchId)
                {
                    Vector2 offset = touch.position - shootStartPos;
                    if (offset.magnitude > 10f && projectilePrefab != null)
                    {
                        ShootProjectile(currentShootDir);
                    }
                    shootTouchId = -1;
                    aimingLine.enabled = false;
                }
            }
        }
    }

    void ExecuteLaunch(float offsetMagnitude)
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(currentMoveDir * (Mathf.Clamp(offsetMagnitude, 0f, 250f) * 0.05f * launchForce), ForceMode2D.Impulse);

        groundCheckBufferTimer = 0.15f;
        isGrounded = false;

        if (animator != null)
        {
            animator.ResetTrigger("doLaunch");
            animator.SetTrigger("doLaunch");
        }
    }

    void UpdateVisualLines()
    {
        Vector3 playerPos = transform.position;
        playerPos.z = 0f;

        if (moveTouchId != -1)
        {
            movementLine.enabled = true;
            movementLine.SetPosition(0, playerPos);
            movementLine.SetPosition(1, playerPos + (Vector3)(currentMoveDir * 2f));
        }

        if (shootTouchId != -1)
        {
            aimingLine.enabled = true;
            aimingLine.SetPosition(0, playerPos);
            aimingLine.SetPosition(1, playerPos + (Vector3)(currentShootDir * 3f));
        }
    }

    void ShootProjectile(Vector2 direction)
    {
        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile proj = projObj.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Launch(direction);
        }
    }

    void EnableSlowMotion()
{
    Time.timeScale = slowMoFactor;
    // Jaga rasio fixedDeltaTime agar kalkulasi fisika saat slide tetap mulus
    Time.fixedDeltaTime = 0.02f * Time.timeScale; 
}

void DisableSlowMotion()
{
    Time.timeScale = normalTimeScale;
    Time.fixedDeltaTime = 0.02f;
}
    void ProcessMouseFallback()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            if (mousePos.x < Screen.width / 2f) { moveTouchId = 99; moveStartPos = mousePos; }
            else { shootTouchId = 99; shootStartPos = mousePos; }
        }
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;
            if (moveTouchId == 99) currentMoveDir = (mousePos - moveStartPos).normalized;
            if (shootTouchId == 99) currentShootDir = (mousePos - shootStartPos).normalized;
        }
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 mousePos = Input.mousePosition;
            if (moveTouchId == 99)
            {
                Vector2 offset = mousePos - moveStartPos;
                ExecuteLaunch(offset.magnitude);
                moveTouchId = -1; movementLine.enabled = false;
            }
            else if (shootTouchId == 99)
            {
                ShootProjectile(currentShootDir);
                shootTouchId = -1; aimingLine.enabled = false;
            }
        }
    }
}
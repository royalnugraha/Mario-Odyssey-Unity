using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    // Komponen
    private Rigidbody rb;
    private Animator animator;
    private AudioSource audioSource;

    // Variabel Climbing
    public float climbSpeed = 2f;
    public LayerMask climbableLayer;
    private bool canClimb;
    private Transform climbPoint;

    // Variabel Movement
    public float speed = 5f;
    public float jumpForce = 7f;
    public float longJumpForce = 10f;
    public float diveForce = 15f;
    public float rollSpeed = 8f;
    public float wallJumpForce = 7f;
    private bool isGrounded;
    private bool isRolling;
    private bool isClimbing;
    private int jumpCount;

    // Variabel Cappy
    public GameObject cappy;
    public Transform cappyThrowPoint;
    private bool cappyThrown;
    private bool isCaptured;

    // Sound Effects
    public AudioClip jumpSound;
    public AudioClip walkSound;
    public AudioClip groundPoundSound;
    public AudioClip diveSound;
    public AudioClip longJumpSound;
    public AudioClip wallJumpSound;
    public AudioClip capThrowSound;
    public AudioClip captureSound;

    // Variabel Kamera
    [SerializeField] private Transform cameraTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Optional: Assign camera automatically if not set in the Inspector
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (isClimbing)
        {
            HandleClimbing();
        }
        else
        {
            Move();
            HandleJumping();
            HandleTripleJump();
            HandleLongJump();
            HandleDiving();
            HandleGroundPound();
            HandleRolling();
            HandleWallJump();
            HandleSideFlip();
            HandleCappyThrow();
            HandleCapJump();
            HandleSpinJump();
            HandleCaptureMechanic();
            AnimatePlayer();
        }

        CheckForClimb();
    }

    // Fungsi untuk movement dasar
    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Adjust move direction based on the camera's forward direction
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Keep the player movement parallel to the ground
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Calculate movement direction relative to camera
        Vector3 moveDir = forward * v + right * h;
        rb.MovePosition(transform.position + moveDir * speed * Time.deltaTime);

        // Rotate player to face movement direction
        if (moveDir != Vector3.zero)
            transform.forward = moveDir;

        // Trigger walk animation and sound
        animator.SetBool("isWalking", moveDir.magnitude > 0);
        if (moveDir.magnitude > 0 && isGrounded && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(walkSound);
        }
    }


    // Fungsi untuk lompat dasar
    void HandleJumping()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            jumpCount = 1;
            animator.SetBool("isJumping", true);
            audioSource.PlayOneShot(jumpSound);
        }
    }

    // Fungsi untuk Triple Jump
    void HandleTripleJump()
    {
        if (Input.GetButtonDown("Jump") && !isGrounded && jumpCount == 2)
        {
            rb.AddForce(Vector3.up * jumpForce * 1.2f, ForceMode.Impulse);
            jumpCount = 3;
            animator.SetTrigger("isTripleJumping");
            audioSource.PlayOneShot(jumpSound);
        }
    }

    // Fungsi untuk Long Jump
    void HandleLongJump()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && isGrounded && Input.GetButton("Jump"))
        {
            rb.AddForce(transform.forward * longJumpForce + Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("isLongJumping");
            audioSource.PlayOneShot(longJumpSound);
        }
    }

    // Fungsi untuk Dive
    void HandleDiving()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isGrounded && !isRolling)
        {
            rb.AddForce(transform.forward * diveForce, ForceMode.Impulse);
            animator.SetTrigger("isDiving");
            audioSource.PlayOneShot(diveSound);
        }
    }

    // Fungsi untuk Ground Pound
    void HandleGroundPound()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isGrounded)
        {
            rb.AddForce(Vector3.down * diveForce, ForceMode.Impulse);
            animator.SetTrigger("isGroundPounding");
            audioSource.PlayOneShot(groundPoundSound);
        }
    }

    // Fungsi untuk Roll
    void HandleRolling()
    {
        if (Input.GetKeyDown(KeyCode.R) && isGrounded)
        {
            isRolling = true;
            rb.AddForce(transform.forward * rollSpeed, ForceMode.Impulse);
            animator.SetBool("isRolling", true);
        }
        else if (isRolling && Input.GetKeyUp(KeyCode.R))
        {
            isRolling = false;
            animator.SetBool("isRolling", false);
        }
    }

    // Fungsi untuk Wall Jump
    void HandleWallJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isGrounded && jumpCount == 1)
        {
            rb.AddForce(Vector3.up * wallJumpForce + -transform.forward * wallJumpForce, ForceMode.Impulse);
            jumpCount = 2;
            animator.SetTrigger("isWallJumping");
            audioSource.PlayOneShot(wallJumpSound);
        }
    }

    // Fungsi untuk Side Flip
    void HandleSideFlip()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Input.GetAxis("Horizontal") != 0)
        {
            rb.AddForce(Vector3.up * jumpForce * 1.1f, ForceMode.Impulse);
            animator.SetTrigger("isSideFlipping");
            audioSource.PlayOneShot(jumpSound);
        }
    }

    // Fungsi untuk Cappy Throw
    void HandleCappyThrow()
    {
        if (Input.GetKeyDown(KeyCode.F) && !cappyThrown)
        {
            GameObject thrownCappy = Instantiate(cappy, cappyThrowPoint.position, Quaternion.identity);
            thrownCappy.GetComponent<Rigidbody>().AddForce(transform.forward * 10f, ForceMode.Impulse);
            audioSource.PlayOneShot(capThrowSound);
            cappyThrown = true;
        }
    }

    // Fungsi untuk Cap Jump
    void HandleCapJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && cappyThrown)
        {
            rb.AddForce(Vector3.up * jumpForce * 1.2f, ForceMode.Impulse);
            animator.SetTrigger("isCapJumping");
            cappyThrown = false; // Reset cap throw untuk loncatan berikutnya
            audioSource.PlayOneShot(jumpSound);
        }
    }

    // Fungsi untuk Spin Jump
    void HandleSpinJump()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("isSpinJumping");
            audioSource.PlayOneShot(jumpSound);
        }
    }

    // Fungsi untuk Capture Mechanic
    void HandleCaptureMechanic()
    {
        if (Input.GetKeyDown(KeyCode.C) && cappyThrown)
        {
            isCaptured = true;
            animator.SetBool("isCaptured", true);
            audioSource.PlayOneShot(captureSound);
            // Logic tambahan untuk capture (contoh: ambil kontrol makhluk yang dicapture)
        }
    }

    // Fungsi untuk Climbing
    void HandleClimbing()
    {
        float v = Input.GetAxis("Vertical");
        
        if (v > 0)
        {
            rb.MovePosition(transform.position + Vector3.up * climbSpeed * Time.deltaTime);
            animator.SetBool("isClimbing", true);
        }
        else if (v < 0)
        {
            rb.MovePosition(transform.position + Vector3.down * climbSpeed * Time.deltaTime);
        }

        if (Input.GetButtonDown("Jump"))
        {
            isClimbing = false;
            rb.isKinematic = false;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetBool("isClimbing", false);
        }
    }

    // Deteksi tepi untuk climbing
    void CheckForClimb()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up;
        
        if (Physics.Raycast(origin, transform.forward, out hit, 1f, climbableLayer))
        {
            canClimb = true;
            climbPoint = hit.transform;
        }
        else
        {
            canClimb = false;
        }

        if (canClimb && Input.GetKeyDown(KeyCode.E))
        {
            StartClimb();
        }
    }

    void StartClimb()
    {
        isClimbing = true;
        rb.isKinematic = true;
        transform.position = climbPoint.position - new Vector3(0, 1, 0); // Posisi climbing menyesuaikan objek
        animator.SetBool("isClimbing", true);
    }


    // Fungsi Animasi
    void AnimatePlayer()
    {
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isCaptured", isCaptured);
        // Set animasi lainnya sesuai mekanik
    }

    // Deteksi Grounded
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpCount = 0;
            animator.SetBool("isJumping", false);
        }
    }
}

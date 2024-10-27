using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Basic Variables
    public float walkSpeed = 5f;
    public float jumpForce = 7f;
    public float longJumpForce = 10f;
    public float groundPoundForce = -20f;
    public float diveSpeed = 10f;
    public float wallJumpForce = 10f;
    public float capThrowCooldown = 1f;
    public float rollSpeed = 10f; // Kecepatan rolling

    public GameObject playerCamera;

    private bool isGrounded;
    private bool isLongJumping;
    private bool isRolling;
    private bool canDoubleJump;
    private bool canTripleJump;
    private bool canWallJump;
    private bool isSwimming;
    private bool isClimbing;
    private float capThrowTimer;

    // Components
    private Rigidbody rb;
    private Animator animator;
    private CappyController cappy;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        cappy = GetComponentInChildren<CappyController>();
    }

    void Update()
    {
        Move();
        Jump();
        WallJump();
        LongJump();
        CapThrow();
        SpecialActions();
        SideFlipAndBackFlip();
    }

    // Movement
    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        // Mendapatkan arah kamera
        Vector3 cameraForward = playerCamera.transform.forward;
        cameraForward.y = 0; // Menghilangkan komponen y
        Vector3 cameraRight = playerCamera.transform.right;
        cameraRight.y = 0; // Menghilangkan komponen y

        // Hitung arah gerakan berdasarkan kamera
        Vector3 moveDirection = (cameraForward.normalized * vertical + cameraRight.normalized * horizontal).normalized;

        if (isGrounded && !isRolling && moveDirection.magnitude >= 0.1f)
        {
            float targetSpeed = walkSpeed;
            rb.MovePosition(transform.position + moveDirection * targetSpeed * Time.deltaTime);

            // Atur rotasi pemain
            transform.rotation = Quaternion.LookRotation(moveDirection);

            animator.SetBool("isWalking", true);
        }
        else
        {
            // Kembalikan ke posisi tegak
            animator.SetBool("isWalking", false);
        }
    }



    // Long Jump
    void LongJump()
    {
        // Cek apakah pemain bergerak maju, di tanah, dan menekan tombol khusus (misalnya, "Fire3")
        if (Input.GetButtonDown("Fire3") && isGrounded && rb.velocity.magnitude > 0.1f)
        {
            Vector3 longJumpDirection = transform.forward * longJumpForce;
            rb.velocity = new Vector3(longJumpDirection.x, jumpForce, longJumpDirection.z);
            isGrounded = false;
            isLongJumping = true;
            animator.SetBool("isLongJumping", true); // Atur animasi Long Jump
        }
    }

    // Jump
    void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && !isLongJumping) // Tambahkan cek !isLongJumping
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            isGrounded = false;
            canDoubleJump = true;
            animator.SetBool("isJumping", true);
        }
        else if (Input.GetButtonDown("Jump") && canDoubleJump)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            canDoubleJump = false;
            canTripleJump = true;
            animator.SetBool("isDoubleJumping", true);
        }
        else if (Input.GetButtonDown("Jump") && canTripleJump)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            canTripleJump = false;
            animator.SetBool("isTripleJumping", true);
        }
    }

    // Wall Jump and Cap Jump
    void WallJump()
    {
        if (canWallJump && Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector3(rb.velocity.x, wallJumpForce, rb.velocity.z);
            canWallJump = false;
            animator.SetBool("isWallJumping", true);
        }
    }

    void CapThrow()
    {
        capThrowTimer -= Time.deltaTime;
        if (Input.GetButtonDown("Fire1") && capThrowTimer <= 0)
        {
            cappy.ThrowCap();
            capThrowTimer = capThrowCooldown;
            animator.SetBool("isThrowingCap", true);
        }
    }

    void SpecialActions()
    {
        // Ground Pound
        if (Input.GetButtonDown("Fire2") && !isGrounded)
        {
            rb.velocity = new Vector3(0, groundPoundForce, 0);
            animator.SetBool("isGroundPounding", true);
        }

        // Dive
        if (Input.GetButtonDown("Fire3") && !isGrounded)
        {
            Vector3 diveDirection = transform.forward * diveSpeed;
            rb.velocity = new Vector3(diveDirection.x, rb.velocity.y, diveDirection.z);
            animator.SetBool("isDiving", true);
        }

        // Roll (on ground and moving)
        if (Input.GetButton("Roll") && isGrounded && rb.velocity.magnitude > 0)
        {
            // Dapatkan arah kamera
            Vector3 cameraDirection = Camera.main.transform.forward;
            cameraDirection.y = 0; // Mengabaikan gerakan vertikal
            cameraDirection.Normalize(); // Normalisasi untuk mendapatkan vektor arah yang tepat

            // Mengatur kecepatan rolling
            rb.velocity = cameraDirection * rollSpeed; // rollSpeed adalah variabel baru yang perlu Anda tambahkan
            isRolling = true;
            animator.SetBool("isRolling", true);
        }
        else
        {
            isRolling = false;
            animator.SetBool("isRolling", false);
        }

        // Swimming
        if (isSwimming)
        {
            Swim();
        }

        // Climbing
        if (isClimbing)
        {
            Climb();
        }
    }

    // Side Flip and Back Flip
    void SideFlipAndBackFlip()
    {
        float horizontal = Input.GetAxis("Horizontal");
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            if (horizontal > 0) // Right movement
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                rb.AddForce(new Vector3(5f, 0, 0), ForceMode.Impulse); // Gaya ke arah kanan
                animator.SetBool("isSideFlipping", true);
            }
            else if (horizontal < 0) // Left movement
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                rb.AddForce(new Vector3(-5f, 0, 0), ForceMode.Impulse); // Gaya ke arah kiri
                animator.SetBool("isSideFlipping", true);
            }
            else if (horizontal == 0 && Input.GetAxis("Vertical") < 0) // Backward movement
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                animator.SetBool("isBackFlipping", true);
            }
        }
    }

    // Swimming and Climbing
    void Swim()
    {
        // Mengatur kecepatan berenang
        float swimSpeed = 3f; // Kecepatan berenang (dapat disesuaikan)

        // Mengambil input untuk bergerak
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Menghitung arah berenang
        Vector3 swimDirection = new Vector3(horizontal, 0, vertical).normalized;

        // Menggerakkan pemain
        if (swimDirection.magnitude >= 0.1f)
        {
            // Bergerak dalam arah yang ditentukan
            rb.MovePosition(transform.position + swimDirection * swimSpeed * Time.deltaTime);

            // Atur rotasi pemain ke arah berenang
            Quaternion targetRotation = Quaternion.LookRotation(swimDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Menyelaraskan rotasi
        }

        // Kontrol naik dan turun
        if (Input.GetKey(KeyCode.Space)) // Naik
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce * 0.5f, rb.velocity.z); // Naik dengan kecepatan tertentu
        }
        else if (Input.GetKey(KeyCode.LeftControl)) // Turun
        {
            rb.velocity = new Vector3(rb.velocity.x, -jumpForce * 0.5f, rb.velocity.z); // Turun dengan kecepatan tertentu
        }
        else
        {
            // Menjaga kecepatan vertikal agar tidak terlalu cepat saat tidak ada input
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.9f, rb.velocity.z); // Mengurangi kecepatan vertikal
        }
        animator.SetBool("isSwimming", true);
    }

    void Climb()
    {
        if (Input.GetButtonDown("Jump")) // Menaikkan saat di tepi
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            isClimbing = false; // Stop climbing
            animator.SetBool("isClimbing", false);
        }
        animator.SetBool("isClimbing", true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            canWallJump = false;
            animator.SetBool("isGrounded", true);
            // Reset jumping animations when landing
            animator.SetBool("isJumping", false);
            animator.SetBool("isDoubleJumping", false);
            animator.SetBool("isTripleJumping", false);
            animator.SetBool("isWallJumping", false);
            animator.SetBool("isSideFlipping", false);
            animator.SetBool("isBackFlipping", false);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            animator.SetBool("isGrounded", false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isSwimming = true;
            animator.SetBool("isSwimming", true);
        }
        if (other.CompareTag("Climbable"))
        {
            isClimbing = true;
            animator.SetBool("isClimbing", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isSwimming = false;
            animator.SetBool("isSwimming", false);
        }
        if (other.CompareTag("Climbable"))
        {
            isClimbing = false;
            animator.SetBool("isClimbing", false);
        }
    }
}

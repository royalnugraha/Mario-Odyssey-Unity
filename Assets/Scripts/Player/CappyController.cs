using UnityEngine;

public class CappyController : MonoBehaviour
{
    public float throwSpeed = 10f;
    public float returnSpeed = 5f;
    public float captureRange = 3f;
    public LayerMask captureLayer;
    private bool isThrown;
    private bool isReturning;
    private Vector3 targetPosition;
    private Transform player;
    private Rigidbody rb;

    void Start()
    {
        player = transform.parent; // Assume Cappy is child of player
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isThrown)
        {
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isReturning = true;
                isThrown = false;
            }
        }
        else if (isReturning)
        {
            Vector3 returnDirection = (player.position - transform.position).normalized;
            rb.velocity = returnDirection * returnSpeed;

            if (Vector3.Distance(transform.position, player.position) < 0.5f)
            {
                isReturning = false;
                rb.velocity = Vector3.zero;
                transform.localPosition = Vector3.zero;
            }
        }
    }

    public void ThrowCap()
    {
        if (!isThrown && !isReturning)
        {
            isThrown = true;
            targetPosition = player.position + player.forward * throwSpeed;
            rb.velocity = (targetPosition - transform.position).normalized * throwSpeed;
        }
    }

    public void Capture()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, captureRange, captureLayer);
        foreach (var hitCollider in hitColliders)
        {
            // Capture Logic Here
            if (hitCollider.CompareTag("Captureable"))
            {
                // Implement capture mechanic
                Debug.Log("Captured: " + hitCollider.gameObject.name);
                // Additional logic to switch control to the captured object
            }
        }
    }

    public void SpinThrow()
    {
        // Execute a spin throw by rotating around the player and hitting nearby enemies
        Collider[] hitEnemies = Physics.OverlapSphere(player.position, captureRange, captureLayer);
        foreach (var enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Debug.Log("Hit enemy: " + enemy.name);
                // Implement enemy interaction logic
            }
        }
    }
}

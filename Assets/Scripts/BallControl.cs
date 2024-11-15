using UnityEngine;

public class BallControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float tiltSpeed = 800f;
    public float flickSpeed = 5f;
    public float jumpForce = 500f;
    public float maxVelocity = 10f;

    [Header("Game Settings")]
    public int maxLives = 3;
    public GameObject startPoint;
    public float respawnHeight = 3f;

    private Rigidbody rb;
    private Vector2 startTouch, endTouch;
    private int lives;
    private bool isGrounded;
    private bool isRespawning;
    private Vector3 lastValidPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.mass = 1f;
            rb.linearDamping = 1f;        // Using linearDamping instead of drag
            rb.angularDamping = 0.5f;     // Using angularDamping instead of angularDrag
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.useGravity = true;
        }
    }

    void Start()
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody is not assigned!");
            enabled = false;
            return;
        }

        if (startPoint == null)
        {
            Debug.LogError("StartPoint is not assigned!");
            enabled = false;
            return;
        }

        lives = maxLives;
        Input.gyro.enabled = true;
        ResetBall();
    }

    void FixedUpdate()
    {
        if (isRespawning) return;

        Vector3 tilt = Input.acceleration;
        Vector3 movement = new Vector3(tilt.x, 0, tilt.y);
        rb.AddForce(movement * tiltSpeed * Time.fixedDeltaTime, ForceMode.Force);

        // Limit velocity
        Vector3 currentVelocity = rb.linearVelocity;
        if (currentVelocity.magnitude > maxVelocity)
        {
            rb.linearVelocity = currentVelocity.normalized * maxVelocity;
        }

        // Store valid position when not falling
        if (isGrounded || transform.position.y > -10f)
        {
            lastValidPosition = transform.position;
        }

        // Emergency respawn if falling too far
        if (transform.position.y < -20f)
        {
            ForcedRespawn();
        }
    }

    void Update()
    {
        if (isRespawning) return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouch = touch.position;
                    break;

                case TouchPhase.Ended:
                    endTouch = touch.position;
                    Vector2 swipe = endTouch - startTouch;
                    if (swipe.magnitude > 50f)
                    {
                        Vector3 flickDirection = new Vector3(swipe.x, 0, swipe.y).normalized;
                        rb.AddForce(flickDirection * flickSpeed, ForceMode.Impulse);
                    }
                    break;
            }
        }
    }

    public void Jump()
    {
        if (isGrounded && !isRespawning)
        {
            // Set vertical velocity to zero before jumping
            Vector3 currentVelocity = rb.linearVelocity;
            currentVelocity.y = 0;
            rb.linearVelocity = currentVelocity;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Force);
            isGrounded = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.7f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FallZone") && !isRespawning)
        {
            lives--;
            if (lives >= 0)
            {
                ResetBall();
            }
            else
            {
                Debug.Log("Game Over!");
                // Add game over logic here
            }
        }
    }

    private void ResetBall()
    {
        isRespawning = true;

        // Disable physics and reset position
        rb.isKinematic = false;

        // Set position above start point
        Vector3 respawnPosition = startPoint.transform.position + Vector3.up * respawnHeight;
        transform.position = respawnPosition;

        // Re-enable physics after a delay
        Invoke("EnableBall", 0.2f);

        Debug.Log($"Lives Remaining: {lives}");
    }

    private void ForcedRespawn()
    {
        isRespawning = true;

        // Use last valid position if available, otherwise use start point
        Vector3 respawnPos = lastValidPosition != Vector3.zero ?
            lastValidPosition + Vector3.up * 2f :
            startPoint.transform.position + Vector3.up * respawnHeight;

        rb.isKinematic = true;
        transform.position = respawnPos;

        Invoke("EnableBall", 0.2f);
    }

    private void EnableBall()
    {
        // Re-enable physics
        rb.isKinematic = false;

        // Reset velocities after switching back to non-kinematic
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.WakeUp();
        isRespawning = false;
    }
}
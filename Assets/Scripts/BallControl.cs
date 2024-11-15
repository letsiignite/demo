using UnityEngine;

public class BallControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float tiltSpeed = 15f;
    public float flickSpeed = 8f;
    public float jumpForce = 300f;     // Increased jump force significantly
    public float maxVelocity = 10f;

    [Header("Game Settings")]
    public int maxLives = 3;
    public GameObject startPoint;
    public float respawnHeight = 1f;

    private Rigidbody rb;
    private Vector2 startTouch, endTouch;
    private int lives;
    private bool isGrounded;
    private bool isRespawning;
    private Vector3 lastValidPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.mass = 1f;
            rb.linearDamping = 1f;
            rb.angularDamping = 0.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.useGravity = true;
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

        // Tilt Movement
        Vector3 tilt = Input.acceleration;
        Vector3 movement = new Vector3(tilt.x, 0, tilt.y);
        rb.AddForce(movement * tiltSpeed, ForceMode.Force);

        // Velocity Limit
        Vector3 currentVelocity = rb.linearVelocity;
        if (currentVelocity.magnitude > maxVelocity)
        {
            rb.linearVelocity = currentVelocity.normalized * maxVelocity;
        }

        // Track last valid position
        if (isGrounded || transform.position.y > -10f)
        {
            lastValidPosition = transform.position;
        }

        // Check if ball has fallen below a certain height
        if (transform.position.y < -5f)  // Changed from -20f to -5f for quicker respawn
        {
            ResetBall();  // Changed from ForcedRespawn to ResetBall
        }
    }

    void Update()
    {
        if (isRespawning) return;

        // Flick/Swipe Control
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
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);  // Changed to ForceMode.Impulse
            isGrounded = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)  // Changed from 0.7f to 0.5f for better ground detection
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
            }
        }
    }

    private void ResetBall()
    {
        isRespawning = true;

        // Disable physics temporarily
        rb.isKinematic = true;

        // Reset position above start point
        transform.position = startPoint.transform.position + Vector3.up * respawnHeight;

        // Reset velocities
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Re-enable physics after a short delay
        Invoke("EnableBall", 0.1f);

        Debug.Log($"Lives Remaining: {lives}");
    }

    private void EnableBall()
    {
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.WakeUp();
        isRespawning = false;
    }
}
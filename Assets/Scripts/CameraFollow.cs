using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;  // Ball (Sphere) to follow
    public Vector3 offset = new Vector3(0, 5, -7);  // Camera position offset
    public float smoothSpeed = 0.125f;  // Smooth follow speed

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
            transform.LookAt(target);
        }
    }
}

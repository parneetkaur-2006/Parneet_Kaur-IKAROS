using UnityEngine;

public class CameraFollowAdvanced : MonoBehaviour
{
    public Transform player;

    public float smoothSpeed = 5f;
    public Vector3 offset;

    public bool followX = true;
    public bool followY = true;

    public float fallSpeedMultiplier = 2f; // faster camera when falling

    private float currentVelocityY = 0f;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPosition = transform.position;

        // Follow X
        if (followX)
            targetPosition.x = player.position.x + offset.x;

        // Follow Y (smooth + better for jumps/falls)
        if (followY)
        {
            float targetY = player.position.y + offset.y;

            // If player is falling → camera faster
            float speed = (player.GetComponent<Rigidbody2D>().linearVelocity.y < 0)
                ? smoothSpeed * fallSpeedMultiplier
                : smoothSpeed;

            targetPosition.y = Mathf.SmoothDamp(
                transform.position.y,
                targetY,
                ref currentVelocityY,
                1f / speed
            );
        }

        targetPosition.z = offset.z;

        transform.position = targetPosition;
    }
}

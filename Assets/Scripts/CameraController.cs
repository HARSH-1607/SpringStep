using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;

    [Header("Camera Limits")]
    // The lowest Y position the camera is allowed to go.
    // This acts as the "bottom edge" of your screen.
    public float minY = 0f; 

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPosition = transform.position;

            // 1. Follow Player X
            newPosition.x = player.position.x;

            // 2. Follow Player Y, but NEVER go below 'minY'
            // This effectively "locks" the view from seeing anything below this point.
            newPosition.y = Mathf.Max(player.position.y, minY);

            // 3. Keep Z the same
            newPosition.z = -10f; 

            transform.position = newPosition;
        }
    }
}
using UnityEngine;

/// <summary>
/// Rotates the camera around a target object when enabled. 
/// Useful for showcasing objects from multiple angles in a hands-free orbiting view.
/// </summary>
public class CameraOrbit : MonoBehaviour
{
    [Tooltip("The target object the camera should orbit around.")]
    public Transform target;

    [Tooltip("Speed of the orbit rotation in degrees per second.")]
    public float orbitSpeed = 10f;

    // Flag to track whether camera orbiting is active
    private bool isRotating = false;

    /// <summary>
    /// Rotates the camera around the target on the Y axis when enabled.
    /// Called every frame by Unity.
    /// </summary>
    void Update()
    {
        if (isRotating && target != null)
        {
            transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Enables or disables the orbiting motion.
    /// Called by the toggle UI using a dynamic boolean binding.
    /// </summary>
    /// <param name="enabled">True to enable orbiting, false to stop.</param>
    public void ToggleRotation(bool enabled)
    {
        isRotating = enabled;
    }
}

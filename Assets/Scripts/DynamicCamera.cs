using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class DynamicCamera : MonoBehaviour
{
    public static DynamicCamera Instance;

    [Header("Camera Targets")]
    public List<Transform> targets = new List<Transform>();

    [Header("Position Settings")]
    public Vector3 offset = new Vector3(0f, 10f, -10f); // How far back and up the camera sits
    public float smoothTime = 0.5f; // How quickly it moves to the new position

    [Header("Zoom Settings")]
    public float maxZoomFOV = 10f;  // Most zoomed IN (players are close)
    public float minZoomFOV = 60f;  // Most zoomed OUT (players are far apart)
    public float zoomLimiter = 30f; // How far apart players need to be to reach max zoom out

    private Vector3 velocity;
    private Camera cam;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
    }

    // Players will call this when they spawn into the game
    public void AddPlayer(Transform playerTransform)
    {
        if (!targets.Contains(playerTransform))
        {
            targets.Add(playerTransform);
        }
    }

    // We use LateUpdate for cameras so it moves AFTER the players have moved this frame
    void LateUpdate()
    {
        if (targets.Count == 0) return;

        MoveCamera();
        ZoomCamera();
    }

    void MoveCamera()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = centerPoint + offset;

        // SmoothDamp glides the camera smoothly rather than snapping it instantly
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    void ZoomCamera()
    {
        float greatestDistance = GetGreatestDistance();

        // Calculate the target FOV based on how far apart the players are
        float targetZoom = Mathf.Lerp(maxZoomFOV, minZoomFOV, greatestDistance / zoomLimiter);

        // Smoothly transition the camera's Field of View
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetZoom, Time.deltaTime * 5f);
    }

    float GetGreatestDistance()
    {
        // Draw an invisible box around the first player
        Bounds bounds = new Bounds(targets[0].position, Vector3.zero);

        // Stretch the box to include every other player
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        // Return the largest width of the box
        return bounds.size.x > bounds.size.z ? bounds.size.x : bounds.size.z;
    }

    Vector3 GetCenterPoint()
    {
        // If there's only one player, just look at them
        if (targets.Count == 1) return targets[0].position;

        // Otherwise, find the exact middle of our invisible box
        Bounds bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.center;
    }
}
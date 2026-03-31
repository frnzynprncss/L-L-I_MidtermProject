using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class DynamicCamera : MonoBehaviour
{
    public static DynamicCamera Instance;

    public bool isTracking = false;

    [Header("Camera Targets")]
    public List<Transform> targets = new List<Transform>();

    [Header("Position Settings")]
    public Vector3 offset = new Vector3(0f, 10f, -10f);
    public float smoothTime = 0.5f;

    [Header("Zoom Settings")]
    public float maxZoomFOV = 10f;
    public float minZoomFOV = 60f;
    public float zoomLimiter = 30f;

    // ---> NEW: Your exact Cinematic Coordinates! <---
    [Header("Cinematic Settings")]
    public Vector3 cinematicPosition = new Vector3(-2.4f, 78.8f, -85.6f);
    public Quaternion cinematicRotation = new Quaternion(0.3851689f, -0.06096244f, -0.02938374f, 0.9203615f);

    private Vector3 velocity;
    private Camera cam;
    private Quaternion gameplayRotation;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();

        // Remember the rotation you set in the Unity Editor for actual gameplay!
        gameplayRotation = Quaternion.Euler(41.843f, 0f, 0f);
    }

    // MatchFlowManager will call this to snap the camera to the cinematic spot
    public void SnapToCinematicView()
    {
        isTracking = false;
        //transform.position = cinematicPosition;
        //transform.rotation = cinematicRotation;
    }

    public void AddPlayer(Transform playerTransform)
    {
        if (!targets.Contains(playerTransform))
        {
            targets.Add(playerTransform);
        }
    }

    void LateUpdate()
    {
        targets.RemoveAll(t => t == null);

        if (targets.Count == 0 || !isTracking) return;

        MoveCamera();
        ZoomCamera();
    }

    void MoveCamera()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = centerPoint + offset;

        // Smoothly glide the position...
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);

        // ---> NEW: Smoothly tilt the rotation back to normal gameplay mode! <---
        transform.rotation = Quaternion.Slerp(transform.rotation, gameplayRotation, Time.deltaTime * 2f);
    }

    void ZoomCamera()
    {
        float greatestDistance = GetGreatestDistance();
        float targetZoom = Mathf.Lerp(maxZoomFOV, minZoomFOV, greatestDistance / zoomLimiter);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetZoom, Time.deltaTime * 5f);
    }

    float GetGreatestDistance()
    {
        Bounds bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }
        return bounds.size.x > bounds.size.z ? bounds.size.x : bounds.size.z;
    }

    Vector3 GetCenterPoint()
    {
        if (targets.Count == 1) return targets[0].position;

        Bounds bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.center;
    }

    public void CameraStart()
    {
        transform.position = new Vector3(-20.09115f, 66.5f, -76.1f);
        transform.rotation = Quaternion.Euler(41.843f,0,0);
    }
}
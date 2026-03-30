using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    [Header("Vertical Motion")]
    public float verticalRange = 1.0f;
    public float verticalSpeed = 2.0f;

    [Header("Horizontal Toggle")]
    public Transform pointA;
    public Transform pointB;

    private bool movingToB = false;
    private float startY;
    private float lastHeight;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        // 1. Calculate the new Vertical Position using a Sine wave
        float newY = startY + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;

        // 2. Detect the "Apex" (When the cloud starts moving down again)
        // This is where we trigger the horizontal switch
        if (newY < lastHeight && !movingToB)
        {
            // Just reached the top, switch to B
            SwitchPosition(pointB.position.x);
            movingToB = true;
        }
        else if (newY > lastHeight && movingToB)
        {
            // Just reached the bottom, switch back to A
            SwitchPosition(pointA.position.x);
            movingToB = false;
        }

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        lastHeight = newY;
    }

    void SwitchPosition(float targetX)
    {
        Vector3 pos = transform.position;
        pos.x = targetX;
        transform.position = pos;
    }
}
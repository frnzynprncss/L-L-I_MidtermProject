using UnityEngine;
using Tanks.Complete; // Important: This lets this script see CameraControl

public class CameraTargetLink : MonoBehaviour
{
    private void OnEnable()
    {
        if (CameraControl.instance != null)
            CameraControl.instance.AddTarget(this.transform);
    }

    private void OnDisable()
    {
        if (CameraControl.instance != null)
            CameraControl.instance.RemoveTarget(this.transform);
    }
}
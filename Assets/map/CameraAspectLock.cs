using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAspectLock : MonoBehaviour
{
    public float targetAspect = 16f / 9f;
    public float orthographicSize = 5f;

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        cam.orthographic = true;

        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            cam.orthographicSize = orthographicSize / scaleHeight;
        }
        else
        {
            cam.orthographicSize = orthographicSize;
        }
    }
}

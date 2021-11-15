using UnityEngine;

[ExecuteAlways]
public class CameraScaler : MonoBehaviour
{
    [SerializeField] private Camera cameraToScale;
    [SerializeField] private Vector2 worldSize = new Vector2(10,20);

    private void Awake() => ScaleCamera(worldSize);

#if UNITY_EDITOR
    private void Update() => ScaleCamera(worldSize);
#endif

    /// <summary>
    /// Scales the orthographic size of the camera so it's always showing the defined world size.
    /// </summary>
    public void ScaleCamera(Vector2 worldSize)
    {
        if (cameraToScale == null)
            return;

        float aspectRatio = (float)Screen.height / Screen.width;

        // Either set to max width or max height
        if (aspectRatio >= (float)worldSize.y / worldSize.x)
        {
            cameraToScale.orthographicSize = aspectRatio * worldSize.x / 2;
        }
        else
        {
            cameraToScale.orthographicSize = worldSize.y / 2;
        }

    }
}

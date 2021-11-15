using UnityEngine;

[ExecuteAlways]
public class CameraScaler : MonoBehaviour
{
    private void Awake()
    {
        ScaleCamera();
    }
#if UNITY_EDITOR
    private void Update()
    {
        ScaleCamera();
    }
#endif


    private void ScaleCamera()
    {
        float aspectRatio = (float)Screen.height / Screen.width;

        if (aspectRatio >= (float)Constants.GRID_HEGIHT / Constants.GRID_WIDTH)
        {
            Camera.main.orthographicSize = aspectRatio * Constants.GRID_WIDTH / 2;
        }
        else
        {
            Camera.main.orthographicSize = Constants.GRID_HEGIHT / 2;
        }

    }
}

using UnityEngine;

namespace Util
{
#if UNITY_EDITOR
    [ExecuteAlways]
#endif
    public class CameraScaler : MonoBehaviour
    {
        [SerializeField] private Camera cameraToScale;

        [SerializeField] [Tooltip("The size of the world the camera needs to display in world units")]
        private Vector2 worldSize = new Vector2(10, 20);
        
        [SerializeField] [Tooltip("Additional offset that is applied to the camera position in world units")]
        private Vector3 offset = new Vector3(-0.5f, -0.5f, 0.0f);

        [SerializeField] [Tooltip("The margin placed on the top in world units")]
        private float marginTop = 0.0f;

        [SerializeField] [Tooltip("The margin placed on the bottom in world units")]
        private float marginBottom = 0.0f;

        [SerializeField] [Tooltip("The margin placed on the left in world units")]
        private float marginLeft = 0.0f;

        [SerializeField] [Tooltip("The margin placed on the right in world units")]
        private float marginRight = 0.0f;

        private void Awake() => ScaleCamera();

#if UNITY_EDITOR
        private void Update() => ScaleCamera();
#endif

        /// <summary>
        /// Scales the orthographic size of the camera so it's always showing the defined world size.
        /// </summary>
        private void ScaleCamera()
        {
            if (cameraToScale == null)
                return;

            float aspectRatio = (float)Screen.height / Screen.width;

            // Either set to max width or max height
            if (aspectRatio >= (worldSize.y + marginTop + marginBottom) / (worldSize.x + marginLeft + marginRight))
            {
                cameraToScale.orthographicSize = aspectRatio * (worldSize.x + marginLeft + marginRight) / 2.0f;
            }
            else
            {
                cameraToScale.orthographicSize = (worldSize.y + marginTop + marginBottom) / 2;
            }

            cameraToScale.transform.position = new Vector3(worldSize.x / 2.0f + (marginRight - marginLeft) / 2.0f,
                worldSize.y / 2.0f + (marginTop - marginBottom) / 2.0f, -10.0f) + offset;
        }
    }
}
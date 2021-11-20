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
        private Vector2 offset = new Vector3(-0.5f, -0.5f);

        [SerializeField] [Tooltip("Whether to additionally apply the safe area")]
        private bool applySafeArea;

        [Header("Margins")] [SerializeField] [Tooltip("The margin placed on the top in world units")]
        private float marginTop = 0.0f;

        [SerializeField] [Tooltip("The margin placed on the bottom in world units")]
        private float marginBottom = 0.0f;

        [SerializeField] [Tooltip("The margin placed on the left in world units")]
        private float marginLeft = 0.0f;

        [SerializeField] [Tooltip("The margin placed on the right in world units")]
        private float marginRight = 0.0f;
        
        // For debugging
        private float _left;
        private float _right;
        private float _top;
        private float _bottom;
        private Vector2 _safeAreaPosition;
        private Vector2 _safeAreaSize;
        private Vector2 _cameraSizeWorldUnits;
        private Vector2 _world2Pixel;

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

            _left = marginLeft;
            _right = marginRight;
            _top = marginTop;
            _bottom = marginBottom;

            float verticalWorldSize = worldSize.y + _top + _bottom;
            float horizontalWorldSize = worldSize.x + _left + _right;

            float aspectRatio = (float) Screen.height / Screen.width;

            // Either set to max width or max height
            if (aspectRatio >= verticalWorldSize / horizontalWorldSize)
            {
                cameraToScale.orthographicSize = aspectRatio * (horizontalWorldSize / 2.0f);
            }
            else
            {
                cameraToScale.orthographicSize = verticalWorldSize / 2.0f;
            }

            if (applySafeArea)
            {
                // Calculate camera size in world units (after applying margins)
                _cameraSizeWorldUnits.y = cameraToScale.orthographicSize * 2;
                _cameraSizeWorldUnits.x = _cameraSizeWorldUnits.y * Screen.width / Screen.height;

                _world2Pixel.x = Screen.width / _cameraSizeWorldUnits.x;
                _world2Pixel.y = Screen.height / _cameraSizeWorldUnits.y;
                
                Rect safeArea = Screen.safeArea;
                // Convert safe area to world units
                _safeAreaPosition = safeArea.position / _world2Pixel;
                _safeAreaSize = safeArea.size / _world2Pixel;

                // Add safe area to margins
                _left += _safeAreaPosition.x;
                _right += _cameraSizeWorldUnits.x - (_safeAreaPosition.x + _safeAreaSize.x);
                _top += _cameraSizeWorldUnits.y - (_safeAreaPosition.y + _safeAreaSize.y);
                _bottom += _safeAreaPosition.y;

                
                verticalWorldSize = worldSize.y + _top + _bottom;
                horizontalWorldSize = worldSize.x + _left + _right;
                
                // Re-check if horizontal or vertical should be used
                if (aspectRatio >= verticalWorldSize / horizontalWorldSize)
                {
                    cameraToScale.orthographicSize = aspectRatio * (horizontalWorldSize / 2.0f);
                }
                else
                {
                    cameraToScale.orthographicSize = verticalWorldSize / 2.0f;
                }
            }
            
            // Move camera position depending on margins and offset
            cameraToScale.transform.position = new Vector3(worldSize.x / 2.0f + (_right - _left) / 2.0f,
                worldSize.y / 2.0f + (_top - _bottom) / 2.0f, -10.0f) + (Vector3)offset;
        }
    }
}
using UnityEngine;

namespace Util
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;

        [SerializeField] private Mode mode;

        public enum Mode
        {
            Normal,
            Top,
            Bottom,
            Left,
            Right
        }

        private void Awake()
        {
            if (canvas == null)
            {
                Debug.LogError("No canvas parent selected!");
                return;
            }

            ApplySafeArea();
        }

#if UNITY_EDITOR
        private void Update()
        {
            ApplySafeArea();
        }
#endif
        private void ApplySafeArea()
        {
            if (canvas == null)
                return;

            Rect safeArea = Screen.safeArea;

            Vector2 anchorMin;
            Vector2 anchorMax;

            float width = canvas.pixelRect.width;
            float height = canvas.pixelRect.height;

            switch (mode)
            {
                case Mode.Normal:
                    anchorMin = safeArea.position;
                    anchorMax = anchorMin + safeArea.size;

                    anchorMin.x /= width;
                    anchorMax.x /= width;
                    anchorMin.y /= height;
                    anchorMax.y /= height;
                    break;
                
                case Mode.Top:
                    anchorMin = new Vector2(0, (safeArea.position.y + safeArea.size.y) / height);
                    anchorMax = Vector2.one;
                    break;
                
                case Mode.Bottom:
                    anchorMin = Vector2.zero;
                    anchorMax = new Vector2(1, safeArea.position.y / height);
                    break;

                case Mode.Left:
                    anchorMin = new Vector2(0, 0);
                    anchorMax = new Vector2(safeArea.position.x / width, 1);
                    break;
                
                case Mode.Right:
                    anchorMin = new Vector2((safeArea.position.x + safeArea.size.x) / width, 0);
                    anchorMax = new Vector2(1, 1);
                    break;
                
                default:
                    anchorMin = Vector2.zero;
                    anchorMax = Vector2.zero;
                    break;
            }

            ((RectTransform) transform).anchorMin = anchorMin;
            ((RectTransform) transform).anchorMax = anchorMax;
        }
    }
}
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    [SerializeField] private readonly Canvas canvas;

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

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = anchorMin + safeArea.size;
        float width = canvas.pixelRect.width;
        float height = canvas.pixelRect.height;

        anchorMin.x /= width;
        anchorMin.y /= height;
        anchorMax.x /= width;
        anchorMax.y /= height;

        ((RectTransform)this.transform).anchorMin = anchorMin;
        ((RectTransform)this.transform).anchorMax = anchorMax;
    }
}

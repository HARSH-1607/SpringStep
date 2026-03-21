using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class UIResponsiveFixer : MonoBehaviour
{
    private void Start()
    {
        // 1. Fix Canvas Scaler
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); // Standard HD
            scaler.matchWidthOrHeight = 0.5f; // Balance between width and height
        }

        // 2. Fix Background Panel Stretching
        // Look for a child specifically named "Background" or "Panel" and stretch it
        foreach (Transform child in transform)
        {
            if (child.name.ToLower().Contains("background") || child.name.ToLower().Contains("panel"))
            {
                StretchRect(child.GetComponent<RectTransform>());
            }
        }
    }

    private void StretchRect(RectTransform rect)
    {
        if (rect == null) return;
        
        rect.anchorMin = Vector2.zero; // Bottom-Left (0,0)
        rect.anchorMax = Vector2.one;  // Top-Right (1,1)
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero; // No padding
        rect.offsetMax = Vector2.zero;
        
        // Ensure z-position is zero just in case
        rect.anchoredPosition3D = Vector3.zero;
    }
}

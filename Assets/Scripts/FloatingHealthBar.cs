using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    private Transform target;
    private Vector3 offset = new Vector3(0, 1.2f, 0); // Above player
    private Image foregroundImage;
    private Image backgroundImage;
    private Slider healthSlider;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
    }

    private void Start()
    {
        // 1. Create Canvas
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(this.transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<CanvasScaler>(); // Optional but good practice

        // Size the canvas small
        RectTransform canvasRT = canvasObj.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(1.5f, 0.3f); 
        canvasRT.localPosition = Vector3.zero;

        // 2. Create Slider Structure
        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform);
        backgroundImage = bgObj.AddComponent<Image>();
        backgroundImage.color = Color.black;
        RectTransform bgRT = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // Fill Area
        GameObject fillAreaObj = new GameObject("FillArea");
        fillAreaObj.transform.SetParent(bgObj.transform);
        RectTransform fillAreaRT = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero;
        fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = new Vector2(0.05f, 0.05f); // Tiny padding
        fillAreaRT.offsetMax = new Vector2(-0.05f, -0.05f);

        // Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform);
        foregroundImage = fillObj.AddComponent<Image>();
        foregroundImage.color = Color.red;
        RectTransform fillRT = fillObj.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        // Slider Component
        healthSlider = bgObj.AddComponent<Slider>();
        healthSlider.fillRect = fillRT;
        healthSlider.targetGraphic = backgroundImage;
        healthSlider.direction = Slider.Direction.LeftToRight;
        healthSlider.minValue = 0;
        healthSlider.maxValue = 1;
        healthSlider.value = 1; // Start full
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.rotation = Quaternion.identity; // Lock rotation
        }
        else
        {
            Destroy(gameObject); // Self destruct if target lost
        }
    }
}

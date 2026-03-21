/*
 * BossHealthBarUI.cs
 * Auto-creates a cinematic boss health bar at the top of the screen.
 * Attach to any GameObject — it builds its own Canvas/UI at runtime.
 */

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("Settings")]
    public string bossName = "BOSS";
    public float lerpSpeed = 5f;

    private Slider healthSlider;
    private float targetValue = 1f;
    private GameObject barRoot;
    private TextMeshProUGUI nameText;

    private void Start()
    {
        CreateUI();
        Hide(); // Hidden until arena triggers it
    }

    private void Update()
    {
        // Smooth health bar animation
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetValue, Time.deltaTime * lerpSpeed);
        }
    }

    public void UpdateHealth(int current, int max)
    {
        targetValue = (float)current / max;
        if (targetValue < 0f) targetValue = 0f;
    }

    public void Show()
    {
        if (barRoot != null) barRoot.SetActive(true);
    }

    public void Hide()
    {
        if (barRoot != null) barRoot.SetActive(false);
    }

    public void SetBossName(string name)
    {
        bossName = name;
        if (nameText != null) nameText.text = name;
    }

    private void CreateUI()
    {
        // --- Canvas ---
        barRoot = new GameObject("BossHealthBarCanvas");
        barRoot.transform.SetParent(transform);
        Canvas canvas = barRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        barRoot.AddComponent<CanvasScaler>();
        barRoot.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = barRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // --- Bar Container (top of screen) ---
        GameObject container = CreateRect("BarContainer", barRoot.transform);
        RectTransform containerRT = container.GetComponent<RectTransform>();
        containerRT.anchorMin = new Vector2(0.15f, 0.92f);
        containerRT.anchorMax = new Vector2(0.85f, 0.96f);
        containerRT.offsetMin = Vector2.zero;
        containerRT.offsetMax = Vector2.zero;

        // --- Background ---
        GameObject bg = CreateRect("Background", container.transform);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        RectTransform bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // --- Border ---
        GameObject border = CreateRect("Border", container.transform);
        Image borderImage = border.AddComponent<Image>();
        borderImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        Outline borderOutline = border.AddComponent<Outline>();
        borderOutline.effectColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        borderOutline.effectDistance = new Vector2(2, 2);
        RectTransform borderRT = border.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = Vector2.zero;
        borderRT.offsetMax = Vector2.zero;
        borderImage.color = Color.clear; // Invisible center, outline visible

        // --- Fill Area ---
        GameObject fillArea = CreateRect("FillArea", container.transform);
        RectTransform fillAreaRT = fillArea.GetComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0.005f, 0.1f);
        fillAreaRT.anchorMax = new Vector2(0.995f, 0.9f);
        fillAreaRT.offsetMin = Vector2.zero;
        fillAreaRT.offsetMax = Vector2.zero;

        // --- Fill ---
        GameObject fill = CreateRect("Fill", fillArea.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.85f, 0.15f, 0.15f, 1f); // Deep red
        RectTransform fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        // --- Slider Component ---
        healthSlider = container.AddComponent<Slider>();
        healthSlider.fillRect = fillRT;
        healthSlider.targetGraphic = fillImage;
        healthSlider.direction = Slider.Direction.LeftToRight;
        healthSlider.minValue = 0;
        healthSlider.maxValue = 1;
        healthSlider.value = 1;
        healthSlider.interactable = false;

        // --- Boss Name Text (Below the bar) ---
        GameObject textObj = CreateRect("BossName", container.transform);
        nameText = textObj.AddComponent<TextMeshProUGUI>();
        nameText.text = bossName;
        nameText.fontSize = 20;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;
        nameText.fontStyle = FontStyles.Bold;
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0f, -1.2f);
        textRT.anchorMax = new Vector2(1f, -0.1f);
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
    }

    private GameObject CreateRect(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.AddComponent<RectTransform>();
        return obj;
    }
}

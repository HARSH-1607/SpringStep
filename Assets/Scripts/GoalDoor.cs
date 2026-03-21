using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class GoalDoor : MonoBehaviour
{
    [Header("Animations")]
    [Tooltip("Drag the 5 opening frames here")]
    public Sprite[] openingSprites;
    
    [Tooltip("Drag the 3 closing frames here")]
    public Sprite[] closingSprites;
    
    [Tooltip("Time between frames in seconds")]
    public float frameRate = 0.1f;
    
    [Header("UI")]
    [Tooltip("Optional: Assign a custom prompt. If left empty, one will be auto-created.")]
    public GameObject interactPromptUI;

    [Header("Prompt Settings")]
    public string promptText = "Press E to go to next level";
    public float promptOffsetY = 1.5f;
    public float promptBobAmount = 0.15f;
    public float promptBobSpeed = 2f;
    
    [Header("Settings")]
    public string nextLevelName = "NextLevelNameHere";

    private SpriteRenderer spriteRenderer;
    private bool isPlayerNear = false;
    private Coroutine activeCoroutine;
    private bool autoCreatedPrompt = false;
    private Vector3 promptBasePosition;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Start closed
        if (spriteRenderer != null)
        {
            if (closingSprites != null && closingSprites.Length > 0)
                spriteRenderer.sprite = closingSprites[closingSprites.Length - 1];
            else if (openingSprites != null && openingSprites.Length > 0)
                spriteRenderer.sprite = openingSprites[0];
        }

        // Auto-create floating text prompt if none assigned
        if (interactPromptUI == null)
        {
            CreateFloatingPrompt();
        }

        if (interactPromptUI != null) interactPromptUI.SetActive(false);
    }

    private void CreateFloatingPrompt()
    {
        GameObject promptObj = new GameObject("DoorPrompt");
        promptObj.transform.SetParent(transform);
        promptObj.transform.localPosition = new Vector3(0f, promptOffsetY, 0f);

        TextMeshPro tmp = promptObj.AddComponent<TextMeshPro>();
        tmp.text = promptText;
        tmp.fontSize = 3f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.sortingOrder = 10;

        // Auto-size the rect to fit the text
        RectTransform rect = promptObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(6f, 1.5f);

        interactPromptUI = promptObj;
        autoCreatedPrompt = true;
        promptBasePosition = promptObj.transform.localPosition;
    }

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            EnterDoor();
        }

        // Subtle bobbing animation for the prompt
        if (autoCreatedPrompt && interactPromptUI != null && interactPromptUI.activeSelf)
        {
            float bob = Mathf.Sin(Time.time * promptBobSpeed) * promptBobAmount;
            interactPromptUI.transform.localPosition = promptBasePosition + new Vector3(0f, bob, 0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            PlayAnimation(openingSprites);
            if (interactPromptUI != null) interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            PlayAnimation(closingSprites);
            if (interactPromptUI != null) interactPromptUI.SetActive(false);
        }
    }

    private void PlayAnimation(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0) return;

        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(AnimateSprite(frames));
    }

    private IEnumerator AnimateSprite(Sprite[] frames)
    {
        for (int i = 0; i < frames.Length; i++)
        {
            spriteRenderer.sprite = frames[i];
            yield return new WaitForSeconds(frameRate);
        }
    }

    private void EnterDoor()
    {
        Debug.Log("Entered Goal Door! Loading next level...");
        SceneManager.LoadScene(nextLevelName);
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

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
    [Tooltip("Assign a UI Text object here that says 'Press E to Enter'")]
    public GameObject interactPromptUI;
    
    [Header("Settings")]
    public string nextLevelName = "NextLevelNameHere";

    private SpriteRenderer spriteRenderer;
    private bool isPlayerNear = false;
    private Coroutine activeCoroutine;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Start closed (using the last frame of closing, or the first frame of opening if closing is missing)
        if (spriteRenderer != null)
        {
            if (closingSprites != null && closingSprites.Length > 0)
                spriteRenderer.sprite = closingSprites[closingSprites.Length - 1]; // Use last closed frame as idle
            else if (openingSprites != null && openingSprites.Length > 0)
                spriteRenderer.sprite = openingSprites[0];
        }

        if (interactPromptUI != null) interactPromptUI.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            EnterDoor();
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

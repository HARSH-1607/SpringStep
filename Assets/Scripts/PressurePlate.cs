using UnityEngine;
using System.Collections;

public class PressurePlate : MonoBehaviour
{
    [Header("Trampoline Settings")]
    public float bounceForce = 15f;

    [Header("Visuals")]
    [Tooltip("Idle state (Button Up)")]
    public Sprite unpressedSprite;
    
    [Tooltip("Animation when stepping ON (Button going Down)")]
    public Sprite[] pressingSprites;
    
    [Tooltip("Animation when stepping OFF (Button coming Up)")]
    public Sprite[] releasingSprites;
    
    public float frameRate = 0.05f;

    private SpriteRenderer spriteRenderer;
    private Coroutine activeCoroutine;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && unpressedSprite != null)
        {
            spriteRenderer.sprite = unpressedSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Visual feedback: Animate pressing
            PlayAnimation(pressingSprites);

            // Logic: Bounce Player
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Bounce(bounceForce);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Visual feedback: Animate releasing
            PlayAnimation(releasingSprites);
        }
    }

    private void PlayAnimation(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0) return;
        
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(AnimateSequence(frames));
    }

    private IEnumerator AnimateSequence(Sprite[] frames)
    {
        for (int i = 0; i < frames.Length; i++)
        {
            spriteRenderer.sprite = frames[i];
            yield return new WaitForSeconds(frameRate);
        }
        
        // If this was the releasing animation, ensure we end on the explicit unpressed sprite if desired
        // But usually the last frame of release IS the unpressed sprite. 
        // We'll leave it at the last frame.
    }
}
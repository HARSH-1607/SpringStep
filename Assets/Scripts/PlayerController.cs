/*
* PlayerController.cs
* * Removed: Coin Pickup Animation/Prefab logic.
* * Kept: Everything else (Movement, Double Jump, Health, Dash, etc.)
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Animator))] 
public class PlayerController : MonoBehaviour
{
    // --- Public Variables ---

    [Header("Health Settings")]
    public int maxHealth = 3;
    public float invincibilityDuration = 1.5f;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float jumpCutMultiplier = 0.5f;
    public int extraJumpsValue = 1;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winTextObject;
    public GameObject restartButtonObject;
    public List<Image> healthHearts;

    [Header("Audio Clips")]
    public AudioClip jumpSound;
    public AudioClip coinSound;
    public AudioClip winSound;
    public AudioClip dashSound;
    public AudioClip hurtSound;

    // --- Private Variables ---

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Animator animator; 
    private SpriteRenderer spriteRenderer; 

    private int currentHealth;
    private bool isInvincible = false;
    private bool isGrounded;
    private int score = 0;
    private bool isDashing = false;
    private bool canDash = true;
    private int extraJumps;
    private float originalGravityScale;
    private float lastMoveDirection = 1f;
    private FloatingHealthBar floatingHealthBar; // [NEW] Reference to the floating health bar

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>(); 
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        
        rb.freezeRotation = true; 
        originalGravityScale = rb.gravityScale;
        currentHealth = maxHealth;
        extraJumps = extraJumpsValue;
        Time.timeScale = 1f;

        // [NEW] Instantiate and setup Floating Health Bar
        GameObject healthBarObj = new GameObject("FloatingHealthBar");
        floatingHealthBar = healthBarObj.AddComponent<FloatingHealthBar>();
        floatingHealthBar.SetTarget(transform);
        floatingHealthBar.UpdateHealth(currentHealth, maxHealth);

        UpdateScoreText();
        UpdateHealthUI();
    }

    void Update()
    {
        if (isDashing) return;

        // --- Sideways Movement ---
        float moveInput = Input.GetAxis("Horizontal");

        if (moveInput > 0)
        {
            spriteRenderer.flipX = false;
            lastMoveDirection = 1f;
        }
        else if (moveInput < 0)
        {
            spriteRenderer.flipX = true;
            lastMoveDirection = -1f;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetFloat("yVelocity", rb.linearVelocity.y);
        animator.SetBool("IsGrounded", isGrounded);

        // --- Jumping ---
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                Jump(false);
            }
            else if (extraJumps > 0)
            {
                Jump(true);
                extraJumps--; 
            }
        }

        // --- Jump Cut ---
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        // --- Dashing ---
        if (Input.GetKeyDown(KeyCode.V) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    void Jump(bool isDoubleJump)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        if (jumpSound != null) { audioSource.PlayOneShot(jumpSound); }

        if (isDoubleJump)
        {
            animator.SetTrigger("DoubleJump");
        }
        else
        {
            animator.SetTrigger("Jump");
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        if (dashSound != null) { audioSource.PlayOneShot(dashSound); }

        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(lastMoveDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        rb.gravityScale = originalGravityScale;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        UpdateHealthUI();
        
        // [NEW] Update Floating Health Bar
        if (floatingHealthBar != null)
        {
            floatingHealthBar.UpdateHealth(currentHealth, maxHealth);
        }
        
        if (hurtSound != null) audioSource.PlayOneShot(hurtSound);

        animator.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            StartCoroutine(BecomeInvincible());
        }
    }

    private IEnumerator BecomeInvincible()
    {
        isInvincible = true;
        for (int i = 0; i < 5; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(invincibilityDuration / 10f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(invincibilityDuration / 10f);
        }
        isInvincible = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) 
        {
            isGrounded = true;
            extraJumps = extraJumpsValue;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) isGrounded = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            // Just destroy the coin object directly
            Destroy(other.gameObject);
            
            // Use GameManager for score
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(1);
            }
            else
            {
                Debug.LogWarning("GameManager missing! Creating temporary one for this scene.");
                GameObject gm = new GameObject("GameManager");
                gm.AddComponent<GameManager>();
                GameManager.Instance.AddScore(1);
            }

            UpdateScoreText();
            
            // The sound still plays
            if (coinSound != null) audioSource.PlayOneShot(coinSound);
        }
        else if (other.gameObject.CompareTag("DeathZone"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if (other.gameObject.CompareTag("Spike"))
        {
            TakeDamage(1);
        }
        else if (other.gameObject.CompareTag("Goal"))
        {
            // [MODIFIED] Disabled for new "GoalDoor" logic
            /*
            winTextObject.text = "You Win!\nScore: " + score;
            winTextObject.gameObject.SetActive(true);
            restartButtonObject.SetActive(true);
            Time.timeScale = 0f;
            if (winSound != null) audioSource.PlayOneShot(winSound);
            */
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            int currentScore = (GameManager.Instance != null) ? GameManager.Instance.TotalScore : 0;
            scoreText.text = "Score: " + currentScore;
        }
    }

    void UpdateHealthUI()
    {
        for (int i = 0; i < healthHearts.Count; i++)
        {
            if (i < currentHealth) healthHearts[i].enabled = true;
            else healthHearts[i].enabled = false;
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // [NEW] Trampoline Bounce
    public void Bounce(float bounceForce)
    {
        // Reset vertical velocity to ensure consistent bounce height
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        
        // Apply impulse force
        rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
        
        // Reset jump states
        isGrounded = false;
        animator.SetBool("IsGrounded", false);
        animator.SetTrigger("Jump");
        
        // Optional: restore double jumps
        extraJumps = extraJumpsValue;
    }
}
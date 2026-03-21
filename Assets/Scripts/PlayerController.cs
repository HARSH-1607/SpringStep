/*
* PlayerController.cs
* * Removed: Coin Pickup Animation/Prefab logic.
* * Kept: Everything else (Movement, Double Jump, Health, Dash, Attack, etc.)
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
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    [Header("Attack Settings")]
    public int attackDamage = 1;
    public float attackRange = 0.8f;
    public float attackCooldown = 0.4f;
    public Vector2 attackOffset = new Vector2(0.5f, 0f);
    public LayerMask enemyLayers;

    [Header("Ultimate - Water Projectile")]
    public GameObject waterProjectilePrefab;
    public int ultimateDamage = 2;
    public float ultimateCooldown = 5f;
    public float projectileSpawnOffsetX = 0.6f;

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
    public AudioClip attackSound;
    public AudioClip ultimateSound;

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
    private bool isAttacking = false;
    private bool canAttack = true;
    private bool canUltimate = true;
    private int extraJumps;
    private float originalGravityScale;
    private float lastMoveDirection = 1f;
    private FloatingHealthBar floatingHealthBar;

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

        // Instantiate and setup Floating Health Bar
        GameObject healthBarObj = new GameObject("FloatingHealthBar");
        floatingHealthBar = healthBarObj.AddComponent<FloatingHealthBar>();
        floatingHealthBar.SetTarget(transform);
        floatingHealthBar.UpdateHealth(currentHealth, maxHealth);

        if (scoreText == null)
        {
            CreateFallbackScoreUI();
        }

        UpdateScoreText();
        UpdateHealthUI();
    }

    void FixedUpdate()
    {
        // Reserved for physics updates
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
                isGrounded = false;
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

        // --- Attack ---
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J)) && canAttack && !isAttacking)
        {
            StartCoroutine(Attack());
        }

        // --- Ultimate (Water Projectile) ---
        if (Input.GetKeyDown(KeyCode.Q) && canUltimate && !isAttacking)
        {
            int currentScore = (GameManager.Instance != null) ? GameManager.Instance.TotalScore : 0;
            if (currentScore >= 5)
            {
                if (GameManager.Instance != null) GameManager.Instance.AddScore(-5);
                UpdateScoreText();
                StartCoroutine(Ultimate());
            }
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

    // --- Attack Coroutine ---
    private IEnumerator Attack()
    {
        isAttacking = true;
        canAttack = false;

        // Trigger the attack animation
        animator.SetTrigger("Attack");

        // Play attack sound
        if (attackSound != null) { audioSource.PlayOneShot(attackSound); }

        // Calculate hitbox position based on facing direction
        Vector2 hitPos = (Vector2)transform.position + new Vector2(attackOffset.x * lastMoveDirection, attackOffset.y);

        // Detect enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(hitPos, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Try to deal damage to the enemy
            enemy.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }

        // Wait for attack animation to finish
        yield return new WaitForSeconds(0.25f);
        isAttacking = false;

        // Cooldown before next attack
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // --- Ultimate Coroutine ---
    private IEnumerator Ultimate()
    {
        isAttacking = true;
        canUltimate = false;

        animator.SetTrigger("Attack");
        if (ultimateSound != null) { audioSource.PlayOneShot(ultimateSound); }

        yield return new WaitForSeconds(0.15f);

        // Spawn water projectile
        if (waterProjectilePrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(projectileSpawnOffsetX * lastMoveDirection, 0.1f, 0f);
            GameObject proj = Instantiate(waterProjectilePrefab, spawnPos, Quaternion.identity);

            Projectile projScript = proj.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.damage = ultimateDamage;
                projScript.ownerTag = "Player";
                projScript.SetDirection(new Vector2(lastMoveDirection, 0f));
            }
        }
        else
        {
            Debug.LogWarning("PlayerController: Water Projectile Prefab is not assigned!");
        }

        yield return new WaitForSeconds(0.3f);
        isAttacking = false;

        yield return new WaitForSeconds(ultimateCooldown);
        canUltimate = true;
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
        
        if (floatingHealthBar != null)
        {
            floatingHealthBar.UpdateHealth(currentHealth, maxHealth);
        }
        
        if (hurtSound != null) audioSource.PlayOneShot(hurtSound);

        animator.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            BossArena arena = Object.FindObjectOfType<BossArena>();
            if (arena != null)
            {
                StartCoroutine(ShowBossDeathScreen());
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        else
        {
            StartCoroutine(BecomeInvincible());
        }
    }

    private IEnumerator ShowBossDeathScreen()
    {
        if (winTextObject != null)
        {
            winTextObject.text = "SATYR killed you\nPress ENTER to restart from beginning";
            winTextObject.gameObject.SetActive(true);
        }
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(0.5f);

        while (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            yield return null;
        }

        Time.timeScale = 1f;
        if (GameManager.Instance != null) GameManager.Instance.ResetScore();
        SceneManager.LoadScene("SampleScene"); // Load starting level
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
            Destroy(other.gameObject);
            
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
        if (healthHearts == null || healthHearts.Count == 0) return;
        for (int i = 0; i < healthHearts.Count; i++)
        {
            if (healthHearts[i] == null) continue;
            healthHearts[i].enabled = (i < currentHealth);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Trampoline Bounce
    public void Bounce(float bounceForce)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
        
        isGrounded = false;
        animator.SetBool("IsGrounded", false);
        animator.SetTrigger("Jump");
        
        extraJumps = extraJumpsValue;
    }

    // --- Debug Visualization ---
    private void OnDrawGizmosSelected()
    {
        // Draw the attack range in the Scene view for easy tuning
        Vector2 hitPos = (Vector2)transform.position + new Vector2(attackOffset.x * lastMoveDirection, attackOffset.y);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPos, attackRange);
    }

    private void CreateFallbackScoreUI()
    {
        GameObject canvasObj = new GameObject("FallbackScoreCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject textObj = new GameObject("ScoreText");
        textObj.transform.SetParent(canvasObj.transform, false);
        scoreText = textObj.AddComponent<TextMeshProUGUI>();
        
        RectTransform rt = scoreText.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); // Top Left
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20, -20);
        rt.sizeDelta = new Vector2(400, 100);

        scoreText.fontSize = 36;
        scoreText.color = Color.white;
        scoreText.fontStyle = FontStyles.Bold;
        scoreText.alignment = TextAlignmentOptions.TopLeft;
    }
}
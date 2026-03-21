/*
 * BossController.cs
 * Satyr Boss — Grounded melee boss with 3 phases.
 * Phase 1: Patrol + Charge Ram
 * Phase 2: + Slash Attack (close range)
 * Phase 3: + Leap Slam (AoE ground pound)
 * 
 * Sprite sheet layout (SATYR):
 *   Idle:      frames 0–5   (row y~324)
 *   Run:       frames 6–13  (row y~292)
 *   Attack1:   frames 14–17 (row y~260) — Quick slash
 *   Attack2:   frames 18–24 (row y~226) — Wide swing
 *   Hurt:      frames 25–30 (row y~198)
 *   Jump/Leap: frames 31–36 (row y~167)
 *   Death:     frames 37+   (row y~132)
 */

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class BossController : MonoBehaviour
{
    // --- Health ---
    [Header("Health")]
    public int maxHealth = 12;

    // --- Movement ---
    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chargeSpeed = 9f;
    public float chargeDuration = 0.7f;

    // --- Attacks ---
    [Header("Attack Timing")]
    public float attackCooldown = 2f;

    [Header("Slash Attack")]
    public float slashRange = 1.2f;
    public int slashDamage = 1;
    public Vector2 slashOffset = new Vector2(0.8f, 0f);

    [Header("Leap Slam (Phase 3)")]
    public float leapForce = 10f;
    public float slamDownForce = 16f;
    public float slamAOERange = 3f;
    public int slamDamage = 1;
    public float slamShakeIntensity = 0.2f;
    public float slamShakeDuration = 0.35f;
    public GameObject fireProjectilePrefab;
    public float fireProjectileOffsetX = 1f;

    [Header("Contact Damage")]
    public int contactDamage = 1;

    [Header("Audio")]
    public AudioClip chargeSound;
    public AudioClip slashSound;
    public AudioClip leapSound;
    public AudioClip slamSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    [Header("Events")]
    public UnityEvent OnBossDeath;

    // --- Patrol Boundaries (set via BossArena or manually) ---
    [HideInInspector] public float leftBound = -6f;
    [HideInInspector] public float rightBound = 6f;

    // --- Private ---
    private int currentHealth;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private AudioSource audioSource;
    private Transform playerTransform;

    private bool isActive = false;
    private bool isDead = false;
    private bool isAttacking = false;
    private float attackTimer;
    private int patrolDirection = 1;
    private Color originalColor;

    // Phase thresholds
    private const float PHASE2_THRESHOLD = 0.6f;
    private const float PHASE3_THRESHOLD = 0.3f;

    private enum BossPhase { Phase1, Phase2, Phase3 }
    private BossPhase currentPhase = BossPhase.Phase1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        currentHealth = maxHealth;
        originalColor = spriteRenderer.color;
        attackTimer = attackCooldown;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (!isActive || isDead) return;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else return;
        }

        UpdatePhase();
        FacePlayer();

        if (!isAttacking)
        {
            Patrol();
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f && !isAttacking)
        {
            ChooseAttack();
            attackTimer = GetCooldownForPhase();
        }
    }

    // --- Activation ---
    public void ActivateBoss()
    {
        isActive = true;
        animator.SetFloat("Speed", 0f);
    }

    // --- Phase Management ---
    private void UpdatePhase()
    {
        float healthPercent = (float)currentHealth / maxHealth;

        if (healthPercent <= PHASE3_THRESHOLD && currentPhase != BossPhase.Phase3)
        {
            currentPhase = BossPhase.Phase3;
            patrolSpeed *= 1.4f;
        }
        else if (healthPercent <= PHASE2_THRESHOLD && currentPhase == BossPhase.Phase1)
        {
            currentPhase = BossPhase.Phase2;
            patrolSpeed *= 1.2f;
        }
    }

    private float GetCooldownForPhase()
    {
        switch (currentPhase)
        {
            case BossPhase.Phase1: return attackCooldown;
            case BossPhase.Phase2: return attackCooldown * 0.7f;
            case BossPhase.Phase3: return attackCooldown * 0.5f;
            default: return attackCooldown;
        }
    }

    // --- Movement ---
    private void Patrol()
    {
        rb.linearVelocity = new Vector2(patrolDirection * patrolSpeed, rb.linearVelocity.y);

        if (transform.position.x >= rightBound) patrolDirection = -1;
        else if (transform.position.x <= leftBound) patrolDirection = 1;
    }

    private void FacePlayer()
    {
        if (playerTransform == null) return;
        bool playerIsLeft = playerTransform.position.x < transform.position.x;
        spriteRenderer.flipX = playerIsLeft;
    }

    private float GetFacingDirection()
    {
        return spriteRenderer.flipX ? -1f : 1f;
    }

    // --- Attack Selection ---
    private void ChooseAttack()
    {
        float distToPlayer = playerTransform != null
            ? Vector2.Distance(transform.position, playerTransform.position)
            : 99f;

        switch (currentPhase)
        {
            case BossPhase.Phase1:
                // Always charge in Phase 1
                StartCoroutine(ChargeRam());
                break;

            case BossPhase.Phase2:
                // Close range = slash, far = charge, sometimes leap slam
                float rand2 = Random.value;
                if (rand2 > 0.7f)
                    StartCoroutine(LeapSlam());
                else if (distToPlayer < 2f)
                    StartCoroutine(SlashAttack());
                else
                    StartCoroutine(ChargeRam());
                break;

            case BossPhase.Phase3:
                float rand = Random.value;
                if (rand > 0.6f)
                    StartCoroutine(LeapSlam());
                else if (distToPlayer < 2f)
                    StartCoroutine(SlashAttack());
                else
                    StartCoroutine(ChargeRam());
                break;
        }
    }

    // ===== ATTACK 1: Charge Ram =====
    // Satyr lowers head and charges at the player like a ram
    private IEnumerator ChargeRam()
    {
        isAttacking = true;

        // Telegraph — stop and prepare
        rb.linearVelocity = Vector2.zero;
        animator.SetFloat("Speed", 0f);
        animator.SetTrigger("Stop");
        yield return new WaitForSeconds(0.5f);

        if (chargeSound != null) audioSource.PlayOneShot(chargeSound);

        // Charge toward the player — use Dash animation
        animator.SetTrigger("Dash");
        float dir = (playerTransform.position.x > transform.position.x) ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * chargeSpeed, rb.linearVelocity.y);

        yield return new WaitForSeconds(chargeDuration);

        // Skid to a stop
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        animator.SetTrigger("Stop");
        yield return new WaitForSeconds(0.4f);

        isAttacking = false;
    }

    // ===== ATTACK 2: Slash =====
    // Close-range melee swipe with hooves/horns
    private IEnumerator SlashAttack()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        animator.SetFloat("Speed", 0f);

        // Play attack animation
        animator.SetTrigger("Attack");
        if (slashSound != null) audioSource.PlayOneShot(slashSound);

        // Small delay for windup frames
        yield return new WaitForSeconds(0.2f);

        // Hitbox check in front of the Satyr
        float facing = GetFacingDirection();
        Vector2 hitPos = (Vector2)transform.position + new Vector2(slashOffset.x * facing, slashOffset.y);
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPos, slashRange);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController pc = hit.GetComponent<PlayerController>();
                if (pc != null) pc.TakeDamage(slashDamage);
            }
        }

        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
    }

    // ===== ATTACK 3: Leap Slam (Phase 3 only) =====
    // Satyr leaps into the air and slams down, AoE damage + screen shake
    private IEnumerator LeapSlam()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetFloat("Speed", 0f);

        // Telegraph — crouch with Stop animation
        animator.SetTrigger("Stop");
        if (leapSound != null) audioSource.PlayOneShot(leapSound);
        yield return new WaitForSeconds(0.3f);

        // Leap toward the player — use Jump animation
        animator.SetTrigger("Jump");
        float dirX = 0f;
        if (playerTransform != null)
        {
            dirX = (playerTransform.position.x > transform.position.x) ? 1f : -1f;
        }
        
        // Use direct velocity instead of AddForce to ignore boss mass
        rb.linearVelocity = new Vector2(dirX * 4f, leapForce);

        yield return new WaitForSeconds(0.4f);

        // Slam down
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.3f, -slamDownForce);

        // Wait until grounded
        float timeout = 2f;
        while (!IsGrounded() && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        // Impact! Play Ultimate animation for the slam landing
        animator.SetTrigger("Ultimate");
        if (slamSound != null) audioSource.PlayOneShot(slamSound);
        StartCoroutine(ScreenShake());

        // AoE damage around landing spot
        if (playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist < slamAOERange)
            {
                PlayerController pc = playerTransform.GetComponent<PlayerController>();
                if (pc != null) pc.TakeDamage(slamDamage);
            }
        }

        // Spawn fire projectile toward the player
        SpawnFireProjectile();

        yield return new WaitForSeconds(0.7f);
        isAttacking = false;
    }

    private void SpawnFireProjectile()
    {
        if (fireProjectilePrefab == null)
        {
            Debug.LogWarning("BossController: Fire Projectile Prefab is not assigned!");
            return;
        }
        if (playerTransform == null) return;

        float facing = GetFacingDirection();
        
        // Spawn from the center height of the boss to prevent hitting the ground instantly
        float spawnY = transform.position.y + 0.5f;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) spawnY = col.bounds.center.y;

        Vector3 spawnPos = new Vector3(transform.position.x + (fireProjectileOffsetX * facing), spawnY, 0f);
        GameObject proj = Instantiate(fireProjectilePrefab, spawnPos, Quaternion.identity);

        Projectile projScript = proj.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.ownerTag = gameObject.tag;
            projScript.SetDirection(new Vector2(facing, 0f));
        }
    }

    private bool IsGrounded()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Vector2 rayStart = new Vector2(col.bounds.center.x, col.bounds.min.y + 0.1f);
            RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, 0.5f);
            return hit.collider != null && hit.collider.CompareTag("Ground");
        }
        
        RaycastHit2D fallbackHit = Physics2D.Raycast(transform.position, Vector2.down, 2f);
        return fallbackHit.collider != null && fallbackHit.collider.CompareTag("Ground");
    }

    private IEnumerator ScreenShake()
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        Vector3 originalPos = cam.transform.position;
        float elapsed = 0f;

        while (elapsed < slamShakeDuration)
        {
            float x = Random.Range(-slamShakeIntensity, slamShakeIntensity);
            float y = Random.Range(-slamShakeIntensity, slamShakeIntensity);
            cam.transform.position = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.transform.position = originalPos;
    }

    // --- Damage ---
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (hurtSound != null) audioSource.PlayOneShot(hurtSound);

        animator.SetTrigger("Hit");
        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitFlash()
    {
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        if (!isDead) spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        isDead = true;
        isActive = false;
        rb.linearVelocity = Vector2.zero;

        animator.SetTrigger("Death");
        if (deathSound != null) audioSource.PlayOneShot(deathSound);

        OnBossDeath?.Invoke();

        Destroy(gameObject, 3f);
    }

    // --- Contact Damage ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamage(contactDamage);
            }
        }
    }

    // --- Public Getters ---
    public int GetCurrentHealth() { return currentHealth; }
    public int GetMaxHealth() { return maxHealth; }
    public bool IsDead() { return isDead; }

    // --- Debug ---
    private void OnDrawGizmosSelected()
    {
        // Show slash range
        float facing = Application.isPlaying ? GetFacingDirection() : 1f;
        Vector2 hitPos = (Vector2)transform.position + new Vector2(slashOffset.x * facing, slashOffset.y);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(hitPos, slashRange);

        // Show slam AoE
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, slamAOERange);
    }
}

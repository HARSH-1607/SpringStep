/*
 * Projectile.cs
 * Directional projectile that works for both boss and player.
 * Damages anything it hits via SendMessage("TakeDamage").
 */

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 6f;
    public int damage = 1;
    public float lifeTime = 4f;

    [Tooltip("Tag of the owner so the projectile doesn't hit who fired it")]
    public string ownerTag = "";

    private Vector2 direction = Vector2.left;
    private Rigidbody2D rb;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.flipX = direction.x < 0f;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = direction * speed;

        // Flip sprite if going left
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipX = direction.x < 0f;
        }

        // Ensure triggers are properly set to avoid bouncing the boss
        Collider2D[] cols = GetComponents<Collider2D>();
        foreach(var c in cols) c.isTrigger = true;

        // Auto-destroy
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject hitObject)
    {
        // Don't hit the owner
        if (!string.IsNullOrEmpty(ownerTag) && hitObject.CompareTag(ownerTag))
            return;

        // Ignore harmless triggers (invisible boundary boxes, etc.)
        Collider2D hitCol = hitObject.GetComponent<Collider2D>();
        if (hitCol != null && hitCol.isTrigger && !hitObject.CompareTag("Player") && !hitObject.CompareTag("Enemy") && !hitObject.CompareTag("Boss"))
        {
            return;
        }

        // Destroy on ground
        if (hitObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
            return;
        }

        // Only destroy if it hits a relevant target (or we could just allow it to hit anything not excluded above)
        if (hitObject.CompareTag("Player") || hitObject.CompareTag("Enemy") || hitObject.CompareTag("Boss"))
        {
            hitObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}


/*
 * BossArena.cs
 * Manages the boss fight flow: arena lockdown, intro, fight, and win condition.
 * Attach to a trigger collider that covers the arena entrance.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BossArena : MonoBehaviour
{
    [Header("Arena Walls")]
    [Tooltip("Wall GameObjects that block the player inside. Start these DISABLED in the scene.")]
    public GameObject leftWall;
    public GameObject rightWall;

    [Header("Boss Reference")]
    public BossController boss;

    [Header("Health Bar")]
    [Tooltip("Optional: Assign a BossHealthBarUI. If empty, one is auto-created.")]
    public BossHealthBarUI healthBar;

    [Header("Intro Settings")]
    public string bossName = "SATYR";
    public float introDelay = 6f;

    [Header("Win Settings")]
    public string winSceneName = "WinScreen";
    public float winDelay = 3f;

    [Header("Boss Patrol Bounds (Local X offset from arena center)")]
    public float boundsHalfWidth = 5f;

    private bool fightStarted = false;
    private bool fightOver = false;
    private TextMeshPro introText;

    private void Start()
    {
        // Ensure walls are disabled at start
        if (leftWall != null) leftWall.SetActive(false);
        if (rightWall != null) rightWall.SetActive(false);

        // Auto-create health bar if not assigned
        if (healthBar == null)
        {
            GameObject hbObj = new GameObject("BossHealthBar");
            healthBar = hbObj.AddComponent<BossHealthBarUI>();
        }

        healthBar.SetBossName(bossName);
    }

    private void Update()
    {
        // Update health bar during fight
        if (fightStarted && !fightOver && boss != null)
        {
            healthBar.UpdateHealth(boss.GetCurrentHealth(), boss.GetMaxHealth());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!fightStarted && other.CompareTag("Player"))
        {
            fightStarted = true;
            StartCoroutine(StartBossFight());
        }
    }

    private IEnumerator StartBossFight()
    {
        // 1. Lock the arena
        if (leftWall != null) leftWall.SetActive(true);
        if (rightWall != null) rightWall.SetActive(true);

        // 2. Show boss name intro
        ShowIntroText();

        // Wait for player to press Enter before beginning
        while (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            yield return null;
        }

        // 3. Hide intro text
        if (introText != null) Destroy(introText.gameObject);

        // 4. Show health bar
        healthBar.Show();

        // 5. Set boss patrol bounds and activate
        if (boss != null)
        {
            boss.leftBound = transform.position.x - boundsHalfWidth;
            boss.rightBound = transform.position.x + boundsHalfWidth;
            boss.ActivateBoss();

            // Listen for death
            boss.OnBossDeath.AddListener(OnBossDefeated);
        }
    }

    private void ShowIntroText()
    {
        GameObject textObj = new GameObject("BossIntroText");

        // Position it near the boss
        if (boss != null)
            textObj.transform.position = boss.transform.position + new Vector3(0f, 2f, 0f);
        else
            textObj.transform.position = transform.position + new Vector3(0f, 3f, 0f);

        introText = textObj.AddComponent<TextMeshPro>();
        introText.text = "SATYR the destroyer is here. Beat him with all your might!\nTip: You can use projectiles to defeat SATYR.\nEach projectile uses 5 score points, so previous levels' score makes sense.\n\n[Press ENTER to begin]";
        introText.fontSize = 2.5f;
        introText.enableWordWrapping = true;
        introText.alignment = TextAlignmentOptions.Center;
        introText.color = Color.red;
        introText.fontStyle = FontStyles.Bold;
        introText.sortingOrder = 32000; // Render over other sprites

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(25f, 10f);
    }

    private void OnBossDefeated()
    {
        fightOver = true;
        healthBar.Hide();

        // Open walls
        if (leftWall != null) leftWall.SetActive(false);
        if (rightWall != null) rightWall.SetActive(false);

        StartCoroutine(WinSequence());
    }

    private IEnumerator WinSequence()
    {
        yield return new WaitForSeconds(winDelay);
        SceneManager.LoadScene(winSceneName);
    }
}

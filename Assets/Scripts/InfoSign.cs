using UnityEngine;

public class InfoSign : MonoBehaviour
{
    [Tooltip("Assign the Text GameObject that contains the guide/instruction here.")]
    public GameObject uiTextObject;

    private void Start()
    {
        // Ensure the text is hidden when the game starts
        if (uiTextObject != null)
        {
            uiTextObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && uiTextObject != null)
        {
            uiTextObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && uiTextObject != null)
        {
            uiTextObject.SetActive(false);
        }
    }
}

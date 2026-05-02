using UnityEngine;
using TMPro; // Make sure this is at the top!

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject resultPanel;
    public TextMeshProUGUI resultText; // This usually says "YOU WIN" or "YOU LOSE"
    public TextMeshProUGUI codeDisplayText; // Drag your new "Text (TMP)" here

    void Awake() => Instance = this;

    public void ShowResult(bool won)
    {
        resultPanel.SetActive(true);
        resultText.text = won ? "YOU WIN!" : "YOU LOSE!";

        // Hide the code text if they lose
        if (!won && codeDisplayText != null)
            codeDisplayText.text = "";
    }

    // ADD THIS NEW FUNCTION TO FIX THE ERROR
    public void ShowWinMessage(string secretCode)
    {
        resultPanel.SetActive(true);

        // Header for the panel
        resultText.text = "SIGNAL DECODED";

        if (codeDisplayText != null)
        {
            // This combines the "how it feels" message with the R-G-B-Y code
            codeDisplayText.text = "Now you know what Tritanopia feels like. You have escaped.\n\n" +
                                   "CODE: " + secretCode;
        }
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
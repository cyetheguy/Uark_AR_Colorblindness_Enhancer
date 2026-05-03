using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI codeDisplayText;

    void Awake() => Instance = this;

    void Start()
    {
        // Make sure panel is hidden at start
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    public void ShowResult(bool won)
    {
        resultPanel.SetActive(true);

        if (won)
        {
            resultText.text = "YOU ESCAPED!";
            if (codeDisplayText != null)
                codeDisplayText.text =
                    "You just experienced Tritanopia —\n" +
                    "a form of color blindness where\n" +
                    "blue and yellow look identical.\n\n" +
                    "Yet you solved the puzzle.\n" +
                    "Imagine living this every day.";
        }
        else
        {
            resultText.text = "THE COLORS FOOLED YOU";
            /* if (codeDisplayText != null)
                 codeDisplayText.text =
                     "This is Tritanopia.\n" +
                     "Blue and yellow appear the same.\n\n" +
                     "For millions of people,\n" +
                     "This is not a game " +
                     "this is everyday life.\n\n";*/
            if (codeDisplayText != null)
                codeDisplayText.text =
                    "Blue. Yellow. The same.\n\n" +
                    "This is Tritanopia —\n" +
                    "what 1 in 10,000 people see daily.\n\n";
        }
    }

    public void ShowWinMessage(string secretCode)
    {
        resultPanel.SetActive(true);
        resultText.text = "YOU ESCAPED!";
        if (codeDisplayText != null)
            codeDisplayText.text =
                "You just experienced Tritanopia —\n" +
                "a form of color blindness where\n" +
                "blue and yellow look identical.\n\n" +
                "Yet you solved the puzzle.\n" +
                "Imagine living this every day.";
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
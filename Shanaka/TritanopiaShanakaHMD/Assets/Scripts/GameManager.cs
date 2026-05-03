using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject cubePrefab;
    public Transform cubeHolder;
    public TritanopiaColorblindController colorblindController;
    private Queue<CubeColor> colorQueue = new Queue<CubeColor>();
    private bool gameOver = false;

    void Awake() => Instance = this;

    void Start()
    {
        var colors = new List<CubeColor>
        {
            CubeColor.Red,    CubeColor.Red,
            CubeColor.Green,  CubeColor.Green,
            CubeColor.Blue,   CubeColor.Blue,
            CubeColor.Yellow, CubeColor.Yellow,
            (CubeColor)Random.Range(0, 4)
        };

        for (int i = colors.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (colors[i], colors[j]) = (colors[j], colors[i]);
        }

        foreach (var c in colors)
            colorQueue.Enqueue(c);

        // Enable filter BEFORE spawning any cube
        if (colorblindController != null)
        {
            colorblindController.EnableTritanopia();
            Debug.Log("Tritanopia filter enabled at start");
        }
        else
        {
            Debug.LogError("colorblindController is NULL! Drag FilterController into GameManager!");
        }

        // Delay spawn so filter activates first
        Invoke(nameof(StartGame), 0.5f);
    }

    void StartGame()
    {
        FindObjectOfType<InputHandler>().SpawnNext();
    }

    public CubeController GetNextCube()
    {
        if (colorQueue.Count == 0) return null;
        GameObject go = Instantiate(cubePrefab);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        CubeController cube = go.GetComponent<CubeController>();
        cube.SetColor(colorQueue.Dequeue());
        return cube;
    }

    public void OnCubePlaced()
    {
        if (gameOver) return;
        var grid = GridManager.Instance.grid;

        if (RuleChecker.Instance.AllFilled(grid))
        {
            gameOver = true;
            RevealRealColors();
            bool playerWon = !RuleChecker.Instance.HasViolation(grid);
            if (playerWon)
                UIManager.Instance.ShowWinMessage("R-Y-B-G-Y-R-G-B-Y");
            else
                UIManager.Instance.ShowResult(false);
            return;
        }

        FindObjectOfType<InputHandler>().SpawnNext();
    }

    private void RevealRealColors()
    {
        if (colorblindController != null)
            colorblindController.DisableFilter();
    }
}
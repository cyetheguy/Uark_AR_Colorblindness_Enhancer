using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// No Cube 0
// RED_CUBE 1
//  BLU_CUBE 2
// GRN_CUBE 3

public class Tritanopia : MonoBehaviour
{

    public GameObject cubePrefab;
    public TextMeshProUGUI countText;
    public Material whiteMaterial;
    public Material redMaterial;
    public Material greenMaterial;
    public Material blueMaterial;
    public float gameSpace = 3;



    private GameObject[,] cubeArray = new GameObject[3, 3];
    private int[,] color_map = new int[3, 3];
    private int[] curPos = { 0, 0 };
    private int curColor = 1;
    private int tilesLeft = 9;
    private int[,] winning_map = { { 1, 2, 3 }, { 2, 3, 1 }, { 3, 1, 2 } };




    // Start is called before the first frame update
    void Start()
    {
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                GameObject cellCube = Instantiate(cubePrefab, transform);
                cellCube.transform.localPosition = new Vector3((x-1) * gameSpace, 2, (y-1) * gameSpace);
                cubeArray[y,x] = cellCube;

            }
        }

        UpdateCells();
        UpdateCell(curPos[0], curPos[1], curColor);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        tilesLeft = 9;
        for(int y = 0; y < 3; y++)
        {
            for(int x = 0; x < 3; x++)
            {
                if (color_map[y,x] == winning_map[y,x]) tilesLeft--;

            }
        }

        SetCountText();
        if (tilesLeft < 1) ImageTrackingManager.tritanopiaComplete = true; // Won
        
    }

    void UpdateCell(int y, int x, int color = 0)
    {
        switch(color)
        {
            case 0:
                cubeArray[y,x].GetComponent<Renderer>().material = whiteMaterial;

                break;
            case 1:
                cubeArray[y, x].GetComponent<Renderer>().material = redMaterial;
                break;
            case 2:
                cubeArray[y, x].GetComponent<Renderer>().material = greenMaterial;
                break;
            case 3:
                cubeArray[y, x].GetComponent<Renderer>().material = blueMaterial;
                break;

        }
    }

    void UpdateCells()
    {
        for (int y = 0; y < 3; y++) for (int x = 0; x < 3; x++) UpdateCell(y, x, color_map[y,x]);
    }

    public void SetCell()
    {
        int y = curPos[0];
        int x = curPos[1];

        color_map[y, x] = curColor;
        UpdateCells();

        curPos[0] = 0;
        curPos[1] = 0;
        curColor = UnityEngine.Random.Range(1, 4);
    }

    public void MoveRight()
    {
        curPos[1] = (curPos[1] + 1) % 3;
        UpdateCells();
        UpdateCell(curPos[0], curPos[1], curColor);
    }

    public void MoveUp()
    {
        curPos[0] = (curPos[0] + 1) % 3;
        UpdateCells();
        UpdateCell(curPos[0], curPos[1], curColor);
    }

    void SetCountText()
    {
        string text = "Sudoku Minigame\nExploits Tritanopia\nTiles left till win: " + tilesLeft.ToString();
        if (ImageTrackingManager.tritanopiaComplete) text = text + "\n\nLEVEL COMPLETE";
        countText.text = text;
    }
}

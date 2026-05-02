using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public GameObject cellPrefab;
    public float spacing = 1.1f;
    public CellSlot[,] grid = new CellSlot[3, 3];

    void Awake()
    {
        Instance = this;
        SpawnGrid();
    }

    void SpawnGrid()
    {
        float offset = (spacing * (3 - 1)) / 2f;

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                Vector3 pos = new Vector3(
                    c * spacing - offset,
                    0,
                    r * spacing - offset
                );
                GameObject cell = Instantiate(
                    cellPrefab, pos,
                    Quaternion.identity, transform);
                grid[r, c] = cell.GetComponent<CellSlot>();
                grid[r, c].row = r;
                grid[r, c].col = c;
            }
        }
    }
}
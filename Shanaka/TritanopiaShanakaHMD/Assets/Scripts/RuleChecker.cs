using UnityEngine;

public class RuleChecker : MonoBehaviour
{
    public static RuleChecker Instance;

    void Awake() => Instance = this;

    public bool HasViolation(CellSlot[,] grid)
    {
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                if (!grid[r, c].IsOccupied) continue;

                CubeColor color = grid[r, c].occupant.color;

                
                if (c + 1 < 3 && grid[r, c + 1].IsOccupied)
                    if (grid[r, c + 1].occupant.color == color) return true;

               
                if (r + 1 < 3 && grid[r + 1, c].IsOccupied)
                    if (grid[r + 1, c].occupant.color == color) return true;
            }
        }
        return false;
    }

    public bool AllFilled(CellSlot[,] grid)
    {
        foreach (var cell in grid)
            if (!cell.IsOccupied) return false;
        return true;
    }
}
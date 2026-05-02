using UnityEngine;

public class CellSlot : MonoBehaviour
{
    public int row;
    public int col;
    public CubeController occupant = null;
    public bool IsOccupied => occupant != null;
}
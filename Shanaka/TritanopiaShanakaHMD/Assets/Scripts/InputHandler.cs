using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private int curRow = 0;
    private int curCol = 0;
    private CubeController activeCube;
    private bool gameActive = false;
    public float hoverHeight = 0.6f;

    // Thumbstick control
    private float axisThreshold = 0.7f;
    private bool verticalInUse = false;
    private bool horizontalInUse = false;

    void Start()
    {
        Invoke(nameof(SpawnNext), 1.0f);
    }

    void Update()
    {
        if (!gameActive || activeCube == null) return;

        // -------- KEYBOARD (for testing) --------
        if (Input.GetKeyDown(KeyCode.UpArrow)) Move(1, 0);
        if (Input.GetKeyDown(KeyCode.DownArrow)) Move(-1, 0);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Move(0, -1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Move(0, 1);
        if (Input.GetKeyDown(KeyCode.Space)) Place();

        // -------- VR THUMBSTICK --------
        HandleThumbstick();

        // -------- TRIGGER TO PLACE --------
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
            OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            Place();
        }
    }

    void HandleThumbstick()
    {
        Vector2 axis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        float horizontal = axis.x;
        float vertical = axis.y;

        // -------- VERTICAL --------
        if (Mathf.Abs(vertical) > axisThreshold)
        {
            if (!verticalInUse)
            {
                if (vertical > 0)
                    Move(1, 0);
                else
                    Move(-1, 0);

                verticalInUse = true;
            }
        }
        else
        {
            verticalInUse = false; // reset when released
        }

        // -------- HORIZONTAL --------
        if (Mathf.Abs(horizontal) > axisThreshold)
        {
            if (!horizontalInUse)
            {
                if (horizontal > 0)
                    Move(0, 1);
                else
                    Move(0, -1);

                horizontalInUse = true;
            }
        }
        else
        {
            horizontalInUse = false;
        }
    }

    void Move(int r, int c)
    {
        int newRow = Mathf.Clamp(curRow + r, 0, 2);
        int newCol = Mathf.Clamp(curCol + c, 0, 2);

        if (newRow == curRow && newCol == curCol) return;

        curRow = newRow;
        curCol = newCol;

        ParentCubeToCell(curRow, curCol);
    }

    void ParentCubeToCell(int r, int c)
    {
        var cell = GridManager.Instance.grid[r, c];
        if (cell == null) return;

        activeCube.transform.SetParent(cell.transform);
        activeCube.transform.localPosition = new Vector3(0, hoverHeight, 0);
        activeCube.transform.localRotation = Quaternion.identity;
        activeCube.transform.localScale = Vector3.one;
    }

    void Place()
    {
        CellSlot slot = GridManager.Instance.grid[curRow, curCol];
        if (slot.IsOccupied) return;

        slot.occupant = activeCube;

        activeCube.transform.SetParent(slot.transform);
        //activeCube.transform.localPosition = new Vector3(0, 0.1f, 0);
        //activeCube.transform.localPosition = new Vector3(0, 0.5f, 0);
        activeCube.transform.localPosition = new Vector3(0, 0.1f, 0);
        activeCube.transform.localRotation = Quaternion.identity;
        activeCube.transform.localScale = Vector3.one;

        activeCube = null;
        gameActive = false;

        GameManager.Instance.OnCubePlaced();
    }

    public void SpawnNext()
    {
        if (GameManager.Instance == null) return;
        if (GridManager.Instance == null) return;

        activeCube = GameManager.Instance.GetNextCube();

        if (activeCube != null)
        {
            curRow = 0;
            curCol = 0;
            gameActive = true;

            ParentCubeToCell(0, 0);
        }
    }

    // Optional UI buttons
    public void MoveUp() { if (gameActive) Move(1, 0); }
    public void MoveDown() { if (gameActive) Move(-1, 0); }
    public void MoveLeft() { if (gameActive) Move(0, -1); }
    public void MoveRight() { if (gameActive) Move(0, 1); }
    public void PlaceCube() { if (gameActive) Place(); }
}
using UnityEngine;

public class TritanopiaColorblindController : MonoBehaviour
{
    [Header("Original Materials")]
    public Material matRed;
    public Material matGreen;
    public Material matBlue;
    public Material matYellow;

    [Header("Tritanopia Materials")]
    public Material matRedTrit;
    public Material matGreenTrit;
    public Material matBlueTrit;
    public Material matYellowTrit;

    private bool filterOn = false;

    public void EnableTritanopia()
    {
        filterOn = true;
        RefreshAllCubes();
        Debug.Log("Tritanopia ON");
    }

    public void DisableFilter()
    {
        filterOn = false;
        RefreshAllCubes();
        Debug.Log("Filter OFF");
    }

    public Material GetMaterial(CubeColor color)
    {
        if (filterOn)
        {
            return color switch
            {
                CubeColor.Red => matRedTrit,
                CubeColor.Green => matGreenTrit,
                CubeColor.Blue => matBlueTrit,
                CubeColor.Yellow => matYellowTrit,
                _ => matRedTrit
            };
        }
        return color switch
        {
            CubeColor.Red => matRed,
            CubeColor.Green => matGreen,
            CubeColor.Blue => matBlue,
            CubeColor.Yellow => matYellow,
            _ => matRed
        };
    }

    void RefreshAllCubes()
    {
        var cubes = FindObjectsOfType<CubeController>();
        foreach (var cube in cubes)
            cube.ApplyCurrentMaterial();
    }
}
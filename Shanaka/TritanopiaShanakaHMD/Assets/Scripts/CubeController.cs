using UnityEngine;

public enum CubeColor { Red, Green, Blue, Yellow }

public class CubeController : MonoBehaviour
{
    public CubeColor color;
    public Material matRed;
    public Material matGreen;
    public Material matBlue;
    public Material matYellow;

    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void SetColor(CubeColor c)
    {
        color = c;
        ApplyCurrentMaterial();
    }

    public void ApplyCurrentMaterial()
    {
        if (rend == null) rend = GetComponent<Renderer>();

        var controller = GameManager.Instance?.colorblindController;

        if (controller != null)
        {
            rend.material = controller.GetMaterial(color);
        }
        else
        {
            rend.material = color switch
            {
                CubeColor.Red => matRed,
                CubeColor.Green => matGreen,
                CubeColor.Yellow => matYellow,
                _ => matBlue
            };
        }
    }
}
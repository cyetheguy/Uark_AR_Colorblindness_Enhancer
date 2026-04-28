using System.Collections;
using UnityEngine;

/// <summary>
/// Pedestal: Cylinder platform that accepts a matching ColorCube.
/// Shows shape hint (not color name) so player can solve even under protanopia.
/// Lights up with true color when correctly solved.
/// </summary>
public class Pedestal : MonoBehaviour
{
    public CubeColor PedestalType { get; private set; }
    public bool IsSolved { get; private set; }

    private Material _mat;
    private ColorCube _placedCube;

    public void Initialize(CubeColor type, Material mat)
    {
        PedestalType = type;
        _mat = mat;

        // Make existing collider a trigger so cubes can enter it
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        // Add a larger trigger zone above pedestal for easier placement
        var trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(1.5f, 3f, 1.5f);
        trigger.center = new Vector3(0f, 1f, 0f);
    }

    public void MarkSolved(ColorCube cube)
    {
        IsSolved = true;
        _placedCube = cube;

        // Subtle pulse to indicate correct placement (before full reveal)
        StartCoroutine(SolvedPulse());
    }

    public void ApplyProtanopiaColor()
    {
        if (_mat == null) return;
        _mat.color = GameManager.Instance.GetProtanopiaColor(PedestalType) * 0.7f;
    }

    public void RevealTrueColor()
    {
        if (_mat == null) return;
        Color trueColor = GameManager.Instance.GetTrueColor(PedestalType);
        StartCoroutine(LerpColor(_mat.color, trueColor * 0.7f, 1.2f));

        // Enable emission on pedestal rim
        _mat.EnableKeyword("_EMISSION");
        _mat.SetColor("_EmissionColor", trueColor * 1.2f);
    }

    IEnumerator SolvedPulse()
    {
        Vector3 original = transform.localScale;
        transform.localScale = original * 1.3f;
        yield return new WaitForSeconds(0.15f);
        transform.localScale = original;
        yield return new WaitForSeconds(0.08f);
        transform.localScale = original * 1.15f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = original;
    }

    IEnumerator LerpColor(Color from, Color to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            _mat.color = Color.Lerp(from, to, t / duration);
            yield return null;
        }
        _mat.color = to;
    }
}

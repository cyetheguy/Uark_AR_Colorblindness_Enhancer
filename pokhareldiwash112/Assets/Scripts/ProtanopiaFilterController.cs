using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// ProtanopiaFilterController: Uses OVRPassthroughLayer Color LUT
/// to simulate protanopia on the entire passthrough feed.
/// Set your ProtanopiaLUT texture on the OVRPassthroughLayer in Inspector.
/// This script controls the LUT weight — 1 = full protanopia, 0 = normal vision.
/// </summary>
public class ProtanopiaFilterController : MonoBehaviour
{
    private OVRPassthroughLayer _passthroughLayer;

    public void Initialize()
    {
        _passthroughLayer = FindObjectOfType<OVRPassthroughLayer>();

        if (_passthroughLayer == null)
        {
            Debug.LogError("[ProtanopiaFilter] No OVRPassthroughLayer found!");
            return;
        }

        // Start at 0 — filter off before game begins
        SetLutWeight(0f);
        Debug.Log("[ProtanopiaFilter] Filter ready, weight = 0.");
    }

    public void ApplyImmediate()
    {
        SetLutWeight(1f);
        Debug.Log("[ProtanopiaFilter] Filter applied immediately.");
    }

    public IEnumerator FadeIn(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            SetLutWeight(t);
            yield return null;
        }
        SetLutWeight(1f);
        Debug.Log("[ProtanopiaFilter] Protanopia filter fully active.");
    }

    void SetLutWeight(float weight)
    {
        try
        {
            // colorLutWeight controls how strongly the LUT is applied
            // 0 = normal vision, 1 = full protanopia simulation
            var prop = _passthroughLayer.GetType().GetProperty("colorLutWeight");
            if (prop != null)
            {
                prop.SetValue(_passthroughLayer, weight);
                return;
            }

            // Try field name variation
            var field = _passthroughLayer.GetType().GetField("_colorLutWeight",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(_passthroughLayer, weight);
                return;
            }

            Debug.LogWarning($"[ProtanopiaFilter] Could not set LUT weight — try setting it manually in Inspector");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ProtanopiaFilter] SetLutWeight failed: {e.Message}");
        }
    }

    public IEnumerator FadeOut(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            SetLutWeight(1f - t);
            yield return null;
        }

        SetLutWeight(0f);
        Debug.Log("[ProtanopiaFilter] LUT removed — normal vision restored!");
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// StartButton: Handles Let's Go button interaction.
/// Works with Meta Ray Interactor, hand poke, and mouse click.
/// </summary>
public class StartButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private StartScreenUI _screen;
    private Image _img;
    private Color _normalColor;
    private Color _hoverColor = new Color(1f, 0.9f, 0.3f);
    private Color _pressColor = new Color(0.8f, 0.55f, 0.05f);
    private bool _pressed;

    public void Initialize(StartScreenUI screen, Image img)
    {
        _screen = screen;
        _img = img;
        _normalColor = img.color;
    }

    // ── Unity UI EventSystem (works with OVRInputModule + Ray) ───────
    public void OnPointerClick(PointerEventData eventData)
    {
        TriggerPress();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_pressed && _img) _img.color = _hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_pressed && _img) _img.color = _normalColor;
    }

    // ── Physical poke via hand/controller collider ───────────────────
    void OnTriggerEnter(Collider other)
    {
        TriggerPress();
    }

    // ── Editor mouse click fallback ──────────────────────────────────
    void OnMouseDown() { TriggerPress(); }
    void OnMouseEnter() { if (!_pressed && _img) _img.color = _hoverColor; }
    void OnMouseExit()  { if (!_pressed && _img) _img.color = _normalColor; }

    // ── OVR Controller trigger while gazing ──────────────────────────
    void Update()
    {
        if (_pressed) return;

        #if UNITY_ANDROID
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
            OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            if (IsBeingGazedAt()) TriggerPress();
        }
        #endif
    }

    bool IsBeingGazedAt()
    {
        if (Camera.main == null) return false;
        var ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        var col = GetComponent<Collider>();
        return col != null && col.bounds.IntersectRay(ray);
    }

    void TriggerPress()
    {
        if (_pressed) return;
        _pressed = true;
        if (_img) _img.color = _pressColor;
        _screen.OnLetsGoPressed();
    }
}

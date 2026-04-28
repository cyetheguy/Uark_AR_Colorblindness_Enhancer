using UnityEngine;

/// <summary>
/// SimpleDragInteractable: Editor/desktop fallback for testing cube interaction
/// without a Meta Quest headset. Click and drag to move cubes with the mouse.
/// 
/// This component is automatically added by GameManager when neither
/// OVRGrabbable nor XRGrabInteractable is available in the project.
/// Remove or ignore in production builds on Quest.
/// </summary>
public class SimpleDragInteractable : MonoBehaviour
{
    private bool _dragging;
    private Vector3 _offset;
    private float _dragDepth;
    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void OnMouseDown()
    {
        _dragDepth = Camera.main.WorldToScreenPoint(transform.position).z;
        _offset = transform.position - GetMouseWorldPos();
        _dragging = true;
        if (_rb != null) _rb.isKinematic = true;
    }

    void OnMouseDrag()
    {
        if (!_dragging) return;
        transform.position = GetMouseWorldPos() + _offset;
    }

    void OnMouseUp()
    {
        _dragging = false;
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }
    }

    Vector3 GetMouseWorldPos()
    {
        var mousePoint = Input.mousePosition;
        mousePoint.z = _dragDepth;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}

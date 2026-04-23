using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceObject : MonoBehaviour
{
    [SerializeField] private GameObject prefabObject;

    private GameObject instantiateObject;
    private Vector2 tapPosition;
    private ARRaycastManager _arRaycastManager;
    private List<ARRaycastHit> taps = new List<ARRaycastHit>();

    private void Awake()
    {
        _arRaycastManager = GetComponent<ARRaycastManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                tapPosition = touch.position;

            if (_arRaycastManager.Raycast(tapPosition, taps, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = taps[0].pose;

                if (instantiateObject == null)
                    instantiateObject = Instantiate(prefabObject, hitPose.position, hitPose.rotation);
                else
                    instantiateObject.transform.position = hitPose.position;
            }
        }
    }
}

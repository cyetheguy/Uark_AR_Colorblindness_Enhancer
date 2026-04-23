using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTrackingManager : MonoBehaviour
{
    // Declare variables containing our image library and prefabs
    private ARTrackedImageManager ourTrackedImages;
    public GameObject[] ourModelPrefabs;
    // Create a list to collect any prefabs we would create when the marker is detected
    List<GameObject> createdPrefabs = new List<GameObject>();

    // MonoBehaviour.Awake() is used to initialize variables or states before the application starts.
    void Awake()
    {
        ourTrackedImages = GetComponent<ARTrackedImageManager>();
    }

    // MonoBehaviour.OnEnable() is called when the object becomes enabled and active.
    void OnEnable()
    {
        ourTrackedImages.trackedImagesChanged += WhenTrackedImagesChange;
    }

    // MonoBehavious.OnDisable() is called when the behaviour becomes disabled.
    void OnDisable()
    {
        ourTrackedImages.trackedImagesChanged -= WhenTrackedImagesChange;
    }

    // Create the function that will handle the creation of prefabs depending on the marker observed.
    private void WhenTrackedImagesChange(ARTrackedImagesChangedEventArgs eventArgs)
    {
        //Create a new prefab depending on the image that is being tracked
        foreach (var trackedImage in eventArgs.added)
        {
            foreach (var modelPrefab in ourModelPrefabs)
            {
                if (trackedImage.referenceImage.name == modelPrefab.name)
                {
                    var newCreatedPrefab = Instantiate(modelPrefab, trackedImage.transform);
                    createdPrefabs.Add(newCreatedPrefab);
                }
            }
        }

        //Update the position of the prefab
        foreach (var trackedImage in eventArgs.updated)
        {
            foreach (var gameObject in createdPrefabs)
            {
                if (gameObject.name == trackedImage.name)
                {
                    gameObject.SetActive(trackedImage.trackingState == TrackingState.Tracking);
                }
            }
        }

    }
}

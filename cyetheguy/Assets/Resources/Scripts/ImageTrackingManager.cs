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


    // Objects for filter selection
    public ColorblindFilterScript filterScript;
    public GameObject dropdown;

    public static bool tritanopiaComplete = false;
    public static bool protanopiaComplete = false;
    public static bool deuteranopiaComplete = false;
    private bool gameWon = false;

    // MonoBehaviour.Awake() is used to initialize variables or states before the application starts.
    void Awake()
    {
        ourTrackedImages = GetComponent<ARTrackedImageManager>();

        dropdown.transform.Translate(0, 1000,0);
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
                    newCreatedPrefab.name = modelPrefab.name;
                    createdPrefabs.Add(newCreatedPrefab);
                }
            }
        }

        //Update the position of the prefab
        foreach (var trackedImage in eventArgs.updated)
        {
            string markerName = trackedImage.referenceImage.name;
            bool isTracking = trackedImage.trackingState == TrackingState.Tracking;
            foreach (var gameObject in createdPrefabs)
            {
                if (gameObject.name == markerName)
                {
                    gameObject.SetActive(isTracking);
                }
            }

            if (isTracking && !gameWon)
            {
                switch(markerName)
                {
                    case "Prize":
                        filterScript.OnValueChanged(0);
                        if (tritanopiaComplete && protanopiaComplete && deuteranopiaComplete)
                        {
                            dropdown.transform.Translate(0, -1000, 0);
                            gameWon = true;
                        }
                        break;
                    case "Tritanopia":
                        filterScript.OnValueChanged(1);
                        break;
                    case "Protanopia":
                        protanopiaComplete = true;
                        filterScript.OnValueChanged(2);
                        break;
                    case "Deuteranopia":
                        filterScript.OnValueChanged(3);
                        break;
                }
            }
        }

    }
}

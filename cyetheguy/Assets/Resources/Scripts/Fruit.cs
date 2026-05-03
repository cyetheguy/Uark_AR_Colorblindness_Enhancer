using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Fruit : MonoBehaviour
{
    public BoxCollider gridArea;
    public int boundSize = 6;
    


    // Start is called before the first frame update
    void Start()
    {
        
        goToRandomPos();
        
    }

    private void OnTriggerEnter(Collider other)
    {
        goToRandomPos();
    }




    private void goToRandomPos()
    {
        Bounds bounds = gridArea.bounds;
        float x = UnityEngine.Random.Range(-boundSize, boundSize);
        float z = UnityEngine.Random.Range(-boundSize, boundSize);
        transform.localPosition = new Vector3(Mathf.Round(x), 1.0f, Mathf.Round(z));
    }

    

}

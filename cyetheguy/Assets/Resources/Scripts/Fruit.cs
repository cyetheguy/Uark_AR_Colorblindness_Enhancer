using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Fruit : MonoBehaviour
{
    public BoxCollider gridArea;
    public int boundSize = 18;
    public TextMeshProUGUI countText;
    public GameObject winTextObject;

    private int count = 0;


    // Start is called before the first frame update
    void Start()
    {
        SetCountText();
        winTextObject.SetActive(false);
        goToRandomPos();
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Snake"))
        {
            goToRandomPos();
            count = count + 1;
            SetCountText();
            other.gameObject.GetComponent<Snake>().GrowSnake();

            if (count >= 10)
            {
                countText.text = "";
                winTextObject.SetActive(true);
                //Destroy(other.gameObject);
            }
        }

    }

    private void goToRandomPos()
    {
        Bounds bounds = gridArea.bounds;
        float x = UnityEngine.Random.Range(bounds.min.x, bounds.max.x);
        float z = UnityEngine.Random.Range(bounds.min.z, bounds.max.z);
        transform.position = new Vector3(Mathf.Round(x), 0.75f, Mathf.Round(z));
    }

    void SetCountText()
    {
        countText.text = "Deuteranopia Snake\nCount: " + count.ToString();
    }

}

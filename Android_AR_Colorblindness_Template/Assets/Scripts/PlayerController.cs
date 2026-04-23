using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private int count;
    private float movementX;
    private float movementY;
    public float speed = 0;
    public TextMeshProUGUI countText;
    public GameObject winTextObject;


    // Custom Variables
    public int speedBoost = 2;
    public int speedDuration = 5;
    public float magStrength = 1;
    private int lives = 3;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        SetCountText();
        winTextObject.SetActive(false);
        
    }

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }
    void SetCountText()
    {
        countText.text = "cYoung - Project 1\nCount: " + count.ToString() + "\nLives: " + lives.ToString();
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);
        rb.AddForce(movement * speed);

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            count = count + 1;
            SetCountText();

            if (count >= 12)
            {
                winTextObject.SetActive(true);
                Destroy(GameObject.FindGameObjectWithTag("Enemy"));
            }
        }

        if (other.gameObject.CompareTag("SpeedUp"))
        {
            other.gameObject.SetActive(false);
        }


    }

    // NEW CODED ADDED FOR MAGNET FUNCTIONALITY

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Magnet"))
        {
            Vector3 pullDir = (other.transform.position - transform.position);
            pullDir.y = 0.0f;
            pullDir = pullDir.normalized;

            rb.AddForce(pullDir * speed * magStrength);

        }
    }

    // ----------------------------------------

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            lives = lives - 1;
            SetCountText();
            if (lives < 1)
            {
                // Destroy the current object
                Destroy(gameObject);
                // Update the winText to display "You Lose!"
                winTextObject.gameObject.SetActive(true);
                winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";
            }
            transform.position = new Vector3(0, 0, 0);
            
            
        }
        if (collision.gameObject.CompareTag("SpeedUp"))
        {
            // Speeds up object
            StartCoroutine(ActivateSpeedUp());
           
        }
    }

    IEnumerator ActivateSpeedUp()
    {
        movementX += speedBoost;
        movementY += speedBoost;
        yield return new WaitForSeconds(speedDuration);
        movementX -= speedBoost;
        movementY -= speedBoost;
    }



}

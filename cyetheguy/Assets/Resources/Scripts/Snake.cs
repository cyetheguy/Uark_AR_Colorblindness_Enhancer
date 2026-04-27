using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public BoxCollider gridArea;
    public float MoveSpeed = 1.00f;
    public GameObject BodyPrefab;
    public int bodyGap = 1;

    private Vector3 faceDir =  Vector3.right;
    private List<GameObject> BodySegments = new List<GameObject>();
    private List<Vector3> PrevPos = new List<Vector3>();
    


    // Start is called before the first frame update
    void Start()
    {
        GrowSnake();

    }

    // Update is called once per frame
    void Update()
    {

        // logic for updating direction
        
    }

    void FixedUpdate()
    {
        Bounds bounds = gridArea.bounds;
        Vector3 position = transform.position + transform.forward * MoveSpeed;
        if (position.x > bounds.max.x) position.x = bounds.min.x;
        if (position.x < bounds.min.x) position.x = bounds.max.x;
        if (position.z > bounds.max.z) position.z = bounds.min.z;
        if (position.z < bounds.min.z) position.z = bounds.max.z;

        position.y = 1.0f;

        transform.position = position;

        PrevPos.Insert(0, transform.position);

        int index = 1;
        foreach (var body in BodySegments)
        {
            Vector3 point = PrevPos[Mathf.Min(index * bodyGap, PrevPos.Count-1)];
            body.transform.position = point;
            index++;
        }
    }

    public void GrowSnake()
    {
        GameObject bodyPart = Instantiate(BodyPrefab);
        BodySegments.Add(bodyPart);
    }

    public void turnLeft() { transform.Rotate(Vector3.up * -90); }
    public void turnRight() { transform.Rotate(Vector3.up * 90); }
}

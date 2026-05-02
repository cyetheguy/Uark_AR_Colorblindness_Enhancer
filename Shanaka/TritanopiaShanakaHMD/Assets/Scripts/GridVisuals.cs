using UnityEngine;

public class GridVisuals : MonoBehaviour
{
    public Material floorMaterial;
    public Material gridLineMaterial;
    public Material wallMaterial;

    void Start()
    {
        Invoke(nameof(Build), 0.2f);
    }

    void Build()
    {
        float s = GridManager.Instance.spacing;
        float half = s * 1.5f;
        float center = 0f;
        float pad = 0.25f;
        float floorSize = s * 3f + pad * 2f;
        float lineLen = floorSize;
        float lineThick = 0.05f;
        float lineTall = 0.025f;
        float lineY = lineTall / 2f + 0.015f;

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.parent = transform;
        floor.transform.position = new Vector3(0f, -0.05f, 0f);
        floor.transform.localScale = new Vector3(floorSize, 0.08f, floorSize);
        if (floorMaterial != null) floor.GetComponent<Renderer>().material = floorMaterial;
        Destroy(floor.GetComponent<Collider>());

        float[] linePos = new float[]
        {
        -half,
        -s / 2f,
         s / 2f,
         half
        };

        foreach (float pos in linePos)
        {
            GameObject h = GameObject.CreatePrimitive(PrimitiveType.Cube);
            h.name = "HLine";
            h.transform.parent = transform;
            h.transform.position = new Vector3(center, lineY, pos);
            h.transform.localScale = new Vector3(lineLen, lineTall, lineThick);
            if (gridLineMaterial != null) h.GetComponent<Renderer>().material = gridLineMaterial;
            Destroy(h.GetComponent<Collider>());

            GameObject v = GameObject.CreatePrimitive(PrimitiveType.Cube);
            v.name = "VLine";
            v.transform.parent = transform;
            v.transform.position = new Vector3(pos, lineY, center);
            v.transform.localScale = new Vector3(lineThick, lineTall, lineLen);
            if (gridLineMaterial != null) v.GetComponent<Renderer>().material = gridLineMaterial;
            Destroy(v.GetComponent<Collider>());
        }

        float wallH = 0.6f;
        float wallThick = 0.12f;
        float wallY = wallH / 2f;
        float wallOuter = half + pad + wallThick / 2f;
        float wallLen = s * 3f + pad * 2f + wallThick * 2f;

        GameObject ws = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ws.name = "WallS";
        ws.transform.parent = transform;
        ws.transform.position = new Vector3(0f, wallY, -wallOuter);
        ws.transform.localScale = new Vector3(wallLen, wallH, wallThick);
        if (wallMaterial != null) ws.GetComponent<Renderer>().material = wallMaterial;
        Destroy(ws.GetComponent<Collider>());

        GameObject wn = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wn.name = "WallN";
        wn.transform.parent = transform;
        wn.transform.position = new Vector3(0f, wallY, wallOuter);
        wn.transform.localScale = new Vector3(wallLen, wallH, wallThick);
        if (wallMaterial != null) wn.GetComponent<Renderer>().material = wallMaterial;
        Destroy(wn.GetComponent<Collider>());

        GameObject ww = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ww.name = "WallW";
        ww.transform.parent = transform;
        ww.transform.position = new Vector3(-wallOuter, wallY, 0f);
        ww.transform.localScale = new Vector3(wallThick, wallH, wallLen);
        if (wallMaterial != null) ww.GetComponent<Renderer>().material = wallMaterial;
        Destroy(ww.GetComponent<Collider>());

        GameObject we = GameObject.CreatePrimitive(PrimitiveType.Cube);
        we.name = "WallE";
        we.transform.parent = transform;
        we.transform.position = new Vector3(wallOuter, wallY, 0f);
        we.transform.localScale = new Vector3(wallThick, wallH, wallLen);
        if (wallMaterial != null) we.GetComponent<Renderer>().material = wallMaterial;
        Destroy(we.GetComponent<Collider>());
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// GameManager: Master state machine for "The Hidden Signal"
/// Spawns all objects procedurally, manages protanopia filter,
/// tracks puzzle state, and triggers the final color reveal.
/// 
/// SETUP: Attach to an empty GameObject in SampleScene.
/// Assign ovrCameraRig (your [BuildingBlock] Camera Rig transform).
/// Everything else is spawned at runtime.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Reference")]
    [Tooltip("Drag your [BuildingBlock] Camera Rig here")]
    public Transform ovrCameraRig;

    [Header("Prefabs")]
    [Tooltip("Drag your GrabbableCube prefab here")]
    public GameObject grabbableCubePrefab;

    [Header("Spawn Settings")]
    public float spawnRadius = 0.5f;
    public float spawnHeight = 1.2f;
    public float pedestalDistance = 0.5f;

    // ── Internal state ──────────────────────────────────────────────
    private ProtanopiaFilterController _filter;
    private List<ColorCube> _cubes = new();
    private List<Pedestal>  _pedestals = new();
    private TMPro.TextMeshProUGUI _hintLabel;
    private Transform        _hintCanvasTransform;
    private TMPro.TextMeshProUGUI _revealLabel;
    private bool            _gameComplete;

    // The final output code revealed when all cubes land correctly
    private const string FinalCode = "R2-G7-B4";

    // ── Colors ───────────────────────────────────────────────────────
    // True colors (visible after reveal)
    // True colors — shown before Let's Go (no LUT yet)
    public static readonly Color TrueRed   = new Color(0.8f,  0.15f, 0.1f);
    public static readonly Color TrueGreen = new Color(0.1f,  0.65f, 0.15f);
    public static readonly Color TrueBlue  = new Color(0.15f, 0.4f,  0.85f);

    // Protanopia colors — what LUT does to TrueRed/TrueGreen
    // TrueRed  (0.8, 0.15, 0.1) through LUT = (143, 143, 19) -> (0.56, 0.56, 0.07)
    // TrueGreen(0.1, 0.65, 0.15) through LUT = (136, 136, 77) -> (0.53, 0.53, 0.30)
    // Similar olive - hard to distinguish
    public static readonly Color ProtoRed   = new Color(0.56f, 0.56f, 0.07f);
    public static readonly Color ProtoGreen = new Color(0.53f, 0.53f, 0.30f);
    public static readonly Color ProtoBlue  = new Color(0.15f, 0.40f, 0.85f);

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (ovrCameraRig == null)
            ovrCameraRig = Camera.main?.transform.parent ?? Camera.main?.transform;

        // Show start screen immediately — Camera.main is most reliable on Quest
        Transform camTransform = Camera.main?.transform ?? ovrCameraRig;
        var startGo = new GameObject("StartScreenUI");
        var startScreen = startGo.AddComponent<StartScreenUI>();
        startScreen.Initialize(camTransform);
    }

    /// <summary>
    /// Called by StartScreenUI when the player presses "Let's Go".
    /// </summary>
    public void BeginGame()
    {
        _filter = gameObject.AddComponent<ProtanopiaFilterController>();
        _filter.Initialize();
        StartCoroutine(BeginSequence());
    }

    IEnumerator BeginSequence()
    {
        // Apply filter immediately
        _filter.ApplyImmediate();
        yield return null;
        StartCoroutine(InitSequence());
    }

    // Called after cubes spawn to apply protanopia colors
    public void ApplyProtanopiaColors()
    {
        foreach (var c in _cubes)
            c.ApplyProtanopiaColor();
        foreach (var p in _pedestals)
            p.ApplyProtanopiaColor();
    }

    IEnumerator InitSequence()
    {
        yield return new WaitForSeconds(0.5f);

        ShowHint("Grab the cubes and place them on\nthe matching pedestals.\nCan you tell them apart?");
        SpawnPedestals();
        SpawnCubes();
        ApplyProtanopiaColors();

        yield return new WaitForSeconds(3.5f);
        HideHint();
    }

    // ── Spawning ────────────────────────────────────────────────────

    void SpawnPedestals()
    {
        // Get camera XZ position and forward direction
        Transform cam = Camera.main?.transform;
        Vector3 camPos = cam != null ? cam.position : Vector3.zero;
        Vector3 fwd = cam != null ? cam.forward : Vector3.forward;
        fwd.y = 0f; fwd.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

        CubeColor[] types = { CubeColor.Red, CubeColor.Green, CubeColor.Blue };
        float spacing = 0.25f;
        float dist = 0.5f;
        float height = 0.7f;

        for (int i = 0; i < 3; i++)
        {
            float offset = (i - 1) * spacing; // -0.25, 0, +0.25
            Vector3 pos = new Vector3(camPos.x, 0f, camPos.z)
                        + fwd * dist
                        + right * offset
                        + Vector3.up * height;

            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = $"Pedestal_{types[i]}";
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.18f, 0.04f, 0.18f);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = GetTrueColor(types[i]) * 0.7f;
            go.GetComponent<Renderer>().material = mat;

            var pedestal = go.AddComponent<Pedestal>();
            pedestal.Initialize(types[i], mat);
            _pedestals.Add(pedestal);
        }
    }

    void SpawnCubes()
    {
        Transform cam = Camera.main?.transform;
        Vector3 camPos = cam != null ? cam.position : Vector3.zero;
        Vector3 fwd = cam != null ? cam.forward : Vector3.forward;
        fwd.y = 0f; fwd.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

        CubeColor[] types = { CubeColor.Red, CubeColor.Green, CubeColor.Blue };
        float spacing = 0.25f;
        float dist = 0.5f;
        float height = 0.95f;

        for (int i = 0; i < 3; i++)
        {
            float offset = (i - 1) * spacing; // -0.25, 0, +0.25
            Vector3 pos = new Vector3(camPos.x, 0f, camPos.z)
                        + fwd * dist
                        + right * offset
                        + Vector3.up * height;

            // Use prefab if available (recommended), otherwise create primitive
            GameObject go;
            if (grabbableCubePrefab != null)
            {
                go = Instantiate(grabbableCubePrefab, pos, Quaternion.identity);
                go.name = $"Cube_{types[i]}";
                go.transform.localScale = Vector3.one * 0.1f;
                // Do NOT touch rigidbody — let prefab's grab setup control it
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Cube_{types[i]}";
                go.transform.position = pos;
                go.transform.localScale = Vector3.one * 0.1f;
                var rb = go.AddComponent<Rigidbody>();
                rb.mass = 0.3f;
                rb.useGravity = false;
                rb.isKinematic = true;
                // Grab components handled by prefab — don't add them here
            }

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = GetTrueColor(types[i]);
            // Apply color only to main renderer
            var mainRend = go.GetComponent<Renderer>();
            if (mainRend != null) mainRend.material = mat;

            var cube = go.AddComponent<ColorCube>();
            cube.Initialize(types[i], mat);
            _cubes.Add(cube);
        }
    }

    void AddShapeMarker(Transform parent, CubeColor type)
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "ShapeMarker";
        quad.transform.SetParent(parent);
        quad.transform.localPosition = new Vector3(0, 0, -0.52f);
        quad.transform.localScale = Vector3.one * 0.7f;
        Destroy(quad.GetComponent<Collider>());

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.black;
        quad.GetComponent<Renderer>().material = mat;

        // TMP label on quad
        var labelGo = new GameObject("MarkerLabel");
        labelGo.transform.SetParent(quad.transform);
        labelGo.transform.localPosition = new Vector3(0, 0, -0.01f);
        labelGo.transform.localScale = Vector3.one * 0.08f;

        var tmp = labelGo.AddComponent<TextMeshPro>();
        tmp.text = type switch {
            CubeColor.Red   => "▲",
            CubeColor.Green => "■",
            CubeColor.Blue  => "●",
            _ => "?"
        };
        tmp.fontSize = 8;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }

    void TryAddGrabbable(GameObject go)
    {
        bool added = false;

        // Try all possible grabbable types in order
        string[] types = new string[]
        {
            "OVRGrabbable, Assembly-CSharp",
            "Oculus.Interaction.Grabbable, Oculus.Interaction",
            "Oculus.Interaction.HandGrab.HandGrabInteractable, Oculus.Interaction",
            "OVRGrabbable, Oculus.VR",
        };

        foreach (var typeName in types)
        {
            var t = System.Type.GetType(typeName);
            if (t != null)
            {
                try
                {
                    go.AddComponent(t);
                    Debug.Log($"[GameManager] Added {typeName} to {go.name}");
                    added = true;

                    // Add companion components needed by Oculus.Interaction.Grabbable
                    if (typeName.Contains("Oculus.Interaction.Grabbable"))
                    {
                        var rb = go.GetComponent<Rigidbody>();

                        // GrabInteractable — needed for controller grab
                        var grabType = System.Type.GetType("Oculus.Interaction.GrabInteractable, Oculus.Interaction");
                        if (grabType != null)
                        {
                            try
                            {
                                var grabComp = go.AddComponent(grabType);
                                // Assign rigidbody via reflection
                                var rbField = grabType.GetField("_rigidbody",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                if (rbField != null) rbField.SetValue(grabComp, rb);
                                var rbProp = grabType.GetProperty("Rigidbody");
                                if (rbProp != null) rbProp.SetValue(grabComp, rb);
                            }
                            catch (System.Exception e) { Debug.LogWarning($"GrabInteractable: {e.Message}"); }
                        }

                        // HandGrabInteractable — needed for hand grab/pinch
                        var handGrabType = System.Type.GetType("Oculus.Interaction.HandGrab.HandGrabInteractable, Oculus.Interaction");
                        if (handGrabType != null)
                        {
                            try
                            {
                                var handComp = go.AddComponent(handGrabType);
                                var rbField = handGrabType.GetField("_rigidbody",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                if (rbField != null) rbField.SetValue(handComp, rb);
                                var rbProp = handGrabType.GetProperty("Rigidbody");
                                if (rbProp != null) rbProp.SetValue(handComp, rb);
                            }
                            catch (System.Exception e) { Debug.LogWarning($"HandGrabInteractable: {e.Message}"); }
                        }
                    }
                    break;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[GameManager] Failed to add {typeName}: {e.Message}");
                }
            }
        }

        if (!added)
        {
            // Enable physics so cube can be knocked onto pedestal
            var rb = go.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            Debug.LogWarning($"[GameManager] No grab component found for {go.name} - using physics");

            #if UNITY_EDITOR
            go.AddComponent<SimpleDragInteractable>();
            #endif
        }
    }

    // ── Puzzle logic ─────────────────────────────────────────────────

    public void OnCubePlaced(CubeColor cubeColor, CubeColor pedestalColor, bool correct)
    {
        if (_gameComplete) return;

        if (!correct)
        {
            ShowHint("Not quite — the shapes don't match. Try again.");
            return;
        }

        // Check if all pedestals are filled correctly
        int solvedCount = 0;
        foreach (var p in _pedestals)
            if (p.IsSolved) solvedCount++;

        if (solvedCount >= 3)
            StartCoroutine(TriggerFinalReveal());
    }

    IEnumerator TriggerFinalReveal()
    {
        _gameComplete = true;
        HideHint();

        ShowHint("All placed! Something is shifting...");
        yield return new WaitForSeconds(1.5f);
        HideHint();

        // Restore true colors AND lift filter simultaneously
        foreach (var c in _cubes)     c.RevealTrueColor();
        foreach (var p in _pedestals) p.RevealTrueColor();
        yield return _filter.FadeOut(1.8f);

        yield return new WaitForSeconds(0.5f);

        // Light up solved pedestals — they emit the final code
        ShowFinalCode();
    }

    void ShowFinalCode()
    {
        // Use World Space Canvas — same approach as start screen
        var canvasGo = new GameObject("RevealCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();

        // Exact same size/scale as start panel
        var rt = canvasGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800f, 600f);
        canvasGo.transform.localScale = Vector3.one * 0.001f;

        // Exact same positioning as start panel (PositionCanvasInFrontOfPlayer)
        Transform cam = Camera.main?.transform;
        if (cam != null)
        {
            Vector3 fwd = cam.forward;
            fwd.y = 0f;
            if (fwd.magnitude < 0.01f) fwd = Vector3.forward;
            fwd.Normalize();
            canvasGo.transform.position = cam.position + fwd * 1.0f + Vector3.up * 0.1f;
            canvasGo.transform.rotation = Quaternion.LookRotation(fwd);
        }

        // Background
        var bg = new GameObject("BG");
        bg.transform.SetParent(canvasGo.transform, false);
        var bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Title
        CreateCanvasText(canvasGo.transform, "SIGNAL DECODED", new Vector2(0, 150), 55,
            new Color(1f, 0.85f, 0.2f), TMPro.FontStyles.Bold);

        // Code
        CreateCanvasText(canvasGo.transform, FinalCode, new Vector2(0, 50), 75,
            new Color(1f, 0.85f, 0.2f), TMPro.FontStyles.Bold);

        // Message
        CreateCanvasText(canvasGo.transform,
            "Now you know what protanopia feels like.\nYou have escaped.",
            new Vector2(0, -80), 28, Color.white);

        // Animate scale in
        StartCoroutine(ScaleIn(canvasGo.transform));

        // Emit glow on correctly placed cubes
        foreach (var c in _cubes)
            if (c.IsCorrectlyPlaced)
                c.EmitGlow();
    }

    void CreateCanvasText(Transform parent, string text, Vector2 pos, float fontSize,
        Color color, TMPro.FontStyles style = TMPro.FontStyles.Normal)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(750, 150);
    }

    IEnumerator ScaleIn(Transform t)
    {
        Vector3 targetScale = t.localScale == Vector3.zero ? Vector3.one * 0.001f : t.localScale;
        t.localScale = Vector3.zero;
        float elapsed = 0f;
        while (elapsed < 0.6f)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, elapsed / 0.6f);
            t.localScale = targetScale * s;
            yield return null;
        }
        t.localScale = targetScale;
    }

    // ── Helpers ──────────────────────────────────────────────────────

    public Color GetProtanopiaColor(CubeColor type) => type switch {
        CubeColor.Red   => ProtoRed,
        CubeColor.Green => ProtoGreen,
        CubeColor.Blue  => ProtoBlue,
        _ => Color.white
    };

    public Color GetTrueColor(CubeColor type) => type switch {
        CubeColor.Red   => TrueRed,
        CubeColor.Green => TrueGreen,
        CubeColor.Blue  => TrueBlue,
        _ => Color.white
    };

    void ShowHint(string msg)
    {
        if (_hintLabel == null)
        {
            // Use World Space Canvas — same as start/end panel
            var canvasGo = new GameObject("HintCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            var rt = canvasGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(800f, 200f);
            canvasGo.transform.localScale = Vector3.one * 0.001f;

            var textGo = new GameObject("HintText");
            textGo.transform.SetParent(canvasGo.transform, false);
            _hintLabel = textGo.AddComponent<TMPro.TextMeshProUGUI>();
            _hintLabel.fontSize = 40f;
            _hintLabel.alignment = TextAlignmentOptions.Center;
            _hintLabel.color = Color.white;
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.sizeDelta = new Vector2(780f, 180f);
            textRt.anchoredPosition = Vector2.zero;

            _hintCanvasTransform = canvasGo.transform;
        }

        // Position same as start/end panel
        Transform cam = Camera.main?.transform;
        if (cam != null)
        {
            Vector3 fwd = cam.forward;
            fwd.y = 0f;
            if (fwd.magnitude < 0.01f) fwd = Vector3.forward;
            fwd.Normalize();
            _hintCanvasTransform.position = cam.position + fwd * 1.0f + Vector3.up * 0.4f;
            _hintCanvasTransform.rotation = Quaternion.LookRotation(fwd);
        }

        _hintLabel.text = msg;
        _hintCanvasTransform.gameObject.SetActive(true);
    }

    void HideHint()
    {
        if (_hintCanvasTransform != null)
            _hintCanvasTransform.gameObject.SetActive(false);
    }



    TextMeshPro CreateWorldLabel(string text, Vector3 pos, float fontSize, Color color)
    {
        var go = new GameObject("WorldLabel");
        go.transform.position = pos;
        if (ovrCameraRig != null)
            go.transform.rotation = Quaternion.LookRotation(pos - ovrCameraRig.position);

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        return tmp;
    }
}

public enum CubeColor { Red, Green, Blue }

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// StartScreenUI: Uses a World Space Canvas which is guaranteed to render
/// correctly in Meta Quest passthrough mode.
/// </summary>
public class StartScreenUI : MonoBehaviour
{
    private Transform _cameraRig;
    private GameObject _canvasGo;
    private bool _started;

    private const float CanvasWidth  = 800f;
    private const float CanvasHeight = 600f;
    private const float CanvasScale  = 0.001f; // 1 unit = 1mm, so 800x600 = 0.8x0.6m
    private const float PanelDist    = 1.0f;

    public void Initialize(Transform cameraRig)
    {
        _cameraRig = cameraRig;
        StartCoroutine(SpawnAfterDelay(2.5f));
    }

    IEnumerator SpawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        BuildCanvas();
    }

    void BuildCanvas()
    {
        // ── World Space Canvas ───────────────────────────────────────
        _canvasGo = new GameObject("StartCanvas");
        var canvas = _canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var cr = _canvasGo.AddComponent<CanvasRenderer>();
        var cs = _canvasGo.AddComponent<CanvasScaler>();
        cs.dynamicPixelsPerUnit = 10f;

        // Required for UI raycasting to work
        _canvasGo.AddComponent<GraphicRaycaster>();

        // EventSystem with OVRInputModule for Meta ray interaction
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();

            // Try OVRInputModule first (Meta Building Blocks ray)
            var ovrInputType = System.Type.GetType("OVRInputModule, Assembly-CSharp");
            if (ovrInputType != null)
                esGo.AddComponent(ovrInputType);
            else
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        var rt = _canvasGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(CanvasWidth, CanvasHeight);
        _canvasGo.transform.localScale = Vector3.one * CanvasScale;

        // ── Background panel ─────────────────────────────────────────
        var bg = new GameObject("Background");
        bg.transform.SetParent(_canvasGo.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // ── Title ────────────────────────────────────────────────────
        CreateText("THE HIDDEN SIGNAL", new Vector2(0, 180), 60,
            new Color(1f, 0.85f, 0.2f), FontStyles.Bold);

        // ── Subtitle ─────────────────────────────────────────────────
        CreateText("A Protanopia Experience", new Vector2(0, 110), 30,
            new Color(0.8f, 0.8f, 0.8f));

        // ── Divider ──────────────────────────────────────────────────
        var div = new GameObject("Divider");
        div.transform.SetParent(_canvasGo.transform, false);
        var divImg = div.AddComponent<Image>();
        divImg.color = new Color(1f, 0.85f, 0.2f, 0.6f);
        var divRt = div.GetComponent<RectTransform>();
        divRt.anchoredPosition = new Vector2(0, 70);
        divRt.sizeDelta = new Vector2(600, 3);

        // ── Instructions ─────────────────────────────────────────────
        CreateText(
            "You see the world through the eyes of\n" +
            "someone with red-green colorblindness.\n\n" +
            "Grab the cubes and place them on the\n" +
            "matching pedestals using the shape hints.\n\n" +
            "Can you escape before the signal fades?",
            new Vector2(0, -50), 26, new Color(0.88f, 0.88f, 0.88f));

        // ── Button ───────────────────────────────────────────────────
        var btnGo = new GameObject("LetsGoButton");
        btnGo.transform.SetParent(_canvasGo.transform, false);

        var btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(1f, 0.75f, 0.1f);

        var btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.anchoredPosition = new Vector2(0, -230);
        btnRt.sizeDelta = new Vector2(250, 70);

        var btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(OnLetsGoPressed);

        // Button label
        var btnLabel = new GameObject("BtnText");
        btnLabel.transform.SetParent(btnGo.transform, false);
        var btnTmp = btnLabel.AddComponent<TextMeshProUGUI>();
        btnTmp.text = "LET'S GO";
        btnTmp.fontSize = 32;
        btnTmp.fontStyle = FontStyles.Bold;
        btnTmp.alignment = TextAlignmentOptions.Center;
        btnTmp.color = new Color(0.1f, 0.05f, 0f);
        var btnTmpRt = btnLabel.GetComponent<RectTransform>();
        btnTmpRt.anchorMin = Vector2.zero;
        btnTmpRt.anchorMax = Vector2.one;
        btnTmpRt.offsetMin = Vector2.zero;
        btnTmpRt.offsetMax = Vector2.zero;

        // Add collider for hand/controller interaction
        var collider = btnGo.AddComponent<BoxCollider>();
        collider.size = new Vector3(250, 70, 80); // thick Z for easy poke

        // Add StartButton for physical poke interaction
        var startBtn = btnGo.AddComponent<StartButton>();
        startBtn.Initialize(this, btnImg);

        // Position once in front of player and stay fixed
        PositionCanvasInFrontOfPlayer();
    }

    void CreateText(string text, Vector2 pos, float fontSize, Color color,
                    FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(_canvasGo.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(700, 200);
    }

    void PositionCanvasInFrontOfPlayer()
    {
        Transform cam = Camera.main?.transform;
        if (cam != null)
        {
            Vector3 forward = cam.forward;
            forward.y = 0f;
            if (forward.magnitude < 0.01f) forward = Vector3.forward;
            forward.Normalize();

            _canvasGo.transform.position = cam.position + forward * PanelDist + Vector3.up * 0.1f;
            _canvasGo.transform.rotation = Quaternion.LookRotation(forward);
        }
    }

    public void OnLetsGoPressed()
    {
        if (_started) return;
        _started = true;
        StartCoroutine(DismissAndStart());
    }

    IEnumerator DismissAndStart()
    {
        float e = 0f;
        Vector3 startScale = _canvasGo.transform.localScale;
        while (e < 0.3f)
        {
            e += Time.deltaTime;
            float s = Mathf.SmoothStep(1f, 0f, e / 0.3f);
            _canvasGo.transform.localScale = startScale * s;
            yield return null;
        }
        Destroy(_canvasGo);
        GameManager.Instance.BeginGame();
        Destroy(gameObject);
    }
}

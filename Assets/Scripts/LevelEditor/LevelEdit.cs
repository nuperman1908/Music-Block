using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-50)]
public class LevelEdit : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private Transform ground;

    [Header("Grid")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private int gridExtent = 60;
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.12f);
    [SerializeField] private float gridZ = 0.5f;

    [Header("Camera Pan (right-mouse drag)")]
    [SerializeField] private Camera editorCamera;
    [SerializeField] private int panMouseButton = 1;
    [SerializeField] private bool disableCameraFollow = true;

    [Header("Camera Zoom (mouse wheel)")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minOrthoSize = 2f;
    [SerializeField] private float maxOrthoSize = 30f;

    private Vector3 _lastMousePos;
    private bool _isPanning;
    [SerializeField]  private GameObject _gridGo;

    private void Start()
    {
        ground.position = new Vector3(editorCamera.transform.position.x, 0f, 0f);
        if (editorCamera == null) editorCamera = Camera.main;

        if (disableCameraFollow && editorCamera != null)
        {
            CameraFollow follow = editorCamera.GetComponent<CameraFollow>();
            if (follow != null) follow.enabled = false;
        }

        if (showGrid) BuildGrid();
    }

    private void OnDestroy()
    {
        if (_gridGo != null) Destroy(_gridGo);
    }

    private void Update()
    {
        if (_gridGo)
        {
            _gridGo.transform.position = new Vector3((int)editorCamera.transform.position.x, (int)editorCamera.transform.position.y, 0);
        }
        if (editorCamera == null) return;
        ground.position = new Vector3(editorCamera.transform.position.x, 0f, 0f);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LevelEditorManager.Instance.settingMenu.SetActive(LevelEditorManager.Instance.settingMenu.activeSelf ? false : true);
        }

        if (Input.GetMouseButtonDown(panMouseButton))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            _isPanning = true;
            _lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(panMouseButton))
            _isPanning = false;

        if (_isPanning && Input.GetMouseButton(panMouseButton))
        {
            Vector3 cur = Input.mousePosition;
            Vector3 delta = cur - _lastMousePos;

            float worldH = editorCamera.orthographic
                ? 2f * editorCamera.orthographicSize
                : 2f * Mathf.Abs(editorCamera.transform.position.z) *
                  Mathf.Tan(editorCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float worldW = worldH * editorCamera.aspect;

            Vector3 worldDelta = new Vector3(
                -delta.x * worldW / Screen.width,
                -delta.y * worldH / Screen.height,
                0f);

            Vector3 newPos = editorCamera.transform.position + worldDelta;
            if (newPos.y < -2f) newPos.y = -2f;
            editorCamera.transform.position = newPos;
            _lastMousePos = cur;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (!overUI)
            {
                if (editorCamera.orthographic)
                {
                    float size = editorCamera.orthographicSize - scroll * zoomSpeed;
                    editorCamera.orthographicSize = Mathf.Clamp(size, minOrthoSize, maxOrthoSize);
                }
                else
                {
                    editorCamera.transform.position += editorCamera.transform.forward * (scroll * zoomSpeed);
                }
            }
        }
    }

    private void BuildGrid()
    {
        int half = gridExtent;
        int totalLines = (half * 2 + 1) * 2;
        Vector3[] verts = new Vector3[totalLines * 2];
        int[] idx = new int[totalLines * 2];
        int v = 0;

        // Cell centers sit on integer coords, so lines are offset by 0.5 to bound each cell.
        const float off = 0.5f;

        for (int x = -half; x <= half; x++)
        {
            verts[v] = new Vector3(x - off, -half - off, 0f);
            verts[v + 1] = new Vector3(x - off, half + off, 0f);
            idx[v] = v; idx[v + 1] = v + 1;
            v += 2;
        }
        for (int y = -half; y <= half; y++)
        {
            verts[v] = new Vector3(-half - off, y - off, 0f);
            verts[v + 1] = new Vector3(half + off, y - off, 0f);
            idx[v] = v; idx[v + 1] = v + 1;
            v += 2;
        }

        Mesh mesh = new Mesh { name = "EditorGridMesh" };
        mesh.vertices = verts;
        mesh.SetIndices(idx, MeshTopology.Lines, 0);
        mesh.RecalculateBounds();

        _gridGo = new GameObject("EditorGrid");
        _gridGo.transform.SetParent(transform, false);
        _gridGo.transform.position = new Vector3(0f, 0f, gridZ);

        MeshFilter mf = _gridGo.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        MeshRenderer mr = _gridGo.AddComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        mr.sharedMaterial = CreateGridMaterial();
    }

    private Material CreateGridMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Transparent");

        Material mat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        mat.color = gridColor;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", gridColor);
        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
        mat.renderQueue = 3000;
        return mat;
    }
}
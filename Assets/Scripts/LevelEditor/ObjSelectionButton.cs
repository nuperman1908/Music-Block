using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ObjSelectionButton : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform spawnParent;
    [SerializeField] private float ghostAlpha = 0.5f;
    [SerializeField] private bool singleInstance;

    private static GameObject _selectedPrefab;
    private static GameObject _previewObj;
    private static GameObject _previewPrefab;
    private static float _previewRotZ;
    private static Vector2 _lastPaintPos = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
    private static readonly List<GameObject> _placed = new List<GameObject>();

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Select);
        spawnParent = GameObject.Find("ObjContainer").transform;
    }

    private void OnDisable()
    {
        if (_selectedPrefab == prefab) DestroyPreview();
    }

    private void Select()
    {
        _selectedPrefab = prefab;
        EnsurePreview();
    }

    private void Update()
    {
        if (_selectedPrefab != prefab) return;

        EnsurePreview();

        if (Input.GetKeyDown(KeyCode.R))
            _previewRotZ = Mathf.Repeat(_previewRotZ + 90f, 360f);

        Camera cam = Camera.main;
        if (cam == null) return;

        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        bool snap = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        Vector2 pos = snap
            ? new Vector2(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y))
            : new Vector2(world.x, world.y);
        bool placeable = !overUI && pos.y >= -2f;

        if (_previewObj != null)
        {
            _previewObj.SetActive(placeable);
            if (placeable)
            {
                _previewObj.transform.SetPositionAndRotation(
                    new Vector3(pos.x, pos.y, 0f),
                    Quaternion.Euler(0f, 0f, _previewRotZ));
            }
        }

        if (Input.GetKey(KeyCode.D))
        {
            if (!overUI)
            {
                GameObject toDelete = FindNearestPlaced(pos, 0.5f);
                if (toDelete != null)
                {
                    _placed.Remove(toDelete);
                    Destroy(toDelete);
                }
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
            _lastPaintPos = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        if (!Input.GetMouseButton(0)) return;
        if (!placeable) return;
        if (Vector2.Distance(pos, _lastPaintPos) < 0.5f) return;

        for (int i = 0; i < _placed.Count; i++)
        {
            GameObject go = _placed[i];
            if (go == null) continue;
            if (!IsSamePrefab(go.name, prefab.name)) continue;
            if (((Vector2)go.transform.position - pos).sqrMagnitude < 1e-6f) return;
        }

        GameObject existing = FindNearestPlaced(pos, 0.5f);
        if (existing != null && !IsStackable(prefab.name) && !IsStackable(existing.name))
        {
            _placed.Remove(existing);
            Destroy(existing);
        }

        if (singleInstance) RemoveAllOfPrefab(prefab.name);

        GameObject obj = Instantiate(prefab, new Vector3(pos.x, pos.y, 0f),
            Quaternion.Euler(0f, 0f, _previewRotZ), spawnParent);
        _placed.Add(obj);
        _lastPaintPos = pos;
    }

    private static bool IsStackable(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        return name.StartsWith("Orb") || name.StartsWith("JumpPad") || name.Contains("Portal");
    }

    private static bool IsSamePrefab(string instanceName, string prefabName)
    {
        if (string.IsNullOrEmpty(instanceName) || string.IsNullOrEmpty(prefabName)) return false;
        int idx = instanceName.IndexOf("(Clone)");
        string baseName = idx >= 0 ? instanceName.Substring(0, idx) : instanceName;
        return baseName == prefabName;
    }

    private static void RemoveAllOfPrefab(string prefabName)
    {
        for (int i = _placed.Count - 1; i >= 0; i--)
        {
            GameObject go = _placed[i];
            if (go == null) { _placed.RemoveAt(i); continue; }
            if (!IsSamePrefab(go.name, prefabName)) continue;
            _placed.RemoveAt(i);
            Destroy(go);
        }
    }

    private static GameObject FindNearestPlaced(Vector2 pos, float radius)
    {
        GameObject best = null;
        float bestDist = radius;
        for (int i = _placed.Count - 1; i >= 0; i--)
        {
            GameObject go = _placed[i];
            if (go == null) { _placed.RemoveAt(i); continue; }
            float d = Vector2.Distance(pos, go.transform.position);
            if (d < bestDist) { bestDist = d; best = go; }
        }
        return best;
    }

    private void EnsurePreview()
    {
        if (_previewPrefab == prefab && _previewObj != null) return;
        DestroyPreview();
        if (prefab == null) return;
        _previewObj = Instantiate(prefab);
        _previewObj.name = prefab.name + " [Preview]";
        _previewObj.transform.rotation = Quaternion.Euler(0f, 0f, _previewRotZ);
        ApplyGhostStyle(_previewObj);
        _previewPrefab = prefab;
    }

    private static void DestroyPreview()
    {
        if (_previewObj != null) Destroy(_previewObj);
        _previewObj = null;
        _previewPrefab = null;
    }

    private void ApplyGhostStyle(GameObject go)
    {
        foreach (Collider2D c in go.GetComponentsInChildren<Collider2D>(true)) c.enabled = false;
        foreach (Collider c in go.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        foreach (Rigidbody2D rb in go.GetComponentsInChildren<Rigidbody2D>(true)) rb.simulated = false;
        foreach (Rigidbody rb in go.GetComponentsInChildren<Rigidbody>(true)) rb.isKinematic = true;
        foreach (MonoBehaviour mb in go.GetComponentsInChildren<MonoBehaviour>(true)) mb.enabled = false;

        foreach (SpriteRenderer sr in go.GetComponentsInChildren<SpriteRenderer>(true))
        {
            Color c = sr.color; c.a = ghostAlpha; sr.color = c;
        }
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
        {
            if (r is SpriteRenderer) continue;
            foreach (Material m in r.materials)
            {
                if (m.HasProperty("_Color"))
                {
                    Color c = m.color; c.a = ghostAlpha; m.color = c;
                }
                if (m.HasProperty("_BaseColor"))
                {
                    Color c = m.GetColor("_BaseColor"); c.a = ghostAlpha; m.SetColor("_BaseColor", c);
                }
            }
        }
    }
}

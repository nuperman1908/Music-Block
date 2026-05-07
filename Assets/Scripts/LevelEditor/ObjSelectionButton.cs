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

    private static GameObject _selectedPrefab;
    private static GameObject _previewObj;
    private static GameObject _previewPrefab;
    private static float _previewRotZ;
    private static Vector2Int _lastPaintCell = new Vector2Int(int.MinValue, int.MinValue);
    private static readonly Dictionary<Vector2Int, GameObject> _placed = new Dictionary<Vector2Int, GameObject>();

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
        Vector2Int cell = new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
        bool placeable = !overUI && cell.y >= -2;

        if (_previewObj != null)
        {
            _previewObj.SetActive(placeable);
            if (placeable)
            {
                _previewObj.transform.SetPositionAndRotation(
                    new Vector3(cell.x, cell.y, 0f),
                    Quaternion.Euler(0f, 0f, _previewRotZ));
            }
        }

        if (Input.GetKey(KeyCode.D))
        {
            if (!overUI && _placed.TryGetValue(cell, out GameObject toDelete))
            {
                if (toDelete != null) Destroy(toDelete);
                _placed.Remove(cell);
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
            _lastPaintCell = new Vector2Int(int.MinValue, int.MinValue);

        if (!Input.GetMouseButton(0)) return;
        if (!placeable) return;
        if (cell == _lastPaintCell) return;

        if (_placed.TryGetValue(cell, out GameObject existing))
        {
            if (existing != null) Destroy(existing);
            _placed.Remove(cell);
        }

        GameObject obj = Instantiate(prefab, new Vector3(cell.x, cell.y, 0f),
            Quaternion.Euler(0f, 0f, _previewRotZ), spawnParent);
        _placed[cell] = obj;
        _lastPaintCell = cell;
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

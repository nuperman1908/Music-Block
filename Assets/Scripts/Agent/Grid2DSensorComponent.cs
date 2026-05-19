using Unity.MLAgents.Sensors;
using UnityEngine;

public class Grid2DSensorComponent : SensorComponent
{
    [Header("Sensor")]
    public string sensorName = "Grid2DSensor";

    [Header("Grid Settings")]
    public Vector2 cellScale = Vector2.one;
    public Vector2Int gridSize = new Vector2Int(20, 15);
    public Vector2 gridOffset = Vector2.zero;
    public bool rotateWithAgent = false;

    [Header("Detection")]
    public string[] detectableTags = new string[0];
    public LayerMask colliderMask = ~0;
    public bool detectTriggers = true;

    [Header("Debug")]
    public bool showGizmos = true;
    public Color[] debugColors;

    internal int[,] LastDetected;

    public override ISensor[] CreateSensors()
    {
        if (detectableTags == null || detectableTags.Length == 0)
        {
            Debug.LogWarning($"[{sensorName}] No detectable tags configured.");
            return new ISensor[0];
        }
        LastDetected = new int[gridSize.x, gridSize.y];
        for (int x = 0; x < gridSize.x; x++)
            for (int y = 0; y < gridSize.y; y++)
                LastDetected[x, y] = -1;

        return new ISensor[] { new Grid2DSensor(this) };
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        float angle = rotateWithAgent ? transform.eulerAngles.z : 0f;
        Quaternion rot = Quaternion.Euler(0, 0, angle);
        Vector3 origin = transform.position + rot * (Vector3)gridOffset;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 local = new Vector3(
                    (x - gridSize.x * 0.5f + 0.5f) * cellScale.x,
                    (y - gridSize.y * 0.5f + 0.5f) * cellScale.y,
                    0);
                Vector3 worldPos = origin + rot * local;

                Color col = new Color(1, 1, 1, 0.08f);
                if (LastDetected != null && Application.isPlaying)
                {
                    int tagIdx = LastDetected[x, y];
                    if (tagIdx >= 0 && debugColors != null && tagIdx < debugColors.Length)
                    {
                        col = debugColors[tagIdx];
                        col.a = 0.55f;
                    }
                }
                Gizmos.color = col;
                Gizmos.matrix = Matrix4x4.TRS(worldPos, rot, Vector3.one);
                Gizmos.DrawCube(Vector3.zero, new Vector3(cellScale.x * 0.95f, cellScale.y * 0.95f, 0.01f));
                Gizmos.color = new Color(col.r, col.g, col.b, 0.6f);
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(cellScale.x, cellScale.y, 0.01f));
            }
        }
        Gizmos.matrix = Matrix4x4.identity;
    }
}

public class Grid2DSensor : ISensor
{
    private readonly Grid2DSensorComponent _cfg;
    private readonly int _channels;
    private readonly int _width;
    private readonly int _height;
    private readonly float[,,] _obs;
    private readonly Collider2D[] _hitBuffer = new Collider2D[8];

    public Grid2DSensor(Grid2DSensorComponent cfg)
    {
        _cfg = cfg;
        _channels = cfg.detectableTags.Length;
        _width = cfg.gridSize.x;
        _height = cfg.gridSize.y;
        _obs = new float[_width, _height, _channels];
    }

    public ObservationSpec GetObservationSpec()
    {
        return ObservationSpec.Visual(_height, _width, _channels);
    }

    public int Write(ObservationWriter writer)
    {
        UpdateGrid();
        int count = 0;
        for (int h = 0; h < _height; h++)
        {
            for (int w = 0; w < _width; w++)
            {
                for (int c = 0; c < _channels; c++)
                {
                    writer[h, w, c] = _obs[w, h, c];
                    count++;
                }
            }
        }
        return count;
    }

    public byte[] GetCompressedObservation() => null;
    public CompressionSpec GetCompressionSpec() => CompressionSpec.Default();
    public void Update() { }
    public void Reset() { System.Array.Clear(_obs, 0, _obs.Length); }
    public string GetName() => _cfg.sensorName;

    private void UpdateGrid()
    {
        System.Array.Clear(_obs, 0, _obs.Length);

        var t = _cfg.transform;
        float angle = _cfg.rotateWithAgent ? t.eulerAngles.z : 0f;
        float rad = angle * Mathf.Deg2Rad;
        float cosA = Mathf.Cos(rad);
        float sinA = Mathf.Sin(rad);
        Vector2 off = _cfg.gridOffset;
        Vector2 origin = (Vector2)t.position + new Vector2(cosA * off.x - sinA * off.y, sinA * off.x + cosA * off.y);

        ContactFilter2D filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = _cfg.colliderMask,
            useTriggers = _cfg.detectTriggers
        };

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                float lx = (x - _width * 0.5f + 0.5f) * _cfg.cellScale.x;
                float ly = (y - _height * 0.5f + 0.5f) * _cfg.cellScale.y;
                Vector2 worldPos = origin + new Vector2(cosA * lx - sinA * ly, sinA * lx + cosA * ly);

                int count = Physics2D.OverlapBox(worldPos, _cfg.cellScale, angle, filter, _hitBuffer);
                int firstDetected = -1;
                for (int i = 0; i < count; i++)
                {
                    string tag = _hitBuffer[i].tag;
                    for (int ti = 0; ti < _channels; ti++)
                    {
                        if (tag == _cfg.detectableTags[ti])
                        {
                            _obs[x, y, ti] = 1f;
                            if (firstDetected < 0) firstDetected = ti;
                            break;
                        }
                    }
                }
                if (_cfg.LastDetected != null) _cfg.LastDetected[x, y] = firstDetected;
            }
        }
    }
}

using UnityEngine;

namespace Nature
{
    [ExecuteInEditMode]
    public class FlagController : MonoBehaviour
    {
        Mesh mesh;
        Vector3[] vertices;
        Vector2[] uv;
        int[] triangles;
        [SerializeField] Sprite flag;
        [Range(2, 100)]
        [SerializeField] private int horizontalPoints = 10;
        [Range(2, 100)]
        [SerializeField] private int verticalPoints = 5;

        private int prevHorizontalPoints;
        private int prevVerticalPoints;

        void OnEnable()
        {
            if (GetComponent<SpriteRenderer>())
            {
                flag = GetComponent<SpriteRenderer>().sprite;
                DestroyImmediate(GetComponent<SpriteRenderer>());
            }
            if (!GetComponent<MeshFilter>())
            {
                gameObject.AddComponent<MeshFilter>();
            }
            if (!GetComponent<MeshRenderer>())
            {
                gameObject.AddComponent<MeshRenderer>();
            }
        }
        void Update()
        {
            if (!Application.isPlaying)
            {
                if (flag != null)
                {
                    if (GetComponent<MeshRenderer>().sharedMaterial && GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_MainTex"))
                    {
                        if (flag.texture != GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_MainTex"))
                        {
                            GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", flag.texture);
                            GenerateMesh();
                        }
                    }
                    if (prevHorizontalPoints != horizontalPoints || prevVerticalPoints != verticalPoints)
                    {
                        GenerateMesh();
                        prevHorizontalPoints = horizontalPoints;
                        prevVerticalPoints = verticalPoints;
                    }
                }
            }
        }
        void GenerateMesh()
        {
            float textureWidth = flag.bounds.size.x;
            float textureHeight = flag.bounds.size.y;

            int vertexCount = horizontalPoints * verticalPoints;
            vertices = new Vector3[vertexCount];
            uv = new Vector2[vertexCount];

            float frameWidth = textureWidth / (horizontalPoints - 1);
            float frameHeight = textureHeight / (verticalPoints - 1);

            for (int y = 0; y < verticalPoints; y++)
            {
                for (int x = 0; x < horizontalPoints; x++)
                {
                    int index = y * horizontalPoints + x;
                    float xPos = x * frameWidth;
                    float yPos = y * frameHeight;
                    vertices[index] = new Vector3(xPos, yPos);
                    uv[index] = new Vector2(xPos / textureWidth, yPos / textureHeight);
                }
            }

            int numQuads = (horizontalPoints - 1) * (verticalPoints - 1);
            int numTriangles = numQuads * 6;
            triangles = new int[numTriangles];

            int t = 0;
            for (int y = 0; y < verticalPoints - 1; y++)
            {
                for (int x = 0; x < horizontalPoints - 1; x++)
                {
                    int topLeft = y * horizontalPoints + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = (y + 1) * horizontalPoints + x;
                    int bottomRight = bottomLeft + 1;

                    triangles[t] = topLeft;
                    triangles[t + 1] = bottomLeft;
                    triangles[t + 2] = topRight;
                    triangles[t + 3] = topRight;
                    triangles[t + 4] = bottomLeft;
                    triangles[t + 5] = bottomRight;

                    t += 6;
                }
            }

            mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            GetComponent<MeshFilter>().mesh = mesh;
        }
    }
}
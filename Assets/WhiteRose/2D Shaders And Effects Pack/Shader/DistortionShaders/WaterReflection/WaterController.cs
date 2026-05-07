using UnityEngine;

namespace Distortion
{
    [ExecuteInEditMode]
    public class WaterController : MonoBehaviour
    {
        [SerializeField] private Camera reflectionCamera;
        [SerializeField] private SpriteRenderer water;
        [SerializeField] private SpriteRenderer mask;
        [SerializeField] private float maskOffset = 0;
        private float ortSize = 5;
        private float camHeight;
        private float waterHeight;
        void Update()
        {
            if (!Application.isPlaying)
            {
                if (reflectionCamera == null)
                {
                    foreach (Transform child in transform)
                    {
                        if (child.GetComponent<Camera>())
                        {
                            child.name = "ReflectionCamera";
                            child.GetComponent<Camera>().orthographic = true;
                            reflectionCamera = child.GetComponent<Camera>();
                        }
                    }
                    if (reflectionCamera == null)
                    {
                        GameObject obj = new GameObject("ReflectionCamera");
                        obj.transform.parent = transform;
                        obj.AddComponent<Camera>();
                        obj.GetComponent<Camera>().orthographic = true;
                        reflectionCamera = obj.GetComponent<Camera>();
                    }
                }
                if (water == null)
                {
                    water = GetComponent<SpriteRenderer>();
                }
                else if (water.sharedMaterial != null && water.sharedMaterial.HasTexture("_RenderTexture"))
                {
                    if (water.sharedMaterial.GetTexture("_RenderTexture").GetType() == typeof(RenderTexture))
                    {
                        if (water.sharedMaterial.GetInt("_Reflection") == 1)
                        {
                            if (reflectionCamera.targetTexture != water.sharedMaterial.GetTexture("_RenderTexture"))
                            {
                                reflectionCamera.gameObject.SetActive(true);
                                reflectionCamera.targetTexture = (RenderTexture)water.sharedMaterial.GetTexture("_RenderTexture");
                            }
                            //WithWaterSize();
                        }
                        else
                        {
                            reflectionCamera.targetTexture = null;
                            reflectionCamera.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        reflectionCamera.targetTexture = null;
                        reflectionCamera.gameObject.SetActive(false);
                    }
                    WithWaterSize();
                }
            }
        }
        private void WithWaterSize()
        {
            water.gameObject.transform.localScale = new Vector3(1, 1, 1);
            water.drawMode = SpriteDrawMode.Sliced;

            waterHeight = water.size.y / 2;


            reflectionCamera.orthographicSize = water.size.x / 3.57f;

            ortSize = reflectionCamera.orthographicSize;
            camHeight = 2f * ortSize;

            water.size = new Vector2(water.size.x, camHeight);

            reflectionCamera.transform.localPosition = new Vector3(0, (waterHeight + camHeight / 2), -10);

            if (mask != null)
            {
                if (mask.sprite.texture.isReadable)
                {
                    Texture2D tex;
                    tex = mask.sprite.texture;
                    GetTexSize(tex);
                    mask.transform.localPosition = new Vector2(0, reflectionCamera.transform.localPosition.y - (camHeight / 2 + (GetTexSize(tex) / 2) * mask.transform.localScale.y) - (maskOffset));
                }
                else
                {
                    Debug.LogWarning("Read/Write of the Mask Texture must be enabled");
                }
            }
        }
        private float GetTexSize(Texture2D texture)
        {
            int minX = texture.width;
            int minY = texture.height;
            int maxX = 0;
            int maxY = 0;

            Color32[] pixels = texture.GetPixels32();

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color32 pixel = pixels[y * texture.width + x];
                    if (pixel.a > 0)
                    {
                        if (x < minX)
                            minX = x;
                        if (x > maxX)
                            maxX = x;
                        if (y < minY)
                            minY = y;
                        if (y > maxY)
                            maxY = y;
                    }
                }
            }

            int width = maxX - minX + 1;
            int height = maxY - minY + 1;

            float pixelPerUnit = mask.sprite.pixelsPerUnit;

            return (float)height / pixelPerUnit;
        }
    }
}

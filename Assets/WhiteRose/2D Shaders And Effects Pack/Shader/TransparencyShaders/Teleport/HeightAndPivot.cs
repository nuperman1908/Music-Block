
using UnityEngine;

namespace Transparency
{
    [ExecuteInEditMode]
    public class HeightAndPivot : MonoBehaviour
    {
        private SpriteRenderer mainTexture;
        private float height;
        private float pivot;

        private void Update()
        {
            if (!Application.isPlaying)
            {
                if (mainTexture == null)
                {
                    if (GetComponent<SpriteRenderer>())
                    {
                        mainTexture = GetComponent<SpriteRenderer>();
                    }
                }
                else if (mainTexture.sharedMaterial != null && mainTexture.sharedMaterial.HasProperty("_TextureHeight") && mainTexture.sharedMaterial.HasProperty("_PivotY"))
                {
                    if (mainTexture.sharedMaterial.GetFloat("_TextureHeight") != height || mainTexture.sharedMaterial.GetFloat("_PivotY") != pivot)
                    {
                        SetValues();
                    }
                }
            }
        }
        private void SetValues()
        {
            height = mainTexture.bounds.size.y;
            pivot = Mathf.Abs((mainTexture.sprite.bounds.center.y / height) - 0.5f);
            mainTexture.sharedMaterial.SetFloat("_TextureHeight", height);
            mainTexture.sharedMaterial.SetFloat("_PivotY", pivot);
        }
    }
}
using UnityEngine;

namespace Outline
{
    public class UnifiedOutline : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer parentRenderer;
        private void Start()
        {
            if (parentRenderer != null && GetComponent<SpriteRenderer>())
            {
                GetComponent<SpriteRenderer>().sortingLayerID = 0;
                GetComponent<SpriteRenderer>().sortingOrder = parentRenderer.sortingOrder - 1;
            }
            else if (GetComponent<SpriteRenderer>())
            {
                GetComponent<SpriteRenderer>().sortingLayerID = 0;
                GetComponent<SpriteRenderer>().sortingOrder = 0;
            }
        }
        private void Update()
        {
            if (parentRenderer != null && GetComponent<SpriteRenderer>() && parentRenderer.sprite != GetComponent<SpriteRenderer>().sprite)
            {
                GetComponent<SpriteRenderer>().sprite = parentRenderer.sprite;
            }
        }
    }
}
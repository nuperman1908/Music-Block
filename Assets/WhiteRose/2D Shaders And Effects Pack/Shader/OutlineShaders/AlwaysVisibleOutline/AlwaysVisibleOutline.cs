using System.Linq;
using UnityEngine;

namespace Outline
{
    public class AlwaysVisibleOutline : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer parentRenderer;
        [SerializeField] private int orderInLayer = 20;

        private void Start()
        {
            if (GetComponent<SpriteRenderer>())
            {
                string[] sortingLayers = SortingLayer.layers.Select(layer => layer.name).ToArray();
                if (sortingLayers.Length - 2 < 0)
                {
                    gameObject.GetComponent<SpriteRenderer>().sortingLayerName = sortingLayers[0];
                }
                else
                {
                    gameObject.GetComponent<SpriteRenderer>().sortingLayerName = sortingLayers[sortingLayers.Length - 2];
                }
                gameObject.GetComponent<SpriteRenderer>().sortingOrder = orderInLayer;
            }
        }
        private void Update()
        {
            if (parentRenderer != null && GetComponent<SpriteRenderer>() && parentRenderer.sprite != GetComponent<SpriteRenderer>().sprite)
            {
                gameObject.GetComponent<SpriteRenderer>().sprite = parentRenderer.sprite;
            }
        }
    }
}
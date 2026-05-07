using UnityEngine;

namespace CirclePrefab
{
    public class SpawnObject : MonoBehaviour
    {
        public GameObject circlePrefab;
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Instantiate(circlePrefab, mousePos, Quaternion.identity);
            }
        }
    }
}
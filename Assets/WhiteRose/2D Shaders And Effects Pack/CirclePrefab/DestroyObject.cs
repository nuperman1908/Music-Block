using System.Collections;
using UnityEngine;

namespace CirclePrefab
{
    public class DestroyObject : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine("DestroyPrefab");
        }
        private IEnumerator DestroyPrefab()
        {
            yield return new WaitForSeconds(2);
            Destroy(gameObject);
        }
    }
}
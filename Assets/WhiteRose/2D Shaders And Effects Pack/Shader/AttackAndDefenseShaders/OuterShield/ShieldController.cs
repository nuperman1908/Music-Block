using System.Collections;
using UnityEngine;

namespace AttackAndDefense
{
    public class ShieldController : MonoBehaviour
    {
        private Material material;
        private Color startColor;
        private Color endColor;

        private void Start()
        {
            material = GetComponent<SpriteRenderer>().material;//new Material(GetComponent<Renderer>().sharedMaterial);
            startColor = material.GetColor("_ColorOfWhite");
            endColor = material.GetColor("_ColorOfWhite");
            endColor.a -= 0.2f;
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "Projectile")
            {
                StartCoroutine("Damage");
                collision.gameObject.tag = "Untagged";
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Projectile")
            {
                StartCoroutine("Damage");
            }
        }
        private IEnumerator Damage()
        {
            material.SetColor("_ColorOfWhite", endColor);
            transform.localScale = new Vector2(transform.localScale.x - 0.02f, transform.localScale.y - 0.02f);
            yield return new WaitForSeconds(0.1f);
            transform.localScale = new Vector2(transform.localScale.x + 0.02f, transform.localScale.y + 0.02f);
            material.SetColor("_ColorOfWhite", startColor);
        }
    }
}
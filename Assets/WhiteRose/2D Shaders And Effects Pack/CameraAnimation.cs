using System.Collections;
using UnityEngine;

namespace CameraDemo
{
    public class CameraAnimation : MonoBehaviour
    {
        public float speed = 3;
        public GameObject logo;
        public GameObject logoBg;
        private void Update()
        {
            if ((int)transform.position.x == 330)
            {
                transform.position = new Vector3(331, transform.position.y, -10);
                StartCoroutine(ShowLogo());
            }
            if (transform.position.x < 330)
            {
                if (transform.position.x > 48)
                {
                    if (GetComponent<Camera>() && GetComponent<Camera>().orthographicSize < 7)
                    {
                        GetComponent<Camera>().orthographicSize += Time.deltaTime * speed;
                        speed = 2;
                    }
                }
                else
                {
                    speed = 1;
                }
                transform.position = new Vector3(transform.position.x + Time.deltaTime * speed, transform.position.y, -10);
            }
        }
        private IEnumerator ShowLogo()
        {
            logo.SetActive(true);
            logoBg.SetActive(true);

            Color logoColor = logo.GetComponent<SpriteRenderer>().color;
            Color logoBgColor = logoBg.GetComponent<SpriteRenderer>().color;

            for (float i = 0; i < 1; i += 0.01f)
            {
                logoBgColor.a = i;
                logoBg.GetComponent<SpriteRenderer>().color = logoBgColor;
                yield return new WaitForSeconds(0.01f);
            }
            for (float i = 0; i < 1; i += 0.01f)
            {
                logoColor.a = i;
                logo.GetComponent<SpriteRenderer>().color = logoColor;
                yield return new WaitForSeconds(0.02f);
            }
        }
    }
}
using UnityEngine;

namespace AttackAndDefense
{
    public class LaserControl : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private GameObject gun;
        [SerializeField] private bool shoot = false;
        [SerializeField] private Transform startPoint;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private GameObject fireEffect;
        [SerializeField] private GameObject hitEffect;

        private Vector2 direction;
        private Quaternion rot;
        private void Start()
        {
            hitEffect.SetActive(false);
            fireEffect.SetActive(false);
            lineRenderer.enabled = false;
        }
        void Update()
        {
            if (shoot)
            {
                LaserFire();
            }
            else
            {
                hitEffect.SetActive(false);
                fireEffect.SetActive(false);
                lineRenderer.enabled = false;
            }
        }
        private void LaserFire()
        {
            hitEffect.SetActive(true);
            fireEffect.SetActive(true);
            lineRenderer.enabled = true;

            direction = target.position - startPoint.position;

            RaycastHit2D hit = Physics2D.Raycast((Vector2)startPoint.position, direction.normalized, direction.magnitude);
            if (hit)
            {
                SetLine(startPoint.position, hit.point);
                hitEffect.transform.position = (Vector2)lineRenderer.GetPosition(1);
                hitEffect.SetActive(true);
            }
            else
            {
                hitEffect.SetActive(false);
                SetLine(startPoint.position, target.position);
            }
        }
        private void SetLine(Vector2 startPoint, Vector2 endPoint)
        {
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rot.eulerAngles = new Vector3(0, 0, angle);
            gun.transform.rotation = rot;
        }
    }
}

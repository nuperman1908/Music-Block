using UnityEngine;

namespace Effects
{
    public class ElectricityController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private bool shoot = false;
        [SerializeField] private Transform startPoint;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private GameObject startEffect;
        [SerializeField] private GameObject endEffect;
        [SerializeField] private ParticleSystem startParticle;
        [SerializeField] private ParticleSystem endParticle;
        [SerializeField] private Texture2D[] electricitySprites;
        [SerializeField] private float counterLimit = 60;

        private Vector2 direction;
        private float counter = 0;
        private int spriteOrder = 0;

        private void Awake()
        {
            endEffect.SetActive(false);
            startEffect.SetActive(false);
            lineRenderer.gameObject.SetActive(false);
        }
        void Update()
        {
            if (shoot)
            {
                ElectiricityAttack();
                SetElectiricitySprite();
            }
            else
            {
                endEffect.SetActive(false);
                startEffect.SetActive(false);
                lineRenderer.gameObject.SetActive(false);
            }
        }
        private void ElectiricityAttack()
        {
            startEffect.SetActive(true);
            lineRenderer.gameObject.SetActive(true);
            direction = target.position - startPoint.position;

            RaycastHit2D hit = Physics2D.Raycast((Vector2)startPoint.position, direction.normalized, direction.magnitude);
            if (hit)
            {
                SetLine(startPoint.position, hit.point);
                endEffect.transform.position = (Vector2)lineRenderer.GetPosition(1);
                endEffect.SetActive(true);
            }
            else
            {
                endEffect.SetActive(false);
                SetLine(startPoint.position, target.position);
            }
        }
        private void SetElectiricitySprite()
        {
            counter += Time.deltaTime;

            if (counter >= 1f / counterLimit)
            {
                counter = 0;
                lineRenderer.material.SetTexture("_MainTex", electricitySprites[spriteOrder]);
                startParticle.GetComponent<Renderer>().material.SetTexture("_MainTex", electricitySprites[spriteOrder]);
                endParticle.GetComponent<Renderer>().material.SetTexture("_MainTex", electricitySprites[spriteOrder]);
                spriteOrder++;
                if (spriteOrder > electricitySprites.Length - 1)
                {
                    spriteOrder = 0;
                }
            }
        }
        private void SetLine(Vector2 startPoint, Vector2 endPoint)
        {
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
        }
    }
}
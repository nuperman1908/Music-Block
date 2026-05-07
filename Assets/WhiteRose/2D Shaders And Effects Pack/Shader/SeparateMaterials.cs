using UnityEngine;
using UnityEngine.UI;

namespace Shaders
{
    [ExecuteInEditMode]
    public class SeparateMaterials : MonoBehaviour
    {
        private Material materialSR;
        private Material firstMaterialSR;
        private Material materialLR;
        private Material firstMaterialLR;
        private Material materialTR;
        private Material firstMaterialTR;
        private Material materialMR;
        private Material firstMaterialMR;
        private Material materialPS;
        private Material firstMaterialPS;
        private Material materialI;
        private Material firstMaterialI;
        private void Update()
        {
            if (!Application.isPlaying)
            {
                SetFirstMaterial();
                if (GetComponent<SpriteRenderer>())
                {
                    if (firstMaterialSR != materialSR)
                    {
                        materialSR = new Material(GetComponent<SpriteRenderer>().sharedMaterial);
                        firstMaterialSR = materialSR;
                        GetComponent<SpriteRenderer>().material = materialSR;
                    }
                }
                if (GetComponent<LineRenderer>())
                {
                    if (firstMaterialLR != materialLR)
                    {
                        materialLR = new Material(GetComponent<LineRenderer>().sharedMaterial);
                        firstMaterialLR = materialLR;
                        GetComponent<LineRenderer>().material = materialLR;
                    }
                }
                if (GetComponent<TrailRenderer>())
                {
                    if (firstMaterialTR != materialTR)
                    {
                        materialTR = new Material(GetComponent<TrailRenderer>().sharedMaterial);
                        firstMaterialTR = materialTR;
                        GetComponent<TrailRenderer>().material = materialTR;
                    }
                }
                if (GetComponent<MeshRenderer>())
                {
                    if (firstMaterialMR != materialMR)
                    {
                        materialMR = new Material(GetComponent<MeshRenderer>().sharedMaterial);
                        firstMaterialMR = materialMR;
                        GetComponent<MeshRenderer>().material = materialMR;
                    }
                }
                if (GetComponent<ParticleSystemRenderer>())
                {
                    if (firstMaterialPS != materialPS)
                    {
                        materialPS = new Material(GetComponent<ParticleSystemRenderer>().sharedMaterial);
                        firstMaterialPS = materialPS;
                        GetComponent<ParticleSystemRenderer>().material = materialPS;
                    }
                }
                if (GetComponent<Image>())
                {
                    if (firstMaterialI != materialI)
                    {
                        materialI = new Material(GetComponent<Image>().material);
                        firstMaterialI = materialI;
                        GetComponent<Image>().material = materialI;
                    }
                }
            }
        }
        private void SetFirstMaterial()
        {
            if (GetComponent<SpriteRenderer>())
            {
                firstMaterialSR = GetComponent<SpriteRenderer>().sharedMaterial;
            }
            if (GetComponent<LineRenderer>())
            {
                firstMaterialLR = GetComponent<LineRenderer>().sharedMaterial;
            }
            if (GetComponent<TrailRenderer>())
            {
                firstMaterialTR = GetComponent<TrailRenderer>().sharedMaterial;
            }
            if (GetComponent<MeshRenderer>())
            {
                firstMaterialMR = GetComponent<MeshRenderer>().sharedMaterial;
            }
            if (GetComponent<ParticleSystemRenderer>())
            {
                firstMaterialPS = GetComponent<ParticleSystemRenderer>().sharedMaterial;
            }
            if (GetComponent<Image>())
            {
                firstMaterialI = GetComponent<Image>().material;
            }
        }
    }
}
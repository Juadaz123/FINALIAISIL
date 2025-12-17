using System.Collections;
using UnityEngine;

namespace Player
{
    public class SharkColor : MonoBehaviour
    {
        [Header("Configuración Visual")]
        [SerializeField] private Renderer sharkRenderer; 
        [SerializeField] private string materialNameTarget = "Top";
        
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color biteColor = Color.red;

        private Material _targetMaterial;
        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            if (sharkRenderer != null)
            {
                // Buscamos el material correcto dentro de la lista
                Material[] allMaterials = sharkRenderer.materials;
                foreach (Material mat in allMaterials)
                {
                    if (mat.name.Contains(materialNameTarget))
                    {
                        _targetMaterial = mat;
                        break; 
                    }
                }

                if (_targetMaterial == null && allMaterials.Length > 0)
                {
                    _targetMaterial = allMaterials[0];
                }
                
                SetNormalColor();
            }
        }

        public void SetDamageColor()
        {
            if (_targetMaterial != null)
                _targetMaterial.SetColor(BaseColorID, biteColor);
        }

        public void SetNormalColor()
        {
            if (_targetMaterial != null)
                _targetMaterial.SetColor(BaseColorID, normalColor);
        }
        
    }
}
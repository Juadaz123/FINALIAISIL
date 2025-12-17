using UnityEngine;

namespace LifeSystem
{
    public class DamageCollider : MonoBehaviour
    {
        [Header("Damage Collider Parameters")]
        [SerializeField] private int damageAmount;
        [SerializeField] private string tagTarget = "Boss";
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(tagTarget))
            {
               LifeSystem targetLife = other.GetComponent<LifeSystem>();

                if (targetLife != null)
                {
                    targetLife.TakeDamage(damageAmount);
                    Debug.Log($"Golpe exitoso a {other.name}. Daño aplicado: {damageAmount}");
                }
            
            }
        }
    }
}


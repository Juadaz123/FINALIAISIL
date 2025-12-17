using System;
using UnityEngine;

namespace LifeSystem
{
    public class HealthCollider : MonoBehaviour
    {
        [Header("Heal Parameters")]
        [SerializeField] private int healAmount = 10; 
        [SerializeField] private string tagTarget = "Player";
        
        [Header("Configuration")]
        [SerializeField] private bool destroyOnPickup = true;
        
        private LifeSystem _targetLife;

        private void Awake()
        {
            _targetLife = GameObject.FindGameObjectWithTag("Player").GetComponent<LifeSystem>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(tagTarget))
            {
                Debug.Log("VAR");
                

                if (_targetLife != null)
                {
                    _targetLife.TakeHeal(healAmount);
                    
                    Debug.Log($"Curación exitosa a {other.name}. Vida recuperada: {healAmount}");

                    if (destroyOnPickup)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
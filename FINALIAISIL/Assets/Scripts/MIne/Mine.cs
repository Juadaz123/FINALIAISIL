using System.Collections;
using UnityEngine;
using LifeSystem;

public class Mine : MonoBehaviour
{
    [Header("Mine Parameters")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string bossTag = "Boss";
    [SerializeField] private int damage = 5; 
    
    private Material _material;

    private void Awake()
    {
        Renderer rend = GetComponent<Renderer>();
        if(rend != null)
            _material = rend.material;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) || other.CompareTag(bossTag))
        {
            StartCoroutine(Explotion(other.gameObject));
        }
    }

    private IEnumerator Explotion(GameObject target)
    {
        if (_material != null) _material.color = Color.red;
        yield return new WaitForSeconds(1.5f);

        if(target != null)
        {
            LifeSystem.LifeSystem targetLife = target.GetComponent<LifeSystem.LifeSystem>();

            if(targetLife != null)
            {
                targetLife.TakeDamage(damage); 
                Debug.Log($"Mina explotó contra {target.name} causando {damage} de daño.");
            }
        }

        
        gameObject.SetActive(false);
    }
}
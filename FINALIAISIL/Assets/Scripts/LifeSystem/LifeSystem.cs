using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace LifeSystem
{
    public class LifeSystem : MonoBehaviour
    {
        [Header("Life Parameters ")] 
        [SerializeField] private int maxLife;

        [SerializeField] private float colorInterval = 0.5f;

        [SerializeField] private Image healthBar;

        private SharkColor _color;

        private int _currentLife;

        private void Awake()
        {
            ResetLife();
             _color = GetComponent<SharkColor>();
        }

        //Life System Voids
        public void ResetLife()
        {
            _currentLife = maxLife;
        }

        public void TakeDamage(int damage)
        {
            _currentLife -= damage;
            currenthealth();
            
            StartCoroutine(ChangeColor());
        }

        public void TakeHeal(int heal)
        {

            _currentLife += heal;
            if (_currentLife > maxLife) _currentLife = maxLife;
            currenthealth();

        }

        public int currenthealth()
        {
            return _currentLife;
        }

        //healtbarLogic
        private void Update()
        {
            if (_currentLife <= 0)
            {
                StopAllCoroutines();
                gameObject.SetActive(false);
            }
            
            if (healthBar != null)
            {
                healthBar.fillAmount = (float)_currentLife / maxLife;
            }
        }

        private IEnumerator ChangeColor()
        {
            if(_color == null) yield break;
            
            _color.SetDamageColor();
            yield return new WaitForSeconds(colorInterval);
            _color.SetNormalColor();
            
        }
    
}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ObjectPool;

namespace Player
{
    [RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Player Parameters")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private GameObject biteCollider;
        [SerializeField] private GameObject biteSpawn;

        [Header("Dependencies")]
        [SerializeField] private SharkColor sharkColor;
            [SerializeField] private SymbolPool symbolPool;
        
        private int _swimMultiplierID; 
        private Animator _animator;
        private Rigidbody _rb;
        
        private bool _isDiving;
        private Vector3 _moveDirection;
        private Coroutine _activeBiteCoroutine;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            
            _swimMultiplierID = Animator.StringToHash("speedMultiplier");
            
            if (_animator != null)
                _animator.SetFloat(_swimMultiplierID, 1f);

            if(biteCollider != null) 
                biteCollider.SetActive(false);
            
            if (sharkColor == null)
                sharkColor = GetComponent<SharkColor>();
            
            
        }

        private void FixedUpdate()
        {
            MovingPlayer();

            if (_isDiving)
            {
                _animator.speed = 1.8f;
                _animator.SetFloat(_swimMultiplierID, 2.5f);
                _rb.AddForce(transform.forward * speed, ForceMode.Acceleration);
            }
            else 
            {
                _animator.speed = 1f;
                _animator.SetFloat(_swimMultiplierID, 1f); 
            }
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            Vector2 input = ctx.ReadValue<Vector2>();
            _moveDirection = new Vector3(input.x, input.y, 0);
        }

        public void OnBite(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                if (_activeBiteCoroutine != null) StopCoroutine(_activeBiteCoroutine);
                _activeBiteCoroutine = StartCoroutine(Bites());
            }
            else if (ctx.canceled)
            {
                if (_activeBiteCoroutine != null) 
                {
                    StopCoroutine(_activeBiteCoroutine);
                    if(biteCollider != null) biteCollider.SetActive(false);
                    
                    if (sharkColor != null) sharkColor.SetNormalColor();
                    
                    _activeBiteCoroutine = null;
                }
            }
        }

        public void OnDive(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) _isDiving = true;
            if (ctx.canceled) _isDiving = false;
        }

        private void MovingPlayer()
        {
            if (_moveDirection.sqrMagnitude > 0.01f)
            {
                float pitch = _moveDirection.y * rotationSpeed * Time.fixedDeltaTime;
                float yaw = _moveDirection.x * rotationSpeed * Time.fixedDeltaTime;

                Quaternion deltaRotation = Quaternion.Euler(pitch, yaw, 0);
                _rb.MoveRotation(_rb.rotation * deltaRotation);
            }
        }

        private IEnumerator Bites()
        {
            // 1. Verificación de seguridad para evitar el error NullReference
            if (biteSpawn == null)
            {
                Debug.LogError("ERROR: No has asignado el 'Bite Spawn' en el inspector del PlayerController.");
                yield break;
            }

            if (symbolPool == null)
            {
                Debug.LogError("ERROR: No se encuentra 'SymbolPool' en la escena.");
                yield break;
            }
            
            if(biteCollider == null) yield break;
            
            biteCollider.SetActive(true);
            
            symbolPool.SpawnPlayerSymbol(biteSpawn.transform);
            yield return new WaitForSeconds(1.2f);
            
            biteCollider.SetActive(false); 
            
        }
    }
}
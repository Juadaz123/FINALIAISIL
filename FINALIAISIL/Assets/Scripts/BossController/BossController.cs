using System.Collections;
using UnityEngine;
using IA.SteeringBehaviours;
using ObjectPool;
using Random = UnityEngine.Random;
using ObjectPool;

namespace BossController
{
    [RequireComponent(typeof(Rigidbody))]
    public class BossController : MonoBehaviour, IAgent
    {
        public enum BossStates
        {
            WaitAndSeek,
            WanderMove,
            ArriveSeek,
            Evade,
            Flee,
            IdleBite,
        }

        [Header("Boss Settings")]
        [SerializeField] private BossStates currentState;
        [SerializeField] private GameObject biteCollider;
        [SerializeField] private float maxForce = 5f;
        [SerializeField] private float speed = 10f;
        [SerializeField] private float arriveRadius = 3f;
        
        [Header("Floor Avoidance")]
        [SerializeField] private LayerMask floorLayer;
        [SerializeField] private float floorDetectionDist = 5f;
        [SerializeField] private float floorAvoidanceWeight = 10f; 

        [Header("Wander Settings")]
        [SerializeField] private float wanderCircleDist = 2f;
        [SerializeField] private float wanderRadius = 3f;
        [SerializeField] private float wanderAngleChange = 15f;
        private float _currentWanderAngle;
        
        [Header("Detection Ranges")]
        [SerializeField] private float veryCloseRange = 5f;
        [SerializeField] private float closeRange = 15f;
        [SerializeField] private float farRange = 30f;
        [SerializeField] private float distanceToPlayer;
        
        [Header("Dependences")]
        [SerializeField] private SymbolPool symbolPool;

        // Referencias
        private Transform _playerTransform;
        private Rigidbody _rb;
        private SteeringBehaviour.SimpleAgent _playerAgent; 

        // Variables Internas de Control (Corrutinas)
        private Vector3 _currentSteeringForce;
        private Coroutine _currentStateCoroutine;
        
        private bool _isBusy = false; 

        // --- IMPLEMENTACIÓN DE LA INTERFAZ IAGENT ---
        public float MaxSpeed 
        { 
            get => speed; 
            set => speed = value; 
        }
        public float MaxForce 
        { 
            get => maxForce; 
            set => maxForce = value; 
        }
        public Vector3 Position 
        { 
            get => transform.position; 
            set => transform.position = value; 
        }
        public Vector3 Velocity 
        { 
            get => _rb.linearVelocity; 
            set => _rb.linearVelocity = value; 
        }
        // ---------------------------------------------

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _playerTransform = playerObj.transform;
                _playerAgent = new SteeringBehaviour.SimpleAgent(_playerTransform.position);
            }
        }

        private void Start()
        {
            biteCollider.SetActive(false);
            StartCoroutine(ThinkRoutine());
        }

        private void FixedUpdate() 
        {
            if (_playerTransform == null) return;

            _playerAgent.Position = _playerTransform.position;

            Vector3 totalSteering = Vector3.zero;

            // 1. Evitar suelo (Prioridad Alta)
            totalSteering += AvoidFloor();

            // 2. Sumamos la fuerza calculada por la Corrutina actual
            totalSteering += _currentSteeringForce;

            ApplySteering(totalSteering);
            
            LookWhereYouGo();
        }

        // --- LÓGICA DE EVITAR SUELO ---
        private Vector3 AvoidFloor()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, floorDetectionDist, floorLayer))
            {
                SteeringBehaviour.SimpleAgent floorPoint = new SteeringBehaviour.SimpleAgent(hit.point);
                return SteeringBehaviour.Flee(this, floorPoint) * floorAvoidanceWeight;
            }
            return Vector3.zero;
        }

        // --- MÁQUINA DE ESTADOS (Corrutinas de Acción) ---
        // Este método gestiona el cambio de corrutinas limpiamente
        private void ChangeState(BossStates newState)
        {
            if (currentState == newState && _currentStateCoroutine != null) return;

            currentState = newState;

            if (_currentStateCoroutine != null) StopCoroutine(_currentStateCoroutine);

            switch (currentState)
            {
                case BossStates.ArriveSeek:
                    _currentStateCoroutine = StartCoroutine(StateArriveSeek());
                    break;
                case BossStates.Flee:
                    _currentStateCoroutine = StartCoroutine(StateFlee());
                    break;
                case BossStates.WanderMove:
                    _currentStateCoroutine = StartCoroutine(StateWanderMove());
                    break;
                case BossStates.Evade:
                    _currentStateCoroutine = StartCoroutine(StateEvade());
                    break;
                case BossStates.WaitAndSeek:
                    _currentStateCoroutine = StartCoroutine(StateWaitAndSeek());
                    break;
                case BossStates.IdleBite:
                    _currentSteeringForce = Vector3.zero;
                    Velocity = Vector3.zero;
                    break;
            }
        }

        // --- CORRUTINAS DE COMPORTAMIENTO ---

        private IEnumerator StateArriveSeek()
        {
            _isBusy = true; 

            float timer = 0f;

            while (timer < 4f && currentState == BossStates.ArriveSeek)
            {
                _currentSteeringForce = SteeringBehaviour.Arrive(this, _playerAgent, arriveRadius);
                yield return new WaitForFixedUpdate();
                timer += Time.fixedDeltaTime;
            }

            if (currentState == BossStates.ArriveSeek)
            {
                PerformBiteAttack();
            }

            timer = 0f; 

            while (timer < 2f && currentState == BossStates.ArriveSeek)
            {
                _currentSteeringForce = SteeringBehaviour.Seek(this, _playerAgent);
                yield return new WaitForFixedUpdate();
                timer += Time.fixedDeltaTime;
            }

            if (currentState == BossStates.ArriveSeek)
            {
                PerformBiteAttack();
            }

            _isBusy = false;
            ChangeState(BossStates.WanderMove);
        }

        private IEnumerator StateFlee()
        {
            _isBusy = true;

            float timer = 0f;
            float evadeDuration = 2.5f;

            while (currentState == BossStates.Flee && timer < evadeDuration)
            {
                _currentSteeringForce = SteeringBehaviour.Flee(this, _playerAgent);
        
                yield return new WaitForFixedUpdate();
        
                timer += Time.fixedDeltaTime;
            }
            
            _isBusy = false; 
            ChangeState(BossStates.WanderMove);
        }

        private IEnumerator StateWanderMove()
        {
            _isBusy = true; 

            int totalCycles = 4; 
            float timePerCycle = 2f;

            for (int i = 0; i < totalCycles; i++)
            {
                if(currentState != BossStates.WanderMove) break;

                float timer = 0f;
                while (timer < timePerCycle && currentState == BossStates.WanderMove)
                {
                    _currentSteeringForce = SteeringBehaviour.Wander(this, wanderCircleDist, wanderRadius, ref _currentWanderAngle, wanderAngleChange);
            
                    yield return new WaitForFixedUpdate();
                    timer += Time.fixedDeltaTime;
                }

                if (currentState == BossStates.WanderMove)
                {
                    PerformBiteAttack(); // ¡Ñam!
                }
            }
            
            yield return new WaitForFixedUpdate();

            _isBusy = false; 
        }

        private IEnumerator StateEvade()
        {
            _isBusy = true;

            float timer = 0f;
            float evadeDuration = 2.5f;

            while (currentState == BossStates.Evade && timer < evadeDuration)
            {
                _currentSteeringForce = SteeringBehaviour.Evade(this, _playerAgent);
        
                yield return new WaitForFixedUpdate();
        
                timer += Time.fixedDeltaTime;
            }
            
            _isBusy = false;
            ChangeState(BossStates.WanderMove);
        }

        private IEnumerator StateWaitAndSeek()
        {
            _isBusy = true; 

            float _speed = speed;
            for (int i = 0; i < 2; i++)
            {
                if (currentState != BossStates.WaitAndSeek) break;

                float timer = 0f;
                while (timer < 1.5f)
                {
                    speed *= 1.5f;
                    _currentSteeringForce = SteeringBehaviour.Seek(this, _playerAgent);
            
                    yield return new WaitForFixedUpdate();
            
                    timer += Time.fixedDeltaTime;
                }
                _currentSteeringForce = Vector3.zero;
                PerformBiteAttack();
                yield return new WaitForSeconds(1.5f);
                speed = _speed;
            }
            
            _isBusy = false; 

            
            while (currentState == BossStates.WaitAndSeek)
            {
                _currentSteeringForce = SteeringBehaviour.Seek(this, _playerAgent);
                yield return new WaitForFixedUpdate();
            }
        }
        
        private void PerformBiteAttack()
        {
            
            if(biteCollider != null) biteCollider.SetActive(true);
            Debug.Log("chomp Boss");
            symbolPool.SpawnPlayerSymbol(biteCollider.transform);
            Invoke(nameof(DisableBite), 1.5f);
        }

        private void DisableBite()
        {
            if(biteCollider != null) biteCollider.SetActive(false);
        }

        private IEnumerator ThinkRoutine()
        {
            yield return new WaitForSeconds(0.1f);

            while (true)
            {
                if (_playerTransform != null && !_isBusy)
                {
                    distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
                    DecideStateBasedOnDistance();
                }
                
                yield return new WaitForSeconds(0.5f);
            }
        }

        public void DecideStateBasedOnDistance()
        {
            BossStates nextState = currentState;

            if (distanceToPlayer <= veryCloseRange)
            {
                nextState = PickRandomState(BossStates.Evade, BossStates.Flee, BossStates.WanderMove, BossStates.IdleBite);
            }
            else if (distanceToPlayer <= closeRange)
            {
                nextState = PickRandomState(BossStates.WanderMove, BossStates.WaitAndSeek);
            }
            else if (distanceToPlayer <= farRange)
            {
                nextState = PickRandomState(BossStates.ArriveSeek, BossStates.WanderMove);
            }
            else
            {
                nextState = BossStates.WanderMove;
            }

            Debug.Log(nextState + " state");
            ChangeState(nextState);
        }

        private BossStates PickRandomState(params BossStates[] possibleStates)
        {
            if (possibleStates.Length == 0) return BossStates.WanderMove;
            int randomIndex = Random.Range(0, possibleStates.Length);
            return possibleStates[randomIndex];
        }

        // --- UTILIDADES FÍSICAS ---
        private void ApplySteering(Vector3 steering)
        {
            // CORRECCIÓN IMPORTANTE: Evitar asignar valores basura (NaN) al Rigidbody
            if (float.IsNaN(steering.x) || float.IsNaN(steering.y) || float.IsNaN(steering.z))
            {
                return; 
            }

            steering = Vector3.ClampMagnitude(steering, MaxForce);
            Vector3 newVelocity = Velocity + steering * Time.fixedDeltaTime;
            newVelocity = Vector3.ClampMagnitude(newVelocity, MaxSpeed);
            
            if (!float.IsNaN(newVelocity.x) && !float.IsNaN(newVelocity.y) && !float.IsNaN(newVelocity.z))
            {
                Velocity = newVelocity;
            }
        }

        private void LookWhereYouGo()
        {
            if (Velocity.sqrMagnitude > 0.1f)
            {
                Quaternion lookRot = Quaternion.LookRotation(Velocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.fixedDeltaTime * 5f);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, Vector3.down * floorDetectionDist);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, veryCloseRange);

            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, closeRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, farRange);
        }
    }
}
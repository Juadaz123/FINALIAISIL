using System;
using IA.SteeringBehaviours;
using UnityEngine;

namespace IA.StearingBehaviours
{
    public class Boid : MonoBehaviour, IAgent
{

    public float MaxSpeed { get; set; }
    public float MaxForce { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    
    private FlockManager _manager;
    private float _cohesionWeight;
    private float _separationWeight;
    private float _alignmentWeight;
    
    private bool _enableSphereContainment;
    private float _sphereRadius;
    private float _containmentWeight;
    private Vector3 _sphereCenter;
    private SteeringBehaviour.SimpleAgent _playerAgent; 
    private Transform _playerTransform;


    private void Start()
    {
        SetValuesBoids();
    }

    private void SetValuesBoids()
    {
        _manager = FindAnyObjectByType<FlockManager>();
        Position = transform.position;
        MaxSpeed = _manager.maxSpeed;
        MaxForce = _manager.maxForce;
        
        _cohesionWeight = _manager.cohesionWeight;
        _separationWeight = _manager.separationWeight;
        _alignmentWeight = _manager.alignmentWeight;
        
        _enableSphereContainment = _manager.enableSphereContainment;
        _sphereRadius = _manager.sphereRadius;
        _containmentWeight = _manager.containmentWeight;
        _sphereCenter = _manager.transform.position;
        
        
    }

    private void Update()
    {
        BehaviourFlock();
    }

    public void GetMoreCohesion(bool nearPLayer)
    {
        if (nearPLayer)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _playerTransform = playerObj.transform;
                _playerAgent = new SteeringBehaviour.SimpleAgent(_playerTransform.position);
            }
            _cohesionWeight += 10f;
            MaxSpeed = 1f;
            Position = transform.position;
            Velocity += SteeringBehaviour.Seek(this, _playerAgent).normalized;
            transform.position += Velocity * Time.deltaTime;
        }
        else if (!nearPLayer)
        {
            BehaviourFlock();

        }
    }

    private void BehaviourFlock()
    {Vector3 cohesion = SteeringBehaviour.Cohesion(this, _manager.Boids, _manager.cohesionRadius) * _cohesionWeight;
        Vector3 separation = SteeringBehaviour.Separation(this, _manager.Boids, _manager.separationRadius) * _separationWeight;
        Vector3 alignment = SteeringBehaviour.Alignment(this, _manager.Boids, _manager.alignmentRadius) * _alignmentWeight; 
        Vector3 containment = Vector3.zero;
        if (_enableSphereContainment)
        {
            containment = SteeringBehaviour.InsideSphere(this, _sphereCenter, _sphereRadius) * _containmentWeight;
        }

        Vector3 totalSteeringForce = cohesion + separation + alignment + containment;
        
        totalSteeringForce = Vector3.ClampMagnitude(totalSteeringForce, MaxForce);
        
        Velocity += totalSteeringForce * Time.deltaTime;
        
        Velocity = Vector3.ClampMagnitude(Velocity, MaxSpeed);
        
        Position += Velocity * Time.deltaTime;
        
        if (Velocity.sqrMagnitude > Mathf.Epsilon) 
        {
            transform.up = Velocity.normalized; 
        }
        transform.position = Position;

        
    }
}
}

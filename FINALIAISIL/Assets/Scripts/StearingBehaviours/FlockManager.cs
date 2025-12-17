 using IA.StearingBehaviours;

using IA.SteeringBehaviours;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlockManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject boidPrefab;
    [SerializeField, Range(1,100)] private int flockSize = 10;
    [SerializeField] private Vector3 spawnArea = new Vector3(10, 10, 10);
    
    [Header("Behavior Wieight")]
    public float cohesionWeight;
    public float separationWeight;
    public float alignmentWeight;
    [Header("Boid Settings")] 
    public float maxSpeed = 5f;
    public float maxForce = 10;
    public float cohesionRadius = 1f, separationRadius = 1f, alignmentRadius = 1f;
    
    [Header("Sphere Containment")]
    public bool enableSphereContainment = true;
    public float sphereRadius = 15f;
    public float containmentWeight = 2f;
    
    [Header("Space Wrapping")]
    public bool enableWrapping = true;

    [SerializeField] private Vector3 wrapBounds = new Vector3(15, 15, 15);
    
    public  IAgent[] Boids { get; private set; }

    private void Start()
    {
        Boids = new IAgent[flockSize];
        for (int i = 0; i < flockSize; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(
                Random.Range(-spawnArea.x, spawnArea.x),
                Random.Range(-spawnArea.y, spawnArea.y),
                Random.Range(-spawnArea.z, spawnArea.z)
                );
            GameObject boid = Instantiate(boidPrefab, spawnPosition, Quaternion.identity);
            Boids[i] = boid.GetComponent<Boid>();
        }
    }

    public Vector3 GetWrapedPos(Vector3 position)
    {
        if(!enableWrapping) return position;
        Vector3 wrapped = position;
        if(position.x > wrapBounds.x) wrapped.x = -wrapBounds.x;
        else if(position.x < -wrapBounds.x) wrapped.x = wrapBounds.x; 
        
        if(position.y > wrapBounds.y) wrapped.y = -wrapBounds.y;
        else if(position.y < -wrapBounds.y) wrapped.y = wrapBounds.y; 
        
        if(position.z > wrapBounds.z) wrapped.z = -wrapBounds.z;
        else if(position.z < -wrapBounds.z) wrapped.z = wrapBounds.z; 
        
        return wrapped;
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, spawnArea);
        
        if (enableSphereContainment)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, sphereRadius);
        }
    }
}

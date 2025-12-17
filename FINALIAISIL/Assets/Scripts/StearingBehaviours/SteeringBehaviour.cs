using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace IA.SteeringBehaviours
{
    public class SteeringBehaviour : MonoBehaviour
    {
        public static Vector3 Seek(IAgent agent, IAgent target) // Agente busca al target
        {
            Vector3 desired = (target.Position - agent.Position).normalized * agent.MaxSpeed;
            Vector3 steering = Vector3.ClampMagnitude(desired - agent.Velocity, agent.MaxForce);
            return steering;
        }
        
        public static Vector3 Flee(IAgent agent, IAgent target) // Agente se escapa del target
        {
            Vector3 desired = (agent.Position - target.Position).normalized * agent.MaxSpeed;
            Vector3 steering = Vector3.ClampMagnitude(desired - agent.Velocity, agent.MaxForce);
            return steering;
        }

        public static Vector3 Arrive(IAgent agent, IAgent target, float arriveRadius) // Agente busca al target y reduce su velocidad entrando en el área del radio
        {
            Vector3 desired = (target.Position - agent.Position).normalized * agent.MaxSpeed;
            if (Vector3.Distance(agent.Position, target.Position) < arriveRadius)
            {
                desired *= Vector3.Distance(agent.Position, target.Position) / arriveRadius;
            }
            Vector3 steering = Vector3.ClampMagnitude(desired - agent.Velocity, agent.MaxForce);
            
            return steering;
        }

        public static Vector3 Wander(IAgent agent, float circleDistance, float circleRadius, ref float wanderAngle, float angleChangeDegrees)
        {
            // posicionar el centro del círculo frente al agente
            Vector3 circleCenter = agent.Velocity.normalized * circleDistance;
            // crear nuestro vector displacement en un plano XZ
            Vector3 displacement = Vector3.forward * circleRadius;
            // rotar nuestro displacement usando el wander angle
            displacement = SetAngle(displacement, wanderAngle);
            // agregar fluctuación(Jitter) al ángulo (esto es en radianes)
            float angleChangeRad = angleChangeDegrees * Mathf.Deg2Rad;
            wanderAngle += Random.Range(-angleChangeRad, angleChangeRad);
            // Velocidad deseada...
            Vector3 desired = circleCenter + displacement;
            // steer
            Vector3 steering = Vector3.ClampMagnitude(desired - agent.Velocity, agent.MaxForce);
            return steering;

        }
        // esta función nos ayudará a rotar el displacement de manera "2D" en un plano XZ
        private static Vector3 SetAngle(Vector3 displacement, float angleRad)
        {
            float lenght = displacement.magnitude;
            return new Vector3(Mathf.Cos(angleRad) * lenght, 0 , Mathf.Sin(angleRad) * lenght);
        }

        public static Vector3 Pursue(IAgent agent, IAgent target)
        {
            // Que tanto vamos a predecir 
            Vector3 toTarget = target.Position - agent.Position;
            float distance = toTarget.magnitude;
            
            // Estimar que tanto nos tomará llegar
            float speed = agent.MaxSpeed;
            float predictionTime = distance / speed;
            
            // predecir la posición futura del target USANDO la velocidad del target
            Vector3 futurePosition = target.Position + target.Velocity * predictionTime;
            
            // hacemos el seek
            /*Vector3 desired = (futurePosition - agent.Position).normalized * agent.MaxSpeed;
            Vector3 steering = Vector3.ClampMagnitude(desired - agent.Velocity, agent.MaxForce);
            return steering;*/
            return Seek(agent, new SimpleAgent(futurePosition));
        }
        
        public static Vector3 Evade(IAgent agent, IAgent target)
        {
            // Que tanto vamos a predecir 
            Vector3 toTarget = target.Position - agent.Position;
            float distance = toTarget.magnitude;
            
            // Estimar que tanto nos tomará llegar
            float speed = agent.MaxSpeed;
            float predictionTime = distance / speed;
            
            // predecir la posición futura del target USANDO la velocidad del target
            Vector3 futurePosition = target.Position + target.Velocity * predictionTime;
            
            // hacemos el seek
            /*Vector3 desired = (futurePosition - agent.Position).normalized * agent.MaxSpeed;
            Vector3 steering = Vector3.ClampMagnitude(desired - agent.Velocity, agent.MaxForce);
            return steering;*/
            return Flee(agent, new SimpleAgent(futurePosition));
        }

        public static Vector3 Cohesion(IAgent boid, IAgent[] neighbours, float cohesionRadius)
        {
            Vector3 center = Vector3.zero;
            int count = 0;
            int neighbourCount = neighbours.Length;

            for (int i = 0; i < neighbourCount; i++)
            {
                IAgent neighbour = neighbours[i];
                float distance = Vector3.Distance(neighbour.Position, boid.Position);
                if (distance < cohesionRadius && distance > Mathf.Epsilon)
                {
                    center += neighbour.Position;
                    count++;
                }
            }
            if(count == 0) return Vector3.zero;

            center = center/ count;
            return Seek(boid, new SimpleAgent(center));
        }

        public static Vector3 Separation(IAgent boid, IAgent[] neighbours, float separationRadius)
        {
            // Acumulador del steer deseado
            Vector3 steer = Vector3.zero;
            
            // Cuantos vecinos tenemos cerca (en el radio de detección)
            int count = 0;
            
            // Optimizamos el Vector3.Distance utilizando precomputando la potencia
            float sqrRadius = separationRadius * separationRadius;
            
            // Lista cache de nuestros vecinos 
            int neighbourCount = neighbours.Length;

            for (int i = 0; i < neighbourCount; i++)
            {
                // Recorremos la lista y lo agregamos
                IAgent neightbour = neighbours[i];
                // Vector DESDE el vecino HACIA el boid 
                Vector3 offset = boid.Position - neightbour.Position;
                
                // Distancia al cuadrado del offset
                float sqrDistance = offset.sqrMagnitude;
                
                // Revisamos si estamos en el radio de detección
                if (sqrDistance < sqrRadius && sqrDistance > Mathf.Epsilon)
                {
                    // necesitamos la distancia real para normalizar, esto solo se computa una sola vez
                    float distance = Mathf.Sqrt(sqrDistance);
                    // Conseguimos la dirección normalizada apuntando en contra del vecino
                    Vector3 direction = offset / distance;
                    // peso dependiendo de qué tan cerca este a su vecino, si está demasiado cerca empujarlo mucho más
                    steer += direction/ Mathf.Max(distance, Mathf.Epsilon);
                    count++;
                }
            }
            
            if(count > 0) steer = steer.normalized * boid.MaxSpeed;
            return steer;
        }

        public static Vector3 Alignment(IAgent boid, IAgent[] neighbours, float alignmentRadius)
        {
            // Acumulador para guardar el promedio de nuestras velocidades de nuestros vecinos
            Vector3 avarageVelocity = Vector3.zero;
            
            int count = 0;
            float sqrRadius = alignmentRadius * alignmentRadius;
            int neighbourCount = neighbours.Length;

            for (int i = 0; i < neighbourCount; i++)
            {
                IAgent neighbour = neighbours[i];
                // Magnitud al cuadrado DESDE el boid hacia el vecino
                float sqrDistance = (boid.Position - neighbour.Position).sqrMagnitude;

                if (sqrDistance < sqrRadius && sqrDistance > Mathf.Epsilon)
                {
                    avarageVelocity += neighbour.Velocity;
                    count++;
                }    
            }
            if (count == 0) return Vector3.zero;
            
            // Dividir entre la cantidad de vecinos para conseguir el promedio 
            avarageVelocity /= count;
            
            return avarageVelocity.normalized * boid.MaxSpeed -  boid.Velocity;
        }

        public static Vector3 FollowPath(IAgent agent,
            Transform[] pathNodes,
            ref int currentNodeIndex,
            ref int pathDirection,
            float nodeRadius,
            float pathFollowWeight)
        {
            // Validación inicial, si no hay path retornamos un vector zero
            if(pathNodes == null || pathNodes.Length == 0) return Vector3.zero;
            
            // Nos aseguramos que currentNodeIndex no sobrepase los limites del Array
            currentNodeIndex = Mathf.Clamp(currentNodeIndex, 0, pathNodes.Length - 1);
            
            // Validación del target
            Transform targetNode = pathNodes[currentNodeIndex];
            if(!targetNode) return Vector3.zero;
            
            // Revisamos la distancia y avanzamos el nodo target
            Vector3 targetPosition = targetNode.position;
            if (Vector3.Distance(agent.Position, targetPosition) <= nodeRadius)
            {
                currentNodeIndex += pathDirection;
                // Regresamos si estamos en el último nodo
                if (currentNodeIndex >= pathNodes.Length || currentNodeIndex < 0)
                {
                    pathDirection = -1; // Invertimos la dirección al llegar al final
                    currentNodeIndex += pathDirection;
                }
            }
            
            // Hacemos el Seek al siguiente Nodo
            return Seek(agent, new SimpleAgent(targetPosition)) * pathFollowWeight;
        }

        public static Vector3 InsideSphere(IAgent agent, Vector3 sphereCenter, float weight)
        {
            SimpleAgent centerAgent = new  SimpleAgent(sphereCenter);
            return Seek(agent, centerAgent) * weight;
        }

        public class SimpleAgent : IAgent
        {
            public float MaxSpeed { get; set; }
            public float MaxForce { get; set; }
            public Vector3 Position { get; set; }
            public Vector3 Velocity { get; set; }

            public SimpleAgent(Vector3 position)
            {
                Position = position;
                Velocity =  Vector3.zero;
                MaxSpeed = 0; 
                MaxForce = 0;
            }
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidFlock : MonoBehaviour
{
    public BoidAgent boidPrefab;
    public PredatorAgent predatorPrefab;

    List<BoidAgent> boids = new List<BoidAgent>();
    List<PredatorAgent> predators = new List<PredatorAgent>();

    [Range(10, 1000)]
    public int startCount = 250;

    const float AgentDensity = 0.04f;

    [Range(1f, 30f)]
    public float driveFactor = 10f;
    [Range(1f, 30f)]
    public float maxSpeed = 5f;
    [Range(0.3f, 2f)]
    public float neighborRadius = 1f;
    [Range(1, 3)]
    public int numFlocks = 1;
    [Range(0, 3)]
    public int numPredators = 0;
    

    public float avoidanceRadiusMultiplier = 0.5f;

    float squareMaxSpeed;
    float squareNeighborRadius;
    float squareAvoidanceRadius;
    public float SquareAvoidanceRadius { get { return squareAvoidanceRadius; } }

    public float[] weights;
    Vector2 currentVelocity;
    float agentSmoothTime = 0.5f;

    //Get camera coordinates
    Vector3 bottomLeftWorld;
    Vector3 topRightWorld;

    // Start is called before the first frame update
    void Start()
    {
        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier; 

        for (int i = 0; i < startCount; i++)
        {
            BoidAgent tempBoid = Instantiate(boidPrefab,
                Random.insideUnitCircle * startCount * AgentDensity,
                Quaternion.Euler(Vector3.forward * Random.Range(0f, 360f)),
                transform);
            tempBoid.name = "Boid " + i;

            int type = Mathf.RoundToInt(Random.Range(0, numFlocks));

            if (type == 0)
            {
                if (numFlocks == 1) //If only one flock
                {
                    tempBoid.type = "1";
                    tempBoid.GetComponentInChildren<SpriteRenderer>().color = Color.Lerp(Color.white, Color.magenta, Random.Range(0f, 1f));
                }
                else
                {
                    tempBoid.type = "1";
                    tempBoid.GetComponentInChildren<SpriteRenderer>().color = Color.magenta;
                }
            }
            else if (type == 1)
            {
                tempBoid.type = "2";
                tempBoid.GetComponentInChildren<SpriteRenderer>().color = Color.yellow;
            }
            else if (type == 2)
            {
                tempBoid.type = "3";
                tempBoid.GetComponentInChildren<SpriteRenderer>().color = Color.cyan;
            }
            boids.Add(tempBoid);
        }

        for (int i = 0; i < numPredators; i++)
        {
            PredatorAgent tempPred = Instantiate(predatorPrefab,
                Random.insideUnitCircle * 5f * 1f,
                Quaternion.Euler(Vector3.forward * Random.Range(0f, 360f)),
                transform);
            tempPred.name = "Predator " + i;
            predators.Add(tempPred);
        }

        //Get camera coordinates
        Vector3 bottomLeftWorld = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        Vector3 topRightWorld = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
    }

    // Update is called once per frame
    void Update()
    {
        //Iterate over all boids
        foreach(BoidAgent boid in boids)
        {
            if (boid == null)
            {
                boids.Remove(boid);
                return;
            }

            List<BoidAgent> closeBoids = GetCloseAgents(boid, neighborRadius);
            List<BoidAgent> typeBoids = SameColourBoids(boid, closeBoids);

            if (typeBoids.Count > 0)    //Dynamic colouring (based on current neighbour size
            {
                /*
                if (numFlocks == 1)
                    boid.GetComponentInChildren<SpriteRenderer>().color = Color.Lerp(Color.cyan, Color.magenta, typeBoids.Count / 15f);
                else if (boid.type == "1")
                    boid.GetComponentInChildren<SpriteRenderer>().color = Color.Lerp(Color.cyan, Color.magenta, typeBoids.Count / 15f);
                else if (boid.type == "2")
                    boid.GetComponentInChildren<SpriteRenderer>().color = Color.Lerp(Color.yellow, Color.red, typeBoids.Count / 15f);
                else if (boid.type == "3")
                    boid.GetComponentInChildren<SpriteRenderer>().color = Color.Lerp(Color.green, Color.blue, typeBoids.Count / 15f);*/
                float packSize = 30f;
                if (boid.type == "1")
                    boid.GetComponentInChildren<SpriteRenderer>().color = Color.Lerp(Color.cyan, Color.magenta, typeBoids.Count / packSize);
                else if (boid.type == "2")
                    boid.GetComponentInChildren<SpriteRenderer>().color = Color.Lerp(Color.red, Color.yellow, typeBoids.Count / packSize);
                else if (boid.type == "3")
                    boid.GetComponentInChildren<SpriteRenderer>().color = Color.Lerp(Color.green, Color.blue, typeBoids.Count / packSize); 
            }

            Vector2 movement = CalculateMove(boid, closeBoids, typeBoids);
            movement *= driveFactor;
            if (movement.sqrMagnitude > squareMaxSpeed)            
                movement = movement.normalized * maxSpeed;
            boid.movement(movement);
        }

        //Iterate over all predators
        foreach(PredatorAgent pred in predators)        
            PredatorAction(pred);
    }

    void PredatorAction(PredatorAgent pred)
    {
        Vector2 movement = predatorMovement(pred);
        pred.movement(movement);    //Move predators            
    }

    Vector2 predatorMovement(PredatorAgent pred)
    {
        Vector2 wallMove = moveAvoidWall(pred.transform);
        Vector2 movement = pred.transform.up;   //Move forward

        List<BoidAgent> prey = GetCloseAgents(pred, squareAvoidanceRadius * 100);
        float closeFloat = float.MaxValue;

        if (prey.Count == 0)    //If no prey, move forward
        {
            return movement * 5f + wallMove * 0.05f;
        }
        else  //Move towards prey since prey exists
        {
            Transform closest = null;
            foreach (BoidAgent b in prey)
            {
                float dis = Vector2.Distance(pred.transform.position, b.transform.position);
                if (dis < closeFloat)
                    closeFloat = dis;
                closest = b.transform;
            }

            Vector2 preyDirection = (Vector2)(closest.position - pred.transform.position);
            preyDirection = Vector2.SmoothDamp(pred.transform.up, preyDirection, ref currentVelocity, 0.2f);
            return preyDirection * 3f + movement * 3f + wallMove * 0.05f;
        }      
    }

    List<BoidAgent> SameColourBoids(BoidAgent boid, List<BoidAgent> allBoids)
    {
        List<BoidAgent> returnBoids = new List<BoidAgent>();
        foreach (BoidAgent curBoid in allBoids)
        {
            if (curBoid.type == boid.type)            
                returnBoids.Add(curBoid);            
        }
        return returnBoids;
    }

    List<BoidAgent> GetCloseAgents(Agent agent, float detectionRange)
    {
        List<BoidAgent> closeBoids = new List<BoidAgent>();
        Collider2D[] collidedColliders = Physics2D.OverlapCircleAll(agent.transform.position, detectionRange);   //All colliders that overlap with ourselves

        foreach (Collider2D c in collidedColliders)
        {
            //if (c != agent.AgentCollider)    //If not ourselves, add to our return list
            //{
            BoidAgent curAgent = c.transform.GetComponent<BoidAgent>();
            if (curAgent != null)
                closeBoids.Add(c.transform.GetComponent<BoidAgent>());
            //}
        }

        return closeBoids;  //Return our collided transforms
    }

    Vector2 CalculateMove(BoidAgent boid, List<BoidAgent> closeBoids, List<BoidAgent> typeBoids)
    {        
        Vector2[] moves = new Vector2[5];
        
        moves[0] = moveTowardsBoids(boid, typeBoids);
        moves[1] = moveAlignBoids(boid, typeBoids);
        //moves[2] = moveAvoidBoids(boid, closeBoids);
        moves[2] = moveAvoidBoids(boid, typeBoids);
        moves[3] = moveAvoidWall(boid.transform);
        moves[4] = moveAvoidPredator(boid);

        Vector2 fullMove = Vector2.zero;
        for (int i = 0; i < moves.Length; i++)
        {
            Vector2 partialMove = moves[i] * weights[i];

            if (partialMove != Vector2.zero)
            {
                partialMove.Normalize();
                partialMove *= weights[i];
            }

            fullMove += partialMove;
        }

        return fullMove;
    }

    Vector2 moveAvoidPredator(BoidAgent boid)
    {
        //If no predators
        if (predators.Count == 0)
            return Vector2.zero;

        Vector2 avoidDirection = Vector2.zero;

        float squareAvoidanceRadiusSpecific = squareAvoidanceRadius * 80;
        int avoids = 0;
        foreach (PredatorAgent p in predators)
        {
            if (Vector2.SqrMagnitude(p.transform.position - boid.transform.position) < squareAvoidanceRadiusSpecific)
            {
                avoids += 1;
                avoidDirection += (Vector2)(boid.transform.position - p.transform.position);
            }
        }

        if (avoids > 0)
            avoidDirection /= avoids;
        return avoidDirection;
    }

    Vector2 moveAvoidWall(Transform trans)    //Avoids walls or center
    {        
        Vector2 avoidDirection = Vector2.zero;
        Vector2 middle = Vector2.zero;

        var cam = Camera.main;
        var viewportPosition = cam.WorldToViewportPoint(trans.transform.position);
        var newPosition = trans.transform.position;
        float offset = 0.1f;

        if (viewportPosition.x > 1 + offset)        //Right side      
            avoidDirection = middle - (Vector2)trans.position;
        else if (viewportPosition.x < 0 - offset)   //Left side
            avoidDirection = middle - (Vector2)trans.position;
        else if (viewportPosition.y > 1 + offset)        //Top side
            avoidDirection = middle - (Vector2)trans.position;
        else if (viewportPosition.y < 0 - offset)   //Bottom side
            avoidDirection = middle - (Vector2)trans.position;

        //return avoidDirection * 1.5f;

        
        Vector2 center = Vector2.zero;
        float radius = 15f;

        Vector2 centerOffset = center - (Vector2)trans.position;
        float t = centerOffset.magnitude / radius;

        if (t < 0.7f)        
            centerOffset = Vector2.zero;

        return centerOffset * t * 4f + avoidDirection * 0.1f;



    }

    Vector2 moveAvoidBoids(BoidAgent boid, List<BoidAgent> closeBoids)  //Avoidance avoid very nearby
    {
        //If no neighbours
        if (closeBoids.Count == 0)
            return Vector2.zero;

        Vector2 avoidDirection = Vector2.zero;

        float squareAvoidanceRadiusSpecific = 0f;
        int avoids = 0;
        foreach (BoidAgent b in closeBoids)
        {
            if (boid.type == b.type)
                squareAvoidanceRadiusSpecific = 10f * squareAvoidanceRadius;
            else
                squareAvoidanceRadiusSpecific = squareAvoidanceRadius * 1f;

            if (Vector2.SqrMagnitude(b.transform.position - boid.transform.position) < squareAvoidanceRadiusSpecific)
            {
                avoids += 1;
                avoidDirection += (Vector2)(boid.transform.position - b.transform.position);   
            }
        }

        if (avoids > 0)
            avoidDirection /= avoids;

        return avoidDirection;
    }

    Vector2 moveAlignBoids(BoidAgent boid, List<BoidAgent> closeBoids)  //Alignment aka match velocity
    {
        //If no neighbours
        if (closeBoids.Count == 0)
            return boid.transform.up;

        Vector2 alignMove = Vector2.zero;
        foreach (BoidAgent b in closeBoids)
            alignMove += (Vector2)b.transform.up;    //Sum all the transforms        
        alignMove /= closeBoids.Count;               //Divide by number of transforms

        return alignMove;    
    }

    Vector2 moveTowardsBoids(BoidAgent boid, List<BoidAgent> closeBoids)    //Cohesion aka move towards center of nearby
    {
        //If no neighbours
        if (closeBoids.Count == 0)
            return Vector2.zero;

        Vector2 averageMove = Vector2.zero;
        foreach (BoidAgent b in closeBoids)
            averageMove += (Vector2)b.transform.position; //Sum all the transforms        
        averageMove /= closeBoids.Count;        //Divide by number of transforms

        averageMove -= (Vector2)boid.transform.position;
        averageMove = Vector2.SmoothDamp(boid.transform.up, averageMove, ref currentVelocity, agentSmoothTime);
        return averageMove;
    }



}

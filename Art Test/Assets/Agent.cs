using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class Agent : MonoBehaviour
{
    Collider2D agentCollider;
    public Collider2D AgentCollider { get { return agentCollider; } }

    // Start is called before the first frame update
    void Start()
    {
        agentCollider = GetComponent<Collider2D>();
    }

    public void movement(Vector2 velocity)
    {
        transform.up = velocity;    //Turn Sprite
        transform.position += (Vector3)velocity * Time.deltaTime * 0.4f;   //Move Sprite   (TODO: Add over time change with max turn speed or something)
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorAgent : Agent
{
    BoxCollider2D damageBox;
    public BoxCollider2D DamageBox { get { return damageBox; } }

    private void Start()
    {
        damageBox = GetComponent<BoxCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        BoidAgent curAgent = collision.collider.GetComponent<BoidAgent>();
        if (curAgent != null)
        {
            Debug.Log("Dead bird");
            Destroy(curAgent.transform.gameObject);
        }        
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeScript : MonoBehaviour
{
    public int id;
    public float speedMod = 1f;
    public float deathTimer = 100f;
    public float currentLife = 1;
    public GameObject controller;

    //Set settings
    public float speed = 1f;
    public string mode = "Chaos";

    string direction = "";
    float rot = 0;
    SpriteRenderer rend;

    // Start is called before the first frame update
    void Start()
    {
        float minSpeed = 0.5f;
        float maxSpeed = 1f;

        //Modify rotation based on mode
        if (mode == "Chaos")
        {
            rot = Random.Range(rot - 45, rot + 45);
        }            
        else if (mode == "Order")
        {
            minSpeed = 0.3f;
            maxSpeed = 1.2f;
        }

        //Set speed and renderer
        float speedSub = (0.5f + speed * 0.7f);   //Speed from 1-2
        speedMod = Random.Range(minSpeed * speedSub, maxSpeed * speedSub);
        rend = GetComponent<SpriteRenderer>();

        //Get camera coordinates
        Vector3 bottomLeftWorld = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        Vector3 topRightWorld = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
        
        //Determine direction to travel
        int start = Random.Range(1, 5);
        if (start == 1)         //Down
        {
            transform.position = new Vector3(Random.Range(bottomLeftWorld.x, topRightWorld.x), topRightWorld.y, 0);
            direction = "Down";
            rot = 90;
        }
        else if (start == 2)    //Up
        {
            transform.position = new Vector3(Random.Range(bottomLeftWorld.x, topRightWorld.x), bottomLeftWorld.y, 0);
            direction = "Up";
            rot = 180;
        }
        else if (start == 3)    //Left
        {
            transform.position = new Vector3(topRightWorld.x, Random.Range(bottomLeftWorld.y, topRightWorld.y), 0);
            direction = "Left";
            rot = 90;
        }
        else if (start == 4)    //Right - Must be
        {
            transform.position = new Vector3(bottomLeftWorld.x, Random.Range(bottomLeftWorld.y, topRightWorld.y), 0);
            direction = "Right";
            rot = 180;
        }

        //Set normalized rotation
        transform.rotation = Quaternion.Euler(Vector3.forward * rot);
    }

    void OnEnable()
    {
        Start();
    }

    // Update is called once per frame
    void Update()
    {
        checkAlive();
        movement();
        
    }

    void checkAlive()   //Checks if within world boundaries
    {
        if (Mathf.Abs(transform.position.y) > 6.5f || Mathf.Abs(transform.position.x) > 10.5f)
        {
            gameObject.SetActive(false);    //Deactives object
            controller.GetComponent<ControllerScript>().deactiveNodes.Add(gameObject);
            controller.GetComponent<ControllerScript>().removeNode(gameObject);
        }
    }

    void movement() //Moves node
    {
        if (direction == "Up" || direction == "Down")
            transform.Translate(transform.up * Time.deltaTime * speedMod);
        else
            transform.Translate(transform.right * Time.deltaTime * speedMod);
    }
}

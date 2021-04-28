using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineScript : MonoBehaviour
{
    public float distance = 0f;
    public float fade;

    public GameObject obj1;
    public GameObject obj2;
    public GameObject controller;

    //public settings
    public string theme = "Space";

    public bool onceActive = false;
    public float maxDistance = 1.7f;
    float timeCheck = 0.1f;
    LineRenderer line;

    public int selection = 1;

    // Start is called before the first frame update
    void Start()
    {
        onceActive = false;
        fade = 0;
        line = GetComponent<LineRenderer>();

        Color c1, c2;

        if (theme == "Space")
        {
            float redMax = 0.3f;
            float greenMax = 0.3f;
            float blueMax = 1.0f;
            c1 = new Color(Random.Range(0.1f, redMax), Random.Range(0.1f, greenMax), Random.Range(0.5f, blueMax));
            c2 = new Color(Random.Range(0.1f, redMax), Random.Range(0.1f, greenMax), Random.Range(0.5f, blueMax));
        }
        else if (theme == "Fire")
        {
            float redMax = 1.0f;
            float greenMax = 0.2f;
            float blueMax = 0.3f;
            c1 = new Color(Random.Range(0.2f, redMax), Random.Range(0.1f, greenMax), Random.Range(0.1f, blueMax));
            c2 = new Color(Random.Range(0.2f, redMax), Random.Range(0.1f, greenMax), Random.Range(0.1f, blueMax));
        }
        else
        { 
            float redMax = 0.5f;
            float greenMax = 1.0f;
            float blueMax = 0.5f;
            c1 = new Color(Random.Range(0.1f, redMax), Random.Range(0.5f, greenMax), Random.Range(0.2f, blueMax));
            c2 = new Color(Random.Range(0.1f, redMax), Random.Range(0.5f, greenMax), Random.Range(0.2f, blueMax));
        }

        line.startColor = c1;
        line.endColor = c2;

        line.material.SetColor("_TintColor", new Color(1, 1, 1, 0));
    }

    void OnEnable()
    {
        Start();
    }

    public void setObjects(GameObject o1, GameObject o2)
    {
        obj1 = o1;
        obj2 = o2;
    }

    // Update is called once per frame
    void Update()
    {
        if (!obj1.activeSelf || !obj2.activeSelf)
        {
            removeObject();
            return;
        }

        timeCheck -= Time.deltaTime;
        if (timeCheck < 0)
        {
            distance = Vector3.Distance(obj1.transform.position, obj2.transform.position);
            distance = Vector2.Distance(new Vector2(obj1.transform.position.x, obj1.transform.position.y), new Vector2(obj2.transform.position.x, obj2.transform.position.y));

            if (distance > maxDistance && onceActive)   //Already activated then moved out of distance... delete
            {
                removeObject();
                return;
            }

            fade = calcFade();
            if (fade < 0 && onceActive)
                return;            

            line.material.SetColor("_TintColor", new Color(1, 1, 1, fade));
        }


        line.positionCount = 2;
        var points = new Vector3[2];
        points[0] = new Vector3(obj1.transform.position.x, obj1.transform.position.y, obj1.transform.position.z);
        points[1] = new Vector3(obj2.transform.position.x, obj2.transform.position.y, obj2.transform.position.y);
        line.SetPositions(points);
    }

    float calcFade()
    {
        float tempFade = 1 - (distance / maxDistance) - 0.1f;
        if (tempFade > 1)
            tempFade = 1f;
        else if (tempFade < 0)        
            tempFade = 0f;        
        if (tempFade > 0)
            onceActive = true;

        return tempFade;
    }

    void removeObject()
    {
        //gameObject.SetActive(false);    //Deactives object
        controller.GetComponent<ControllerScript>().lineList.Remove(gameObject);
        Destroy(gameObject);
    }
}

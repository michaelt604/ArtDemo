using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerScript : MonoBehaviour
{
    public GameObject Node;
    public GameObject Line;
    public int startNodes = 100;
    public int maxNodes = 200;
    public int totalNodes = 0;
    public int spawnId = 0;

    //Internal Settings
    public float maxDistance = 2f;
    public float speedMultiplier = 3f;
    public string mode = "";
    public string theme = "";


    public List<GameObject> nodeList;
    public List<GameObject> lineList;
    public List<GameObject> deactiveNodes;
    public List<GameObject> deactiveLines;

    public HashSet<(int, int)> linkedIds;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Starting controller");
        linkedIds = new HashSet<(int, int)>();

        speedMultiplier = UnityEngine.PlayerPrefs.GetFloat(PlayerPrefKeys.interSpeed);
        mode = UnityEngine.PlayerPrefs.GetString(PlayerPrefKeys.interMode);
        theme = UnityEngine.PlayerPrefs.GetString(PlayerPrefKeys.interTheme);

        Debug.Log("Speed: " + speedMultiplier + ", Mode: " + mode + ", Theme: " + theme);
    }

    // Update is called once per frame
    void Update()
    {
        if (totalNodes < maxNodes)
            spawnNode();
        checkLines();
    }

    void checkLines()   //Creates a line link between nodes if they are within distance, and adds them to the already linked ids so they don't have to be checked again
    {
        for (int i = 0; i < nodeList.Count; ++i)
        {
            GameObject node1 = nodeList[i];
            int id1 = node1.GetComponent<NodeScript>().id;

            for (int j = i+1; j < nodeList.Count; ++j)
            {
                GameObject node2 = nodeList[j];
                int id2 = node2.GetComponent<NodeScript>().id;
                if (!linkedIds.Contains((id1, id2)))    //If we haven't linked before you can check distance
                {
                    float distance = Vector2.Distance(new Vector2(node1.transform.position.x, node1.transform.position.y), new Vector2(node2.transform.position.x, node2.transform.position.y));

                    if (maxDistance > distance)
                    {
                        GameObject line = getLine();    //Get a line from pool or create a new line
                        line.GetComponent<LineScript>().maxDistance = maxDistance;
                        line.GetComponent<LineScript>().obj1 = node1;
                        line.GetComponent<LineScript>().obj2 = node2;
                        lineList.Add(line);
                        linkedIds.Add((id1, id2));  //Add to our linked id set
                    }
                }
            }
        }
    }

    public void removeNode(GameObject toRemove)
    {
        nodeList.Remove(toRemove);
        totalNodes -= 1;
    }

    void spawnNode()
    {
        totalNodes += 1;
        GameObject node = getNode();    //Gets object from pool or creates a new object
        node.GetComponent<NodeScript>().speed = speedMultiplier;
        nodeList.Add(node);
    }

    GameObject getNode()    //Retrieves a node from the deactivated node bucket, or create a new node
    {
        foreach (GameObject node in deactiveNodes)   //For each node
        {
            if (node != null)   //If node isn't null
            {
                node.SetActive(true);   //Reactivates node
                deactiveNodes.Remove(node);

                node.GetComponent<NodeScript>().id = spawnId;
                spawnId += 1;
                return node;    //Return the node
            }
        }

        //GameObject newNode = Instantiate(Node, transform);
        GameObject newNode = Instantiate(Node);

        newNode.GetComponent<NodeScript>().controller = gameObject;
        newNode.GetComponent<NodeScript>().id = spawnId;
        newNode.GetComponent<NodeScript>().mode = mode;  // Order or Chaos
        spawnId += 1;
        return newNode;    //If no node found return new node
    }

    GameObject getLine()    //Retrieves a line from the deactivated line bucket, or create a new line
    {
        foreach (GameObject line in deactiveLines)   //For each node
        {
            if (line != null)   //If node isn't null
            {
                line.SetActive(true);   //Reactivates node
                deactiveLines.Remove(line);

                return line;    //Return the node
            }
        }

        GameObject newLine = Instantiate(Line, transform);
        newLine.GetComponent<LineScript>().controller = gameObject;
        newLine.GetComponent<LineScript>().theme = theme;   //Space or Fire or Serenity
        return newLine;    //If no node found return new node
    }
}

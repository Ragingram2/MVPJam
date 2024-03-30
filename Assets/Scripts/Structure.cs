using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;


//public class PartInfo
//{
//    public Vector3 position;
//    public Quaternion rotation;
//    public BlockType type;
//    public bool overrideGlobal;
//    public float force;

//    public string ToJson()
//    {
//        string json = "";
//        json += JsonUtility.ToJson(position) + ", \n";
//        json += JsonUtility.ToJson(rotation) + ", \n";
//        json += $"\"BlockType\":{(int)type}\n";
//        json += $"\"OverrideGlobal\":{(int)type}";
//        return json;
//    }
//}

public class Structure : MonoBehaviour
{
    [SerializeField]
    private float globalForce = 1.0f;
    [SerializeField]
    private bool dampners = true;
    private bool play = false;
    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 inputVec;
    private Rigidbody rb;
    private Dictionary<ThrusterOrientation, int> thrusterCounts = new Dictionary<ThrusterOrientation, int>();
    private List<Part> parts = new List<Part>();
    private List<Thruster> thrusters = new List<Thruster>();

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        rb = GetComponent<Rigidbody>();
        Refresh();
    }

    private void FixedUpdate()
    {
        if (play)
        {
            foreach (var thruster in thrusters)
            {
                var vec = dampners && Mathf.Approximately(inputVec.magnitude, 0) ? inputVec : -rb.velocity.normalized;
                thruster.ApplyForce(rb, inputVec, thrusterCounts[thruster.orientation], globalForce);
            }
        }
    }

    void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            Refresh();
            play = !play;
        }

        if(Keyboard.current.deleteKey.wasPressedThisFrame)
        {
            Destroy(gameObject);
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            string path = Application.streamingAssetsPath + "/Ship.json";
            File.WriteAllText(path, SaveStructure());
        }

        rb.isKinematic = !play;

        if (play)
        {
            inputVec = Vector3.zero;
            if (Keyboard.current.spaceKey.isPressed)
                inputVec.y = 1.0f;

            if (Keyboard.current.leftShiftKey.isPressed)
                inputVec.y = -1.0f;

            if (Keyboard.current.rightArrowKey.isPressed)
                inputVec.x = 1.0f;

            if (Keyboard.current.leftArrowKey.isPressed)
                inputVec.x = -1.0f;

            if (Keyboard.current.upArrowKey.isPressed)
                inputVec.z = 1.0f;

            if (Keyboard.current.downArrowKey.isPressed)
                inputVec.z = -1.0f;
        }
        else
        {
            transform.position = startPos;
            transform.rotation = startRot;
        }
    }

    public void Refresh()
    {
        parts = GetComponentsInChildren<Part>().ToList();
        thrusters = GetComponentsInChildren<Thruster>().ToList();
        for (int i = 0; i < (int)ThrusterOrientation.Count; i++)
        {
            var orientation = (ThrusterOrientation)i;
            if (!thrusterCounts.ContainsKey(orientation))
            {
                thrusterCounts.Add(orientation, 0);
                continue;
            }
            thrusterCounts[orientation] = 0;
        }

        foreach (var thruster in thrusters)
        {
            thrusterCounts[thruster.orientation]++;
        }
    }

    //Write the structure to a simple JSON file with block types and positions, then when we load it back in all we need to do is instantiate the block type with the appropriate prefab,
    //the loading of the structure will probably be relegated to the building system
    public string SaveStructure()
    {
        Refresh();
        string json = "{\n" +
            "\"Structure\":[\n";
        int i = 0;
        foreach (var part in parts)
        {
            if (i != 0)
                json += ",\n";

            json += "{\n";
            if (part.type == BlockType.Default || part.type == BlockType.Gyroscope || part.type == BlockType.Count)
                json += part.ToJson();
            if (part.type == BlockType.Thruster)
                json += ((Thruster)part).ToJson();
            json += "}";
            i++;
        }
        json += "]\n}\n";
        return json;
    }

    private void OnDrawGizmos()
    {
        if (rb)
        {
            rb.automaticCenterOfMass = true;
            Gizmos.DrawSphere(transform.position + (transform.rotation * rb.centerOfMass), .5f);
        }
    }
}

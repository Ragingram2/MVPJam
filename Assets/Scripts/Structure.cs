using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
using static ControlMap;


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

public class Structure : MonoBehaviour, IStructureActions, IShipActions
{
    [SerializeField]
    private float globalForce = 1.0f;
    [SerializeField]
    private bool dampners = true;
    [SerializeField]
    private bool keepHeight = true;
    private bool play = false;
    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 inputVec;
    private Rigidbody rb;
    private Dictionary<ThrusterOrientation, int> thrusterCounts = new Dictionary<ThrusterOrientation, int>();
    private List<Part> parts = new List<Part>();
    private List<Thruster> thrusters = new List<Thruster>();
    private ControlMap controls;

    public void OnEnable()
    {
        if (controls == null)
        {
            controls = new ControlMap();
        }
        EnableStructureControls();
    }

    public void OnDisable()
    {
        DisableStructureControls();
    }

    public void EnableStructureControls()
    {
        controls.Structure.SetCallbacks(this);
        controls.Structure.Enable();
    }

    public void DisableStructureControls()
    {
        controls.Structure.Disable();
    }

    public void EnableShipControls()
    {
        controls.Ship.SetCallbacks(this);
        controls.Ship.Enable();
    }

    public void DisableShipControls()
    {
        controls.Ship.Disable();
    }

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
                //var vec = dampners && Mathf.Approximately(inputVec.magnitude, 0) ? inputVec : -rb.velocity.normalized;
                thruster.ApplyForce(rb, inputVec, thrusterCounts[thruster.orientation], globalForce, keepHeight);
            }
        }
    }

    void Update()
    {
        rb.isKinematic = !play;

        if (!play)
        {
            transform.position = startPos;
            transform.rotation = startRot;
            //inputVec = Vector3.zero;
            //if (Keyboard.current.spaceKey.isPressed)
            //    inputVec.y = 1.0f;

            //if (Keyboard.current.leftShiftKey.isPressed)
            //    inputVec.y = -1.0f;

            //if (Keyboard.current.rightArrowKey.isPressed)
            //    inputVec.x = 1.0f;

            //if (Keyboard.current.leftArrowKey.isPressed)
            //    inputVec.x = -1.0f;

            //if (Keyboard.current.upArrowKey.isPressed)
            //    inputVec.z = 1.0f;

            //if (Keyboard.current.downArrowKey.isPressed)
            //    inputVec.z = -1.0f;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
            inputVec = context.ReadValue<Vector3>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {

    }

    public void OnSaveStructure(InputAction.CallbackContext context)
    {
        if (play) return;

        if (context.action.IsPressed())
        {
            string path = Application.streamingAssetsPath + "/Ship.json";
            File.WriteAllText(path, SaveStructure());
        }
    }

    public void OnTogglePlay(InputAction.CallbackContext context)
    {
        if (context.action.IsPressed())
        {
            Refresh();
            play = !play;

            if (play)
                EnableShipControls();
            else
                DisableShipControls();
        }
    }

    public void OnDeleteStructure(InputAction.CallbackContext context)
    {
        if (play) return;

        if (context.action.IsPressed())
        {
            Destroy(gameObject);
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

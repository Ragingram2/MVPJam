using NaughtyAttributes;
using SimpleJSON;
using System;
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


public class Structure : MonoBehaviour, IStructureActions, IShipActions
{
    [SerializeField]
    private float globalForce = 1.0f;
    [SerializeField]
    public PIDController pidX = new PIDController();
    [SerializeField]
    public PIDController pidY = new PIDController();
    [SerializeField]
    public PIDController pidZ = new PIDController();

    public bool play = false;
    private Vector3 startPos;
    private Quaternion startRot;
    public Vector3 inputVec;
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
        //rb.maxLinearVelocity = 20.0f;
        Refresh();
    }

    private void FixedUpdate()
    {
        if (play)
        {
            foreach (var thruster in thrusters)
            {
                thruster.ApplyForce(inputVec, globalForce);
            }

            //Debug.Log("==============================");
            //Debug.Log($"Velocity before RCS: {rb.velocity}");
            RCS(pidX, 0, transform.right, ThrusterOrientation.Left);
            RCS(pidY, 0, transform.up, ThrusterOrientation.Up);
            RCS(pidZ, 0, transform.forward, ThrusterOrientation.Forward);

            var velocity = rb.velocity;
            if (Mathf.Abs(rb.velocity.x) <= 0.1f)
                velocity.x = 0.0f;
            if (Mathf.Abs(rb.velocity.y) <= 0.1f)
                velocity.x = 0.0f;
            if (Mathf.Abs(rb.velocity.z) <= 0.1f)
                velocity.x = 0.0f;

            rb.velocity = velocity;
        }
    }

    void RCS(PIDController pid, float targetVel, Vector3 forceAxis, ThrusterOrientation orientation)
    {
        var delta = 0.0f;
        if (orientation == ThrusterOrientation.Right || orientation == ThrusterOrientation.Left)
            delta = pid.Update(targetVel, rb.velocity.x, Time.fixedDeltaTime);
        else if (orientation == ThrusterOrientation.Up || orientation == ThrusterOrientation.Down)
            delta = pid.Update(targetVel, rb.velocity.y, Time.fixedDeltaTime);
        else if (orientation == ThrusterOrientation.Forward || orientation == ThrusterOrientation.Backward)
            delta = pid.Update(targetVel, rb.velocity.z, Time.fixedDeltaTime);

        if (Mathf.Abs(delta) <= 0.5f) return;
        rb.AddForce(forceAxis * delta * globalForce, ForceMode.Force);
    }

    void Update()
    {
        rb.isKinematic = !play;

        if (!play)
        {
            transform.position = startPos;
            transform.rotation = startRot;
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
            thruster.ResetThruster();
        }
        pidX.Reset();
        pidY.Reset();
        pidZ.Reset();
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
            {
                json += ((Thruster)part).ToJson();
            }
            json += "}";
            i++;
        }
        json += "],\n";
        json += pidX.ToJson() + "\n";
        json += pidY.ToJson() + "\n";
        json += pidZ.ToJson() + "\n";
        json += "}\n";
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

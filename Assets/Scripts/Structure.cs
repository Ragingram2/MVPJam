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
    public float globalForce = 10.0f;
    [Range(-10.0f, 10.0f)]
    public float proportionalGain;
    [Range(-10.0f, 10.0f)]
    public float intergralGain;
    [Range(-10.0f, 10.0f)]
    public float derivativeGain;

    public bool play = false;
    private Vector3 startPos;
    private Quaternion startRot;
    public Vector3 inputVec;
    private Rigidbody rb;
    private List<Part> parts = new List<Part>();
    private List<Thruster> thrusters = new List<Thruster>();
    private ControlMap controls;

    #region Input
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
    #endregion

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
                thruster.ThrusterUpdate(proportionalGain, intergralGain, derivativeGain);

            if (rb.velocity.magnitude < 1.0f)
            {
                rb.velocity = Vector3.zero;
                return;
            }

        }
    }

    void Update()
    {
        rb.isKinematic = !play;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
            inputVec = context.ReadValue<Vector3>();
    }
    public void OnLook(InputAction.CallbackContext context)
    {

    }
    public void OnTogglePlay(InputAction.CallbackContext context)
    {
        if (context.action.IsPressed())
        {
            Refresh();
            play = !play;

            if (!play)
            {
                transform.position = startPos;
                transform.rotation = startRot;
            }

            if (play)
                EnableShipControls();
            else
                DisableShipControls();
        }
    }
    public void Refresh()
    {
        parts = GetComponentsInChildren<Part>().ToList();
        thrusters = GetComponentsInChildren<Thruster>().ToList();
        foreach (var thruster in thrusters)
            thruster.ResetThruster();
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

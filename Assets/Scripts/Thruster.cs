using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEditor;
using Cinemachine.Utility;
using UnityEngine.Windows;

[System.Serializable]
public enum ThrusterOrientation
{
    Up,
    Down,
    Left,
    Right,
    Forward,
    Backward,
    Count
}

public class Thruster : Part
{
    public bool overrideGlobal = false;
    public float force = 1.0f;
    public ThrusterOrientation orientation;
    public GameObject exhaustMesh;

    private float throttle = 0.0f;
    private Rigidbody rb;
    private Structure structure;
    private PIDController controller = new PIDController();

    private Vector3 displayVec;
    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        structure = GetComponentInParent<Structure>();
        //Straight up devilry right here
        //Debug.Log($"{transform.up}"); //->(1.00,0.00,0.00)
        //Debug.Log($"up.x: {transform.up.x}"); //->1
        //Debug.Log($"up.x == 1.0f:{transform.up.x == 1.0f}");//->False
        //Debug.Log($"up.x == 1:{transform.up.x == 1}");//->False
        //Debug.Log($"up.x > 0:{transform.up.x > 0}");//->True

        if (Mathf.Approximately(Vector3.Dot(transform.up, Vector3.up), 1.0f))
            orientation = ThrusterOrientation.Up;
        if (Mathf.Approximately(Vector3.Dot(transform.up, -Vector3.up), 1.0f))
            orientation = ThrusterOrientation.Down;

        if (Mathf.Approximately(Vector3.Dot(transform.up, Vector3.right), 1.0f))
            orientation = ThrusterOrientation.Right;
        if (Mathf.Approximately(Vector3.Dot(transform.up, -Vector3.right), 1.0f))
            orientation = ThrusterOrientation.Left;

        if (Mathf.Approximately(Vector3.Dot(transform.up, Vector3.forward), 1.0f))
            orientation = ThrusterOrientation.Forward;
        if (Mathf.Approximately(Vector3.Dot(transform.up, -Vector3.forward), 1.0f))
            orientation = ThrusterOrientation.Backward;
    }

    public void ThrusterUpdate(float p, float i, float d)
    {
        controller.proportionalGain = p;
        controller.integralGain = i;
        controller.derivativeGain = d;

        controller.SetRange(-10.0f, 10.0f);
        var targetVel = Vector3.Project(rb.velocity, transform.up);
        displayVec = targetVel;
        throttle = controller.Update(Vector3.zero, targetVel, Time.fixedDeltaTime).magnitude;
        rb.AddForceAtPosition(transform.up * structure.globalForce * throttle, transform.position, ForceMode.Force);
    }

    public void ResetThruster()
    {
        throttle = 0;
        controller.Reset();
    }

    private void Update()
    {
        if (exhaustMesh)
        {
            var scal = exhaustMesh.transform.localScale;
            scal.y = Mathf.Clamp(throttle, 0.5f, 3.0f);
            exhaustMesh.transform.localScale = scal;
        }
    }

    private void OnDrawGizmos()
    {
        //Handles.Label(transform.position, throttle.ToString());
        Handles.Label(transform.position, displayVec.ToString());
        Handles.Label(transform.position - transform.up, transform.up.ToString());
        if (rb)
            Handles.Label(transform.position - (transform.up * 2.0f), rb.velocity.ToString());
    }

    public override string ToJson()
    {
        string json = "";
        json += $",\"OverrideGlobal\":{overrideGlobal},\n";
        json += $"\"Force\":{force}";

        return base.ToJson() + json;
    }

    public override void FromJson(string json)
    {
        base.FromJson(json);
        var p = JSON.Parse(json);
        overrideGlobal = p["OverrideGlobal"].AsBool;
        force = p["Force"].AsFloat;
    }
}

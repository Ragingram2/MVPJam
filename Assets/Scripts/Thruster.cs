using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEditor;
using Cinemachine.Utility;

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

[System.Serializable]
public enum OrientationGroup
{
    X,
    Y,
    Z
}

public class Thruster : Part
{
    public bool overrideGlobal = false;
    public float force = 1.0f;
    public ThrusterOrientation orientation;
    public GameObject exhaustMesh;
    private Structure structure;

    private float throttle = 0.0f;
    private Rigidbody rb;

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

    public void ApplyRCSForce(float deltaVel, float globalForce)
    {
        //deltaVel = Mathf.Max(0.0f, deltaVel);
        throttle = deltaVel;

        float forceToApply = deltaVel * globalForce;
        rb.AddForceAtPosition(transform.up * forceToApply, transform.position, ForceMode.Force);
    }

    public void ApplyForce(Vector3 input, float globalForce)
    {
        var angleToInput = Mathf.Max(0.0f,Vector3.Dot(input, transform.up));
        //Debug.Log(angleToInput);
        throttle = angleToInput;
        float forceToApply = angleToInput * (overrideGlobal ? force : globalForce);
        rb.AddForceAtPosition(transform.up * forceToApply, transform.position, ForceMode.Force);
    }

    public void ResetThruster()
    {
        throttle = 0;
    }

    private void Update()
    {
        if (exhaustMesh)
        {
            var scal = exhaustMesh.transform.localScale;
            scal.y = Mathf.Clamp(((throttle / 1.0f) * 3.0f) + .5f, .5f, 3f);
            exhaustMesh.transform.localScale = scal;
        }
    }

    private float displayValue;
    private void OnDrawGizmos()
    {
        //Handles.Label(transform.position, _throttle.ToString());
        Handles.Label(transform.position, displayValue.ToString());
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

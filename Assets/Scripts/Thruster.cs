using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

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
    [SerializeField]
    public float force = 1.0f;
    [HideInInspector]
    public ThrusterOrientation orientation;
    private Structure m_structure;

    private float displayValue;

    private void Start()
    {
        m_structure = GetComponentInParent<Structure>();
        //Straight up devilry right here
        //Debug.Log($"{transform.up}"); //->(1.00,0.00,0.00)
        //Debug.Log($"up.x: {transform.up.x}"); //->1
        //Debug.Log($"up.x == 1.0f:{transform.up.x == 1.0f}");//->False
        //Debug.Log($"up.x == 1:{transform.up.x == 1}");//->False
        //Debug.Log($"up.x > 0:{transform.up.x > 0}");//->True

        if (transform.up.y >= 0.0f)
            orientation = ThrusterOrientation.Up;
        if (transform.up.y < 0.0f)
            orientation = ThrusterOrientation.Down;

        if (transform.up.x >= 0.0f)
            orientation = ThrusterOrientation.Right;
        if (transform.up.x < 0.0f)
            orientation = ThrusterOrientation.Left;

        if (transform.up.z >= 0.0f)
            orientation = ThrusterOrientation.Forward;
        if (transform.up.z < 0.0f)
            orientation = ThrusterOrientation.Backward;
    }

    public void ApplyForce(Rigidbody rb, Vector3 input, int thrusterCount, float globalForce, bool keepHeight)
    {
        var comp = Mathf.Max(0, Vector3.Dot(input, transform.up));
        var gravForce = Physics.gravity.magnitude;
        var COM = rb.transform.position + (transform.rotation * rb.centerOfMass);
        var dstFromCOM = (transform.position - COM).magnitude;
        //var forceAdjusted = ((overrideGlobal ? force : globalForce) / dstFromCOM);
        float forceAdjusted = 1.0f/ dstFromCOM;
        var throttle = (keepHeight ? 1.0f : 0.0f) + (comp * (thrusterCount / 10.0f));
        rb.AddForceAtPosition(transform.up * gravForce * forceAdjusted * throttle, transform.position, ForceMode.Force);
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
        overrideGlobal = p["OrverrideGlobal"].AsBool;
        force = p["Force"].AsFloat;
    }

    //private void OnDrawGizmos()
    //{
    //    Handles.Label(transform.position, $"{displayValue}");
    //}
}

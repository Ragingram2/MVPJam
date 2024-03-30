using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEditor;

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
    private Structure m_structure;
    private float displayValue;

    private bool applyingForce = false;

    private void Start()
    {
        m_structure = GetComponentInParent<Structure>();
        //Straight up devilry right here
        //Debug.Log($"{transform.up}"); //->(1.00,0.00,0.00)
        //Debug.Log($"up.x: {transform.up.x}"); //->1
        //Debug.Log($"up.x == 1.0f:{transform.up.x == 1.0f}");//->False
        //Debug.Log($"up.x == 1:{transform.up.x == 1}");//->False
        //Debug.Log($"up.x > 0:{transform.up.x > 0}");//->True

        if (Vector3.Dot(transform.up, Vector3.up) == 1.0f)
            orientation = ThrusterOrientation.Up;
        if (Vector3.Dot(transform.up, -Vector3.up) == 1.0f)
            orientation = ThrusterOrientation.Down;

        if (Vector3.Dot(transform.up, Vector3.right) == 1.0f)
            orientation = ThrusterOrientation.Right;
        if (Vector3.Dot(transform.up, -Vector3.right) == 1.0f)
            orientation = ThrusterOrientation.Left;

        if (Vector3.Dot(transform.up, Vector3.forward) == 1.0f)
            orientation = ThrusterOrientation.Forward;
        if (Vector3.Dot(transform.up, -Vector3.forward) == 1.0f)
            orientation = ThrusterOrientation.Backward;
    }

    public void ApplyForce(Rigidbody rb, Vector3 input, float globalForce, float thrusterCount, float currentHeight, float targetHeight, bool keepHeight)
    {
        var comp = Mathf.Max(0, Vector3.Dot(input, transform.up));
        var COM = rb.transform.position + (transform.rotation * rb.centerOfMass);
        var dstFromCOM = (transform.position - COM).magnitude;
        //var forceAdjusted = ((overrideGlobal ? force : globalForce) / dstFromCOM);
        //float forceAdjusted = 1.0f/ dstFromCOM;

        var lerpStep = (currentHeight - targetHeight) * Time.deltaTime;
        var throttle = (((keepHeight ? targetHeight - Mathf.Lerp(currentHeight, targetHeight, lerpStep) : comp) * globalForce) / thrusterCount);
        displayValue = thrusterCount;
        var forceToApply = transform.up * comp * globalForce * (1.0f / thrusterCount);
        applyingForce = forceToApply.magnitude > 0;
        rb.AddForceAtPosition(forceToApply, transform.position, ForceMode.Force);
    }

    private void Update()
    {
        if (exhaustMesh)
            exhaustMesh.SetActive(applyingForce);
    }

    private void OnDrawGizmos()
    {
        //Handles.Label(transform.position, displayValue.ToString());
        Handles.Label(transform.position, transform.up.ToString());
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
}

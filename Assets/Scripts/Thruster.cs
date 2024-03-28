using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ThrusterOrientation
{
    Up,
    Down,
    Left,
    Right,
    Forward,
    Backward
}

public class Thruster : Part
{
    [SerializeField]
    private float force = 1.0f;
    private ThrusterOrientation orientation;

    private void Start()
    {
        var heightMax = Mathf.Sin(Mathf.Deg2Rad * 15.0f);
        if (Mathf.Abs(transform.up.y) < heightMax)
        {
            //Figure out how to get the orientation when its not clearly up or down

        }
        else
        {
            if (transform.up.y > 0.0f)
                orientation = ThrusterOrientation.Up;
            else if (transform.up.y < 0.0f)
                orientation = ThrusterOrientation.Down;
        }
    }

    public void ApplyForce(Rigidbody rb, float throttle)
    {
        var gravForce = Physics.gravity.magnitude;
        rb.AddForceAtPosition(transform.up * gravForce * force * throttle, transform.position, ForceMode.Force);
    }
}

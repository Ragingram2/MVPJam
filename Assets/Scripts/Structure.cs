using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Structure : MonoBehaviour
{
    private bool play = false;
    private Vector3 startPos;
    private Quaternion startRot;

    private List<Thruster> thrusters = new List<Thruster>();
    private Rigidbody rb;
    private float throttle = 0.0f;
    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        foreach (var thruster in thrusters)
        {
            thruster.ApplyForce(rb, throttle);
        }
    }

    void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            Refresh();
            play = !play;
        }

        rb.isKinematic = !play;

        if (play)
        {
            if (Keyboard.current.spaceKey.isPressed)
                throttle += .1f;

            if (Keyboard.current.leftShiftKey.isPressed)
                throttle -= .1f;

            throttle = Mathf.Max(0.0f, throttle);
        }
        else
        {
            transform.position = startPos;
            transform.rotation = startRot;
        }
    }

    public void Refresh()
    {
        thrusters = GetComponentsInChildren<Thruster>().ToList();
    }
}

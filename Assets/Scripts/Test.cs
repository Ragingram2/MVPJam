using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{
    [SerializeField]
    private float throttle = 1.0f;
    [SerializeField]
    private float targetHeight = 10.0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var rb = GetComponent<Rigidbody>();
        //if (transform.position.y != targetHeight)
        //{
        //    var diff = targetHeight - transform.position.y;

        //    throttle = 1.0f + Mathf.Lerp(diff, 0, Time.fixedDeltaTime);
        //}

        if (Keyboard.current.spaceKey.isPressed)
        {
            throttle = 1.5f;
        }
        else
        {
            throttle = 1.0f;
        }

        rb.AddForce(-Physics.gravity * throttle, ForceMode.Force);
    }
}

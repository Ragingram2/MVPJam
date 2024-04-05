using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gyroscope : Part
{


    private Rigidbody rb;
    private Structure structure;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        structure = GetComponentInParent<Structure>();
    }

    // Update is called once per frame
    public void ApplyTorque()
    {
        
    }
}

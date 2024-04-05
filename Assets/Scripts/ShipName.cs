using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShipName : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TMP_InputField>().onSelect.AddListener((string text) =>
        {
            FlyCamera.UISelected = true;
        });

        GetComponent<TMP_InputField>().onEndEdit.AddListener((string text) =>
        {
            FlyCamera.UISelected = false;
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

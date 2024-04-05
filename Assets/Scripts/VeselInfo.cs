using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VeselInfo : MonoBehaviour
{
    public TMP_Text name;
    public TMP_Text dateTime;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            BuildingSystem.shipToLoadPath = name.text;
        });
    }
}

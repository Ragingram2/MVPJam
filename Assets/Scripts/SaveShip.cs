using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveShip : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField shipName;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            FindAnyObjectByType<BuildingSystem>().SaveShip(shipName.text);
        });
    }
}

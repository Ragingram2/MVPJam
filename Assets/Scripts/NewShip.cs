using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewShip : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField shipName;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => 
        {
            shipName.text = "New Ship";
            FindAnyObjectByType<BuildingSystem>().NewShip();
        });
    }

}

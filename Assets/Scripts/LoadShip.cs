using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadShip : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            FindAnyObjectByType<BuildingSystem>().LoadShip();
            transform.parent.gameObject.SetActive(false);
        });
    }
}

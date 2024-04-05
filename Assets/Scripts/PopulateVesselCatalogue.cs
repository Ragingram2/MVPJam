using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopulateVesselCatalogue : MonoBehaviour
{
    [SerializeField]
    private GameObject shipInfoPrefab;
    [SerializeField]
    private Transform contentTransform;

    private List<VeselInfo> infos = new List<VeselInfo>();

    void Awake()
    {
        foreach (var comp in infos)
        {
            Destroy(comp.gameObject);
        }
        infos.Clear();
        foreach (var path in BuildingSystem.shipFilePaths)
        {
            var info = Instantiate(shipInfoPrefab, contentTransform);
            var infoComp = info.GetComponent<VeselInfo>();
            infoComp.name.text = Path.GetFileName(path);
            infoComp.dateTime.text = Directory.GetCreationTime(path).ToString();
            infos.Add(infoComp);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using SimpleJSON;
using static ControlMap;
using UnityEngine.Rendering;
using System.Linq;

[System.Serializable]
public enum BlockType
{
    Default = 0,
    Thruster = 1,
    Gyroscope = 2,
    Count = 3
}

[System.Serializable]
public class PartData
{
    public BlockType Type;
    public GameObject partPrefab;
    public GameObject partPreviewPrefab;
}

public class BuildingSystem : MonoBehaviour, IBuildSystemActions
{
    [SerializeField]
    private GameObject structurePrefab;
    [SerializeField]
    private List<PartData> parts = new List<PartData>();
    [SerializeField]
    private LayerMask raycastMask;

    private RaycastHit hit;
    private GameObject partPreview;
    private BlockType currentType = BlockType.Default;
    private static GameObject structureGO;
    private static Structure currentStructure => structureGO ? structureGO.GetComponent<Structure>() : null;

    private ControlMap controls;
    private Part hitPart = null;

    public static string shipToLoadPath = "/Ship.json";
    public static List<string> shipFilePaths = new List<string>();

    void Start()
    { 
        partPreview = Instantiate(parts[(int)currentType].partPreviewPrefab);
        partPreview.SetActive(false);

        shipFilePaths = Directory.EnumerateFiles(Application.streamingAssetsPath).ToList();
    }

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Destroy(partPreview);
            currentType = BlockType.Default;
            partPreview = Instantiate(parts[(int)currentType].partPreviewPrefab);
            partPreview.SetActive(false);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            Destroy(partPreview);
            currentType = BlockType.Thruster;
            partPreview = Instantiate(parts[(int)currentType].partPreviewPrefab);
            partPreview.SetActive(false);
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            Destroy(partPreview);
            currentType = BlockType.Gyroscope;
            partPreview = Instantiate(parts[(int)currentType].partPreviewPrefab);
            partPreview.SetActive(false);
        }

        if (!currentStructure) return;

        if (!currentStructure.play)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            partPreview.SetActive(false);
            hitPart = null;
            if (Physics.Raycast(ray, out hit, 100, raycastMask))
            {
                hitPart = hit.collider.transform.GetComponentInParent<Part>();
                if (hitPart)
                {
                    var pos = hitPart.GetConnector(hit.normal);
                    partPreview.SetActive(true);
                    partPreview.transform.position = pos;
                    partPreview.transform.up = -hit.normal;
                }
            }
        }
    }

    public GameObject InitializeStructure()
    {
        if (structureGO != null)
        {
            Destroy(structureGO);
        }

        return structureGO = Instantiate(structurePrefab, null);
    }

    public void NewShip()
    {
        structureGO = InitializeStructure();
        Instantiate(parts[(int)BlockType.Default].partPrefab, structureGO.transform, false);
    }

    public void SaveShip(string shipName)
    {
        SaveStructure(shipName);
    }

    public void LoadShip()
    {
        LoadStructure(InitializeStructure(), parts);
    }

    public static string SaveStructure(string shipName)
    {
        currentStructure.Refresh();
        string json = "{\n";
        json += "\"Structure\":[\n";

        int i = 0;
        List<Part> parts = structureGO.GetComponentsInChildren<Part>().ToList();
        foreach (var part in parts)
        {
            if (i != 0)
                json += ",\n";

            json += "{\n";
            if (part.type == BlockType.Default || part.type == BlockType.Gyroscope || part.type == BlockType.Count)
                json += part.ToJson();
            if (part.type == BlockType.Thruster)
            {
                json += ((Thruster)part).ToJson();
            }
            json += "}";
            i++;
        }

        json += "],\n";
        json += $"\"ProportionalGain\":{currentStructure.proportionalGain},\n";
        json += $"\"IntegralGain\":{currentStructure.intergralGain},\n";
        json += $"\"DerivativeGain\":{currentStructure.derivativeGain}\n";
        json += "}\n";

        string path = Application.streamingAssetsPath + $"/{shipName}.json";
        File.WriteAllText(path, json.ToString());
        shipFilePaths = Directory.EnumerateFiles(Application.streamingAssetsPath).ToList();
        return json;
    }

    public static void LoadStructure(GameObject structure, List<PartData> parts)
    {
        string path = Application.streamingAssetsPath + shipToLoadPath;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            if (json.Length < 1) return;

            var p = JSON.Parse(json);
            var size = p["Structure"].Count;

            var structObj = currentStructure;

            for (int i = 0; i < size; i++)
            {
                var s = p["Structure"][i];
                var type = (BlockType)s["BlockType"].AsInt;

                GameObject obj = Instantiate(parts[(int)type].partPrefab, structObj.transform, true);
                if (type == BlockType.Default || type == BlockType.Count || type == BlockType.Gyroscope)
                {
                    obj.GetComponent<Part>().FromJson(s.ToString());
                }
                else if (type == BlockType.Thruster)
                {
                    obj.GetComponent<Thruster>().FromJson(s.ToString());
                }
            }
        }
    }

    #region Input
    public void OnEnable()
    {
        if (controls == null)
        {
            controls = new ControlMap();
            controls.BuildSystem.SetCallbacks(this);
        }

        controls.BuildSystem.Enable();
    }
    public void OnDisable()
    {
        controls.BuildSystem.Disable();
    }
    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.action.IsPressed() && hitPart)
        {
            hitPart.AttachPart(parts[(int)currentType].partPrefab, hitPart.GetConnector(hit.normal), hit.normal);
        }
    }
    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.action.IsPressed() && hitPart)
        {
            hitPart.RemovePart();
        }
    }
    public void OnSaveStructure(InputAction.CallbackContext context)
    {
        if (currentStructure && currentStructure.play) return;

        if (context.action.IsPressed())
        {
            SaveStructure("Ship");
        }
    }
    public void OnLoadStructure(InputAction.CallbackContext context)
    {
        if (context.action.IsPressed())
            LoadStructure(InitializeStructure(), parts);
    }
    public void OnDeleteStructure(InputAction.CallbackContext context)
    {
        if (currentStructure && currentStructure.play) return;

        if (context.action.IsPressed())
        {
            Destroy(structureGO);
        }
    }
    public void OnInitNewStructure(InputAction.CallbackContext context)
    {
        if (context.action.IsPressed())
        {
            structureGO = InitializeStructure();
            Instantiate(parts[(int)BlockType.Default].partPrefab, structureGO.transform, false);
        }
    }
    #endregion
}

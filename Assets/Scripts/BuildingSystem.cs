using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using SimpleJSON;
using static ControlMap;

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
    private List<PartData> m_parts = new List<PartData>();
    [SerializeField]
    private LayerMask raycastMask;

    private RaycastHit hit;
    private GameObject partPreview;
    private BlockType currentType = BlockType.Default;
    private GameObject structure;

    private ControlMap controls;
    private Part hitPart = null;

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

    void Start()
    {
        partPreview = Instantiate(m_parts[(int)currentType].partPreviewPrefab);
        partPreview.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Destroy(partPreview);
            currentType = BlockType.Default;
            partPreview = Instantiate(m_parts[(int)currentType].partPreviewPrefab);
            partPreview.SetActive(false);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            Destroy(partPreview);
            currentType = BlockType.Thruster;
            partPreview = Instantiate(m_parts[(int)currentType].partPreviewPrefab);
            partPreview.SetActive(false);
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            Destroy(partPreview);
            currentType = BlockType.Gyroscope;
            partPreview = Instantiate(m_parts[(int)currentType].partPreviewPrefab);
            partPreview.SetActive(false);
        }

        if (structure && !structure.GetComponent<Structure>().play)
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

    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.action.IsPressed() && hitPart)
        {
            hitPart.AttachPart(m_parts[(int)currentType].partPrefab, hitPart.GetConnector(hit.normal), hit.normal);
        }
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.action.IsPressed() && hitPart)
        {
            hitPart.RemovePart();
        }
    }

    public void OnLoadStructure(InputAction.CallbackContext context)
    {
        if (context.action.IsPressed())
            LoadStructure();
    }

    public void OnInitNewStructure(InputAction.CallbackContext context)
    {
        if (context.action.IsPressed())
        {
            structure = InitializeStructure();
            Instantiate(m_parts[(int)BlockType.Default].partPrefab, structure.transform, false);
        }
    }

    public GameObject InitializeStructure()
    {
        if (structure != null)
        {
            Destroy(structure);
        }

        return Instantiate(structurePrefab, null);
    }

    public void LoadStructure()
    {
        string path = Application.streamingAssetsPath + "/Ship.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            if (json.Length < 1) return;

            var p = JSON.Parse(json);
            var size = p["Structure"].Count;
            if(structure != null)
            {
                Destroy(structure);
            }
            structure = InitializeStructure();

            //Might wanna end up making a list of PID controllers
            var structureComp = structure.GetComponent<Structure>();
            structureComp.pidX.FromJson(p[structureComp.pidX.name].ToString());
            structureComp.pidY.FromJson(p[structureComp.pidY.name].ToString());
            structureComp.pidZ.FromJson(p[structureComp.pidZ.name].ToString());

            for (int i = 0; i < size; i++)
            {
                var s = p["Structure"][i];
                var type = (BlockType)s["BlockType"].AsInt;

                GameObject obj = Instantiate(m_parts[(int)type].partPrefab, structure.transform, true);
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
}

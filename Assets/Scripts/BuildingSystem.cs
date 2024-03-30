using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using SimpleJSON;
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

public class BuildingSystem : MonoBehaviour
{
    [SerializeField]
    private List<PartData> m_parts = new List<PartData>();
    [SerializeField]
    private LayerMask raycastMask;

    private RaycastHit hit;
    private GameObject partPreview;
    private BlockType currentType = BlockType.Default;

    void Start()
    {
        partPreview = Instantiate(m_parts[(int)currentType].partPreviewPrefab);
        partPreview.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            LoadStructure();
        }

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            GameObject structure = InitializeStructure();
            Instantiate(m_parts[(int)BlockType.Default].partPrefab, structure.transform, false);
        }

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

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        partPreview.SetActive(false);
        if (Physics.Raycast(ray, out hit, 100, raycastMask))
        {
            Part part = hit.collider.transform.GetComponentInParent<Part>();
            //if (hit.transform.parent.parent.TryGetComponent<Part>(out part))
            if (part)
            {
                var connectorPos = part.GetConnector(hit.normal);
                partPreview.SetActive(true);
                partPreview.transform.position = connectorPos;
                partPreview.transform.up = -hit.normal;
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    part.AttachPart(m_parts[(int)currentType].partPrefab, connectorPos, hit.normal);
                }

                if (Mouse.current.rightButton.wasReleasedThisFrame)
                {
                    part.RemovePart();
                }
            }
        }

    }

    public GameObject InitializeStructure()
    {
        GameObject structure = new GameObject("Structure");
        structure.AddComponent<Structure>();
        structure.layer = LayerMask.NameToLayer("Ignore Raycast");
        var rb = structure.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.drag = 0.0f;
        rb.useGravity = false;
        return structure;
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
            GameObject structure = InitializeStructure();

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

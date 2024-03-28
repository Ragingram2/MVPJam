using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public enum BlockType
{
    Default = 0,
    Thruster = 1,
    Count = 2
}

[System.Serializable]
public class PartData
{
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

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        partPreview.SetActive(false);
        if (Physics.Raycast(ray, out hit, 100, raycastMask))
        {
            Part part;
            if (hit.collider.transform.TryGetComponent<Part>(out part))
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
}

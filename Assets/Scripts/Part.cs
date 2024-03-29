using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Part : MonoBehaviour
{
    private List<Part> connectedParts = new List<Part>();
    public BlockType type;

    private void Start()
    {
        //FindNearbyParts();
    }

    //public void FindNearbyParts()
    //{
    //    findPart(transform.right);
    //    findPart(transform.up);
    //    findPart(transform.forward);
    //    findPart(-transform.right);
    //    findPart(-transform.up);
    //    findPart(-transform.forward);
    //}

    //private void findPart(Vector3 direction)
    //{
    //    RaycastHit hitInfo;
    //    if (Physics.BoxCast(transform.position, new Vector3(.5f, .5f, .5f), direction, out hitInfo))
    //    {
    //        Part p;
    //        if (hitInfo.transform.TryGetComponent<Part>(out p))
    //        {
    //            if (!connectedParts.Contains(p))
    //            {
    //                connectedParts.Add(p);
    //            }
    //        }
    //    }
    //}

    public Vector3 GetConnector(Vector3 normal)
    {
        return transform.position + normal;
    }

    public void AttachPart(GameObject prefab, Vector3 position, Vector3 faceNormal)
    {
        if (transform.parent == null)
        {
            transform.parent = new GameObject("Structure").transform;
            transform.parent.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            transform.parent.AddComponent<Structure>();
            var rb = transform.parent.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.drag = 1.0f;
        }

        var part = Instantiate(prefab, position, Quaternion.identity, transform.parent).GetComponent<Part>();
        part.transform.up = -faceNormal;
    }

    public void RemoveAttachedPart(Part p)
    {
        if (connectedParts.Contains(p))
        {
            connectedParts.Remove(p);
            p.RemovePart();
        }
    }

    public void RemovePart()
    {
        Destroy(gameObject);
    }
}

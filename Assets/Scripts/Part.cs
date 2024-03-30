using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using SimpleJSON;

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

    public virtual string ToJson()
    {
        string json = "";
        json += $"\"Position\":{JsonUtility.ToJson(transform.localPosition)},\n";
        json += $"\"Rotation\":{JsonUtility.ToJson(transform.localRotation)}, \n";
        json += $"\"BlockType\":{(int)type}\n";
        return json;
    }

    public virtual void FromJson(string json)
    {
        var part = JSON.Parse(json);
        float x = part["Position"]["x"];
        float y = part["Position"]["y"];
        float z = part["Position"]["z"];
        transform.localPosition = new Vector3(x, y, z);

        x = part["Rotation"]["x"];
        y = part["Rotation"]["y"];
        z = part["Rotation"]["z"];
        float w = part["Rotation"]["w"];
        transform.localRotation = new Quaternion(x, y, z, w);
        type = (BlockType)part["BlockType"].AsInt;
    }
}

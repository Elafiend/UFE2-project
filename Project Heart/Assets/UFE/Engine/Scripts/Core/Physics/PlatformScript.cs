using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFENetcode;
using FPLibrary;

public class PlatformScript : UFEBehaviour, UFEInterface
{
    public bool canClimbUp;
    public bool canClimbDown;

    [HideInInspector] public FPRect rect;

    Rect boundsRect;

    public override void UFEFixedUpdate()
    {
        if (!isActiveAndEnabled)
            return;
    }
    Bounds GetMaxBounds(GameObject g)
    {
        var b = new Bounds(g.transform.position, Vector3.zero);
        foreach (Renderer r in g.GetComponentsInChildren<Renderer>())
        {
            b.Encapsulate(r.bounds);
        }
        return b;
    }

    public void RefreshData()
    {
        Bounds bounds = GetMaxBounds(gameObject);

        boundsRect = new Rect(bounds.min.x, 
            bounds.min.y,
            bounds.max.x - bounds.min.x,
            bounds.max.y - bounds.min.y);

        rect = new FPRect(boundsRect);

    }
    private void GizmosDrawRectangle(Vector3 topLeft, Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight)
    {
        Gizmos.DrawLine(topLeft, bottomLeft);
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
    }

    void OnDrawGizmos()
    {
        Rect iRect = rect.ToRect();
        Vector3 topLeft = new Vector3(iRect.x, iRect.y, transform.position.z);
        Vector3 topRight = new Vector3(iRect.xMax, iRect.y, transform.position.z);
        Vector3 bottomLeft = new Vector3(iRect.x, iRect.yMax, transform.position.z);
        Vector3 bottomRight = new Vector3(iRect.xMax, iRect.yMax, transform.position.z);

        GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);
    }
}

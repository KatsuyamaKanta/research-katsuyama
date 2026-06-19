using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxChouten
{

    Vector3[] vertices = new Vector3[8];
    Plane[] surfaces = new Plane[6];
    float[] distance = new float[6];
    GameObject ControlledObject;

    public BoxChouten(Collider box, GameObject controlledObject)
    {
        setBoxChouten(box);
        setControlledObject(controlledObject);
    }

    public void setBoxChouten(Collider box)
    {
        BoxCollider boxCollider = box.GetComponent<BoxCollider>();
        var trans = boxCollider.gameObject.transform;
        var min = boxCollider.center - boxCollider.size * 0.5f;
        var max = boxCollider.center + boxCollider.size * 0.5f;
        vertices[0] = trans.TransformPoint(new Vector3(min.x, min.y, min.z));//migi oku shita
        vertices[1] = trans.TransformPoint(new Vector3(min.x, min.y, max.z));//migi temae shita
        vertices[2] = trans.TransformPoint(new Vector3(min.x, max.y, min.z));//migi oku ue
        vertices[3] = trans.TransformPoint(new Vector3(min.x, max.y, max.z));// migi temae ue
        vertices[4] = trans.TransformPoint(new Vector3(max.x, min.y, min.z));//hidari oku shita
        vertices[5] = trans.TransformPoint(new Vector3(max.x, min.y, max.z));//hidari temae shita
        vertices[6] = trans.TransformPoint(new Vector3(max.x, max.y, min.z));//hidari oku ue
        vertices[7] = trans.TransformPoint(new Vector3(max.x, max.y, max.z));//hidari temae ue

        surfaces[0].Set3Points(vertices[2], vertices[3], vertices[6]);
        surfaces[0].Flip();
        surfaces[1].Set3Points(vertices[0], vertices[2], vertices[3]);
        surfaces[2].Set3Points(vertices[0], vertices[2], vertices[6]);
        surfaces[2].Flip();
        surfaces[3].Set3Points(vertices[1], vertices[3], vertices[7]);
        surfaces[4].Set3Points(vertices[4], vertices[5], vertices[7]);
        surfaces[5].Set3Points(vertices[0], vertices[1], vertices[5]);
    }

    public void setControlledObject(GameObject obj)
    {
        ControlledObject = obj;
        calculateDistance();
    }

    public void log()
    {
    }

    private Plane getClosestSurface()
    {
        Plane plane = surfaces[0];
        float distancei = distance[0];

        for (int i = 1; i < 6; i++)
        {
            if (Math.Abs(distance[i]) < Math.Abs(distancei))
            {
                plane = surfaces[i];
                distancei = distance[i];
            }
        }
        return plane;
    }

    private void calculateDistance()
    {
        for (int i = 0; i < 6; i++)
        {
            distance[i] = surfaces[i].GetDistanceToPoint(ControlledObject.transform.position);
        }
    }

    public Vector3 getHousenOfClosestSurface()
    {
        return getClosestSurface().normal;
    }

    public Vector3 getClosestPoint(GameObject sphere)
    {
        return getClosestSurface().ClosestPointOnPlane(sphere.transform.position);

    }

}

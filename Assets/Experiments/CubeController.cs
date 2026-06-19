using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour
{
    private float x,y,z;
    GameObject camera;
    Vector3 newVec;
    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            x += 0.1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            x -= 0.1f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            z += 0.1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            z -= 0.1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            y += 0.1f;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            y -= 0.1f;
        }


        if (Input.GetKey(KeyCode.Space))
        {
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;
        }

        Vector3 organic = new Vector3(x, y, z);
        Vector3 pos = transform.position;
        float radianx = Mathf.Deg2Rad* camera.transform.rotation.eulerAngles.x;
        float radiany = Mathf.Deg2Rad * camera.transform.rotation.eulerAngles.y;
        float radianz = Mathf.Deg2Rad * camera.transform.rotation.eulerAngles.z;

        

        newVec = kaitenXjiku(new Vector3(x,y,z),radianx);
        newVec = kaitenYjiku(newVec, radiany);
        newVec = kaitenZjiku(newVec, radianz);

        newVec = kaitenXjiku(newVec, -radianx);
        newVec = kaitenYjiku(newVec, -radiany);
        newVec = kaitenZjiku(newVec, -radianz);

        Debug.Log(organic);
        //Debug.Log(sin(theta));
        //Debug.Log(Mathf.Clamp((float)Math.Sin(180f * Mathf.Deg2Rad),-1,1));

        //transform.SetPositionAndRotation((camera.transform.InverseTransformVector(organic)), transform.rotation);
        transform.SetPositionAndRotation(newVec, transform.rotation);

    }

    private Vector3 kaitenYjiku(Vector3 zahyou, float radian)
    {
        return new Vector3(zahyou.x * cos(radian) + zahyou.z * (sin(radian)), zahyou.y, -zahyou.x * (sin(radian)) + zahyou.z * cos(radian));
    }

    private Vector3 kaitenXjiku(Vector3 zahyou, float radian)
    {
        return new Vector3(zahyou.x    ,       zahyou.y*cos(radian)-zahyou.z*sin(radian)     ,  zahyou.y*sin(radian)+zahyou.z*cos(radian)    );
    }

    private Vector3 kaitenZjiku(Vector3 zahyou, float radian)
    {
        return new Vector3( zahyou.x*cos(radian) - zahyou.y*sin(radian)    ,  zahyou.x*sin(radian) + zahyou.y * cos(radian)     ,       zahyou.z);
    }

    private float sin(float theta)
    {
        return ((float)Math.Round(Math.Sin(theta) * 100.0f)) * 0.01f;
    }

    private float cos(float theta)
    {
        return (((float)Math.Cos(theta) * 100.0f)) * 0.01f;
    }
}

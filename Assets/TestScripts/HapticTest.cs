using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HapticTest : MonoBehaviour
{
    public HapticPlugin HPPlugin = null;

    // Update is called once per frame
    void Update()
    {
        var a = HPPlugin.CurrentPosition;
        Debug.Log(a);
    }
}

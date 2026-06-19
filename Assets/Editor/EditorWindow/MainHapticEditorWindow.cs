using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.Runtime.InteropServices;

public class MainHapticEditorWindow : EditorWindow
{
    public string DeviceIdentifier = "Default Device";
    CustomHapticEditor custom;
    private GameObject startingPosition;
    private GameObject positionZero;

    private string startingPositionName = "startingPositionHapticEditorObject";


    [MenuItem("Window/CustomEditor/MainHaptic")]
    public static void ShowWindow()
    {
        GetWindow<MainHapticEditorWindow>("MainHaptic");
        
    }

    public void OnEnable()
    {
        startingPosition = GameObject.Find(startingPositionName);
        if (!startingPosition)
        {
            startingPosition = new GameObject();
            startingPosition.name = startingPositionName;
        }
        custom = new CustomHapticEditor(startingPosition,positionZero);
    }

    public void OnDestroy()
    {
        DestroyImmediate(startingPosition);
    }

    public void Update()
    {
        custom.Update();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("space pressed");
        }

    }

    void OnSelectionChange()
    {
        Debug.Log("OnSelectionChange");
        custom.VisualizationMeshPub = Selection.activeGameObject;
        custom.CollisionMeshPub = Selection.activeGameObject;
        custom.setTarget(Selection.activeGameObject);
        Repaint();
    }

    private void OnGUI()
    {

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField(custom.VisualizationMeshPub, typeof(GameObject), true);
        EditorGUILayout.ObjectField(custom.CollisionMeshPub, typeof(GameObject), true);
        EditorGUI.EndDisabledGroup();
        if (GUILayout.Button("SetUP"))
        {
            custom.setUp();
        }
        if (GUILayout.Button("Disconnect"))
        {
            CustomHapticEditor.disconnectAllDevices();
        }

        var e = Event.current;
        if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
        {
            custom.refresh();
        }



    }



}

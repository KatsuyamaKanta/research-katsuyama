using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class MainHapticEditorWindow : EditorWindow
{
    public string DeviceIdentifier = "Default Device";
    CustomHapticEditor custom;
    private GameObject startingPosition;
    private GameObject positionZero;

    private string startingPositionName = "startingPositionHapticEditorObject";

    // デバッグログ用
    private List<string> debugLogs = new List<string>();
    private Vector2 scrollPos;

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
        custom = new CustomHapticEditor(startingPosition, positionZero);
        EditorApplication.update += Update;

        DebugToEditor("エディタが有効になりました");
    }

    public void OnDisable()
    {
        EditorApplication.update -= Update;
        DebugToEditor("エディタが無効になりました");
    }

    public void OnDestroy()
    {
        DestroyImmediate(startingPosition);
        DebugToEditor("開始位置オブジェクトを削除しました");
    }

    public void Update()
    {
        custom.Update();
        Repaint();
    }

    void OnSelectionChange()
    {
        DebugToEditor("選択オブジェクトが変更されました");

        custom.StopHapticControl();

        custom.VisualizationMeshPub = Selection.activeGameObject;
        custom.CollisionMeshPub = Selection.activeGameObject;
        custom.setTarget(Selection.activeGameObject);

        DebugToEditor("選択変更によりTouch操作を停止しました");
        Repaint();
    }

    private void OnGUI()
    {
        GUILayout.Label("1. Hierarchyから操作したいオブジェクトを選択してください", EditorStyles.boldLabel);

        EditorGUILayout.ObjectField("操作対象", custom.VisualizationMeshPub, typeof(GameObject), true);

        GUILayout.Space(10);
        GUILayout.Label("2. Touch操作", EditorStyles.boldLabel);

        GUIStyle largeButtonStyle = new GUIStyle(GUI.skin.button);
        largeButtonStyle.fontSize = 15;
        largeButtonStyle.fixedHeight = 48;

        GUILayout.Label("現在の操作: " + custom.GetControlModeName(), EditorStyles.helpBox);

        if (GUILayout.Button("Touchで移動開始", largeButtonStyle))
        {
            custom.StartHapticControl();
            DebugToEditor("Touch移動操作を開始しました");
        }

        GUILayout.Space(6);

        if (GUILayout.Button("Touchで拡大縮小開始", largeButtonStyle))
        {
            custom.StartScaleControl();
            DebugToEditor("Touch拡大縮小操作を開始しました");
        }

        GUILayout.Space(6);

        if (GUILayout.Button("Touchで変形開始", largeButtonStyle))
        {
            custom.StartDeformControl();
            DebugToEditor("Touch変形操作を開始しました");
        }

        GUILayout.Space(6);

        if (GUILayout.Button("Touch操作停止", largeButtonStyle))
        {
            custom.StopHapticControl();
            DebugToEditor("Touch操作を停止しました");
        }

        // デバイス名表示
        GUILayout.Space(5);
        GUILayout.Label($"現在のデバイス: {DeviceIdentifier}", EditorStyles.helpBox);


        GUILayout.Space(15);
        GUILayout.Label("詳細操作", EditorStyles.boldLabel);

        if (GUILayout.Button("デバイス切断", GUILayout.Height(24)))
        {
            custom.DisconnectDevice();
            DebugToEditor("デバイスを切断しました");
        }

        // デバッグログ欄
        GUILayout.Space(10);
        GUILayout.Label("■ デバッグログ", EditorStyles.boldLabel);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(100));
        foreach (string log in debugLogs)
        {
            GUILayout.Label(log);
        }
        EditorGUILayout.EndScrollView();
    }

    // デバッグログ追加メソッド
    private void DebugToEditor(string message)
    {
        if (debugLogs.Count > 100)
        {
            debugLogs.RemoveAt(0); // 古いログ削除
        }

        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string logEntry = $"[{timestamp}] {message}";
        debugLogs.Add(logEntry);

        Debug.Log(logEntry); // UnityのConsoleにも出力
    }
}

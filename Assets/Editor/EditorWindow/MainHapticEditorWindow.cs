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
        custom.VisualizationMeshPub = Selection.activeGameObject;
        custom.CollisionMeshPub = Selection.activeGameObject;
        custom.setTarget(Selection.activeGameObject);
        Repaint();
    }

    private void OnGUI()
    {
        GUILayout.Label("操作したいオブジェクトをクリックしてSetUPボタンを押してください", EditorStyles.boldLabel);
        GUILayout.Space(5);
        if (GUILayout.Button("SetUP"))
        {
            custom.setUp();
            DebugToEditor("SetUPボタンが押されました");
        }
           
        EditorGUILayout.ObjectField(custom.VisualizationMeshPub, typeof(GameObject), true);
        EditorGUILayout.ObjectField(custom.CollisionMeshPub, typeof(GameObject), true);

        GUILayout.Space(5);
        GUILayout.Label("接続解除する際はDisconnectボタンを押してください", EditorStyles.boldLabel);
        GUILayout.Space(5);
        if (GUILayout.Button("Disconnect"))
        {
            CustomHapticEditor.disconnectAllDevices();
            DebugToEditor("Disconnectボタンが押されました");
        }

        // デバイス名表示
        GUILayout.Space(5);
        GUILayout.Label($"現在のデバイス: {DeviceIdentifier}", EditorStyles.helpBox);

        // スペースキー処理
        var e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
        {
            custom.refresh();
            DebugToEditor("Spaceキーが押されました（refresh）");
        }

        // 座標リアルタイム表示
        GUILayout.Space(10);
        GUILayout.Label("■ 現在のターゲット座標（リアルタイム）", EditorStyles.boldLabel);
        GameObject target = Selection.activeGameObject;
        if (target != null)
        {
            Vector3 pos = target.transform.position;
            GUILayout.Label($"World Position - X: {pos.x:F3} Y: {pos.y:F3} Z: {pos.z:F3}");

            if (GUILayout.Button("座標をコピー"))
            {
                EditorGUIUtility.systemCopyBuffer = pos.ToString("F3");
                DebugToEditor($"座標をコピーしました: {pos.ToString("F3")}");
            }
        }
        else
        {
            GUILayout.Label("オブジェクトが選択されていません。");
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

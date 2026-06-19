using UnityEngine;
using UnityEditor;

public class TryoutWindow : EditorWindow
{
    Color color;

    public GameObject hapticPlugin = null;

    [MenuItem("Window/CustomEditor/Tryout")]
    public static void ShowWindow()
    {
        GetWindow<TryoutWindow>("Colorizer");
    }

    private void Awake()
    {
        Debug.Log("awake");
    }


    private void Update()
    {
        Debug.Log("testing update");
        try
        {
            hapticPlugin.GetComponent<HapticPlugin>().FixedUpdate();
        }
        catch (UnassignedReferenceException e)
        {
            Debug.Log(hapticPlugin.GetComponent<HapticPlugin>().CurrentPosition);
        }
        Debug.Log(hapticPlugin.GetComponent<HapticPlugin>().CurrentPosition);


    }

    void OnGUI()
    {
        GUILayout.Label("Color the selected objects!", EditorStyles.boldLabel);

        color = EditorGUILayout.ColorField("Color", color);

        hapticPlugin = (GameObject)EditorGUILayout.ObjectField(hapticPlugin, typeof(GameObject), true);

        if (GUILayout.Button("COLORIZE"))
        {
            //foreach (GameObject obj in Selection.gameObjects)
            //{
            //    Debug.Log(obj.name);
            //    Renderer renderer = obj.GetComponent<Renderer>();
            //    if(renderer != null)
            //    {
            //        renderer.material.color = color;
            //    }
            //}
            hapticPlugin.GetComponent<HapticPlugin>().OnEnable();
            hapticPlugin.GetComponent<HapticPlugin>().Start();


        }
    }

    //void OnSceneGUI()
    //{
    //Event e = Event.current;
    //Debug.Log("pressead");

    //if (e.type == EventType.KeyDown)
    //{
    //    Debug.Log("pressed1");

    //    if (Event.current.keyCode == KeyCode.LeftArrow)
    //    {
    //        Debug.Log("pressed");
    //        Selection.activeGameObject.transform.Translate(1.0f, 0.0f, 0.0f);
    //    }
    //}
    //}

    void OnSceneGUI()
    {
        Debug.Log("pressed");

        Event e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
                {
                    if (Event.current.keyCode == (KeyCode.A))
                    {
                        Debug.Log("pressed");

                    }
                    break;
                }
        }
    }

}

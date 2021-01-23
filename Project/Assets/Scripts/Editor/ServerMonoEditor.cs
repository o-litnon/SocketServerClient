using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ServerMono))]
public class ServerMonoEditor : Editor
{
    private ServerMono driver;
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        driver = (ServerMono)target;

        string message = "Hello, World!";
        message = EditorGUILayout.TextField("Test message:", message);

        if (!string.IsNullOrEmpty(message) && GUILayout.Button("Send"))
            driver.SendTest(message);

        base.OnInspectorGUI();
    }
}

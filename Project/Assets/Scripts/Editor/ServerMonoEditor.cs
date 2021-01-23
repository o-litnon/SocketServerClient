using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ServerMono))]
public class ServerMonoEditor : Editor
{
    private ServerMono driver;
    private string message = "Hello, World!";
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        driver = (ServerMono)target;

        var count = driver.Server != null ? driver.Server.ConnectedChannels.OpenChannels.Count : 0;
        GUILayout.Label($"Has {count} connected clients");

        message = EditorGUILayout.TextField("Test message:", message);

        if (!string.IsNullOrEmpty(message) && GUILayout.Button("Send"))
            driver.SendTest(message);

        base.OnInspectorGUI();
    }
}

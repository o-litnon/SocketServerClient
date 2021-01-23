using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ClientMono))]
public class ClientMonoEditor : Editor
{
    private ClientMono driver;
    private string message = "Hello, World!";
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        driver = (ClientMono)target;

        GUILayout.Label($"Is Connected: {driver.Socket?.Running}");
        message = EditorGUILayout.TextField("Test message:", message);

        if (!string.IsNullOrEmpty(message))
        {
            if(GUILayout.Button("Send TCP"))
                driver.SendTest(message, NetSockets.ConnectionType.TCP);
            if (GUILayout.Button("Send UDP"))
                driver.SendTest(message, NetSockets.ConnectionType.TCP);
        }

        if (GUILayout.Button("Connect"))
            driver.Connect();
        if (GUILayout.Button("Disconnect"))
            driver.Disconnect();

        base.OnInspectorGUI();
    }
}

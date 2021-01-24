using NetSockets;
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

        if (!string.IsNullOrEmpty(message))
        {
            using (var packet = new Packet())
            {
                if (GUILayout.Button("Send TCP"))
                {
                    packet.Write(message + "using TCP");
                    driver.Server.SendAll(packet, ConnectionType.TCP);
                }
                if (GUILayout.Button("Send UDP"))
                {
                    packet.Write(message + "using UDP");
                    driver.Server.SendAll(packet, ConnectionType.UDP);
                }
            }
        }

        base.OnInspectorGUI();
    }
}

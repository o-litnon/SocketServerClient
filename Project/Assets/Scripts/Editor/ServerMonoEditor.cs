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

        if (driver.Server != null)
        {
            GUILayout.Label($"Has {driver.Server.ConnectedChannels.OpenChannels.Count} connected clients");
            GUILayout.Label($"Has {driver.Server.ConnectedChannels.ActiveChannels.Count}/{driver.Server.ConnectedChannels.MaxPlayers} active clients");
            GUILayout.Label($"Has {driver.Server.ConnectedChannels.PendingChannels.Count} pending clients");

            message = EditorGUILayout.TextField("Test message:", message);

            if (!string.IsNullOrEmpty(message))
            {
                using (var packet = new Packet())
                {
                    if (GUILayout.Button("Send TCP"))
                    {
                        packet.Write(message + " [using TCP]");
                        driver.Server.SendAll(packet, ConnectionType.TCP);
                    }
                    if (GUILayout.Button("Send UDP"))
                    {
                        packet.Write(message + " [using UDP]");
                        driver.Server.SendAll(packet, ConnectionType.UDP);
                    }
                }
            }
        }

        base.OnInspectorGUI();
    }
}

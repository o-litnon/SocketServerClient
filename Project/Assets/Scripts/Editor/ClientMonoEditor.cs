using NetSockets;
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

        GUILayout.Label($"Id: {driver.Socket?.Id}");
        GUILayout.Label($"Is Connected: {driver.Socket?.Running}");

        message = EditorGUILayout.TextField("Test message:", message);

        if (!string.IsNullOrEmpty(message))
        {
            using (var packet = new Packet())
            {
                if (GUILayout.Button("Send TCP"))
                {
                    packet.Write(message + " using TCP");
                    driver.Socket.Send(packet.ToArray(), ConnectionType.TCP);
                }

                if (GUILayout.Button("Send UDP"))
                {
                    packet.Write(message + " using UDP");
                    driver.Socket.Send(packet.ToArray(), ConnectionType.UDP);
                }
            }
        }

        if (GUILayout.Button("Connect"))
            driver.Socket.Open();
        if (GUILayout.Button("Disconnect"))
            driver.Socket.Close();

        base.OnInspectorGUI();
    }
}

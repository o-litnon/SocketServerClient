using System;
using UnityEngine;

namespace NetSockets
{
    public static class Debugging
    {
        public static bool isEditor => Application.platform.Equals(RuntimePlatform.WindowsEditor) || Application.platform.Equals(RuntimePlatform.LinuxEditor) || Application.platform.Equals(RuntimePlatform.OSXEditor);
        
        public static void Log(object item)
        {
            if (isEditor)
                Debug.Log(item);
            else
                Console.WriteLine(item);
        }

        public static void LogWarning(object item)
        {
            if (isEditor)
                Debug.LogWarning(item);
            else
                Console.WriteLine($"WARNING: {item}");
        }

        public static void LogError(object item)
        {
            if (isEditor)
                Debug.LogError(item);
            else
                Console.WriteLine($"ERROR: {item}");
        }
    }
}

using System;
using System.Collections.Generic;

namespace NetSockets
{
    public class ThreadManager
    {
        private static readonly List<Action> executeOnMainThread = new List<Action>();
        private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();

        public static void ExecuteOnMainThread(Action _action)
        {
            if (_action != null)
                lock (executeOnMainThread)
                    executeOnMainThread.Add(_action);
        }

        /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
        public static void UpdateMain()
        {
            executeCopiedOnMainThread.Clear();
            lock (executeOnMainThread)
            {
                executeCopiedOnMainThread.AddRange(executeOnMainThread);
                executeOnMainThread.Clear();
            }

            for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
                executeCopiedOnMainThread[i]();
        }
    }
}
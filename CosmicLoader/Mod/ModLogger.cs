using System;
using UnityEngine;

namespace CosmicLoader
{
    public class ModLogger
    {
        public ModLogger(string prefix)
        {
            LogPrefix = prefix;
        }

        public string LogPrefix { get; set; }

        public T Log<T>(T obj)
        {
            //Console.WriteLine($"[{LogPrefix}] {obj}");
            Debug.Log($"[{LogPrefix}] {obj}");
            ModManager.Config.Logs.Add($"[{LogPrefix}] {obj}");
            return obj;
        }

        public T LogWarning<T>(T obj)
        {
            //Console.WriteLine($"[{LogPrefix}] [Warning] {obj}");
            Debug.Log($"[{LogPrefix}] [Warning] {obj}");
            ModManager.Config.Logs.Add($"[{LogPrefix}] [Warning] {obj}");
            return obj;
        }

        public T LogError<T>(T obj)
        {
            //Console.WriteLine($"[{LogPrefix}] [Error] {obj}");
            Debug.Log($"[{LogPrefix}] [Error] {obj}");
            ModManager.Config.Logs.Add($"[{LogPrefix}] [Error] {obj}");
            return obj;
        }

        public T LogException<T>(string message, T ex) where T : Exception
        {
            //Console.WriteLine($"[{LogPrefix}] [Exception] {message}: {ex}");
            Debug.Log($"[{LogPrefix}] [Exception] {message}: {ex}");
            ModManager.Config.Logs.Add($"[{LogPrefix}] [Exception] {message}: {ex}");
            return ex;
        }
    }
}

using System;
using CosmicLoader.Core;
using UnityEngine;

namespace CosmicLoader.Mod
{
    public class ModLogger
    {
        public string LogFormat = "[{0}] {1}";
        public string LogWarningFormat = "<color=#F5B417>[{0}: Warning] {1}</color>";
        public string LogErrorFormat = "<color=#AF2543>[{0}: Error] {1}</color>";
        public string LogExceptionFormat = "<color=#AF2543>[{0}] {1}: {2}</color>";
        
        public ModLogger(string prefix)
        {
            LogPrefix = prefix;
        }

        public string LogPrefix { get; set; }

        public T Log<T>(T obj)
        {
            LogRaw(string.Format(LogFormat, LogPrefix, obj));
            return obj;
        }

        public T LogWarning<T>(T obj)
        {
            LogRaw(string.Format(LogWarningFormat, LogPrefix, obj));
            return obj;
        }

        public T LogError<T>(T obj)
        {
            LogRaw(string.Format(LogErrorFormat, LogPrefix, obj));
            return obj;
        }

        public T LogException<T>(string message, T ex) where T : Exception
        {
            LogRaw(string.Format(LogExceptionFormat, LogPrefix, message, ex));
            return ex;
        }
        
        public static void LogRaw(string log)
        {
            Debug.Log(RDUtils.RemoveRichTags(log));
            CosmicManager.Config.Logs.Add(log);
        }
    }
}

using System;
using System.Linq;
using System.IO;
using HarmonyLib;
using System.Reflection;

namespace AdofaiModManager
{
    public class Mod
    {
        public ModInfo Info { get; private set; }
        public ModLogger Logger { get; private set; }
        public Harmony Harmony { get; private set; }
        public Assembly Assembly { get; private set; }
        public string ModPath { get; private set; }
        public bool Loaded { get; private set; }
        public bool LoadFailed { get; private set; }
        public Action<bool> OnToggle;
        public Action OnUpdate;
        public Action OnExit;
        private bool active;
        internal Mod(ModInfo mInfo, string dir)
        {
            Info = mInfo;
            Harmony = new Harmony(mInfo.Id, false);
            ModPath = dir;
            Logger = new ModLogger(mInfo.Name);
            var dict = ModManager.Config.States;
            if (dict.TryGetValue(mInfo.Id, out bool state))
                Active = state;
            dict[mInfo.Id] = false;
        }
        public bool Active
        {
            get => active;
            set
            {
                active = value;
                if (active)
                    TryLoad();
                OnToggle?.Invoke(active);
            }
        }
        public bool TryLoad()
        {
            if (Loaded)
            {
                LoadFailed = false;
                return true;
            }
            try { Load(); }
            catch 
            {
                LoadFailed = true;
                return false;
            }
            LoadFailed = false;
            return true;
        }
        void Load()
        {
            var dll = $"{ModPath}/{Info.FileName}";
            var currentResolving = default(Library);
            try
            {
                var refs = Info.References;
                for (int i = 0; i < refs?.Length; i++)
                {
                    currentResolving = refs[i];
                    currentResolving.Resolve(ModPath);
                }
            }
            catch { Logger.LogError($"Library '{currentResolving.FileName}' Failed To Resolve!"); return; }
            if (File.Exists(dll))
            {
                var bytes = File.ReadAllBytes(dll);
                Assembly = AppDomain.CurrentDomain.Load(bytes);
                string[] split = Info.EntryPoint.Split('.');
                if (split.Length <= 0)
                {
                    Logger.LogError($"[{Info.Name}] Invalid EntryPoint! ({Info.EntryPoint})");
                    return;
                }
                string entryType = split.Take(split.Length - 1).Aggregate((c, n) => $"{c}{n}");
                string entryMethod = split.Last();
                Type realEntryType = Assembly.GetType(entryType);
                if (realEntryType == null)
                {
                    Logger.LogError($"[{Info.Name}] Type '{entryType}' Was Not Found!");
                    return;
                }
                MethodInfo realEntryMethod = realEntryType.GetMethod(entryMethod, AccessTools.all);
                if (realEntryMethod == null)
                {
                    Logger.LogError($"[{Info.Name}] Method '{entryMethod}' Was Not Found!");
                    return;
                }
                if (realEntryMethod.ReturnType == typeof(bool))
                {
                    try
                    {
                        var hasArg = realEntryMethod.GetParameters().Any();
                        bool result = (bool)realEntryMethod.Invoke(null, hasArg ? new[] { this } : null);
                        if (!result)
                        {
                            Logger.LogError("Load Failed. (Load Method Returns False)");
                            return;
                        }
                    }
                    catch (Exception e) { Logger.LogError($"Load Failed. {e}"); return; }
                }
                else
                {
                    try
                    {
                        var hasArg = realEntryMethod.GetParameters().Any();
                        realEntryMethod.Invoke(null, hasArg ? new[] { this } : null);
                    }
                    catch (Exception e) { Logger.LogError($"Load Failed. {e}"); return; }
                }
            }
            else
            {
                Logger.LogError($"[{Info.Name}] '{dll}' Was Not Found!");
                return;
            }
            Loaded = true;
        }
    }
}

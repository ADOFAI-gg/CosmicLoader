using System;
using System.Linq;
using System.IO;
using HarmonyLib;
using System.Reflection;
using CosmicLoader.UMM;
using UnityEngine;
using UnityModManagerNet;

namespace CosmicLoader
{
    public class Mod
    {
        public ModInfo Info { get; private set; }
        public ModLogger Logger { get; private set; }
        public Harmony Harmony { get; private set; }
        public Assembly Assembly { get; private set; }
        public string Path { get; private set; }
        public bool Loaded { get; private set; }
        public bool LoadFailed { get; private set; }
        public Action<bool> OnToggle;
        public Action OnUpdate;
        public Action OnExit;
        private bool active;

        internal Mod(ModInfo mInfo)
        {
            Info = mInfo;
            Harmony = new Harmony(mInfo.Id);
            Path = mInfo.Path;
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

            try
            {
                if (Info.IsUMM) LoadUMM();
                else Load();
            }
            catch (Exception e)
            {
                Logger.LogException($"Error while loading mod {Info.Id}", e);
                LoadFailed = true;
                return false;
            }

            LoadFailed = false;
            return true;
        }

        void Load()
        {
            static MethodInfo GetEntryPoint(Assembly assembly, string entry)
            {
                var identifiers = entry.Split('.');
                var methodName = identifiers[identifiers.Length - 1];
                var typeName = string.Join(".", identifiers.Take(identifiers.Length - 1));
                var type = assembly.GetType(typeName);
                return type?.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            }

            var dllPath = System.IO.Path.Combine(Path, Info.FileName);
            if (!File.Exists(dllPath))
            {
                throw new Exception($"Mod {Info.Id} file not found!");
            }

            var assembly = Assembly.LoadFile(dllPath);
            var entryMethod = GetEntryPoint(assembly, Info.EntryPoint);
            if (entryMethod == null)
            {
                throw new Exception($"Mod {Info.Name} entry not found!");
            }

            this.Assembly = entryMethod.DeclaringType!.Assembly;
            var res = entryMethod.Invoke(null, new object[] {this});
            if (res is false) throw new Exception($"Mod {Info.Name} entry method returned false!");
            Loaded = true;
        }

        void LoadUMM()
        {
            static MethodInfo GetEntryPoint(Assembly assembly, string entry)
            {
                var identifiers = entry.Split('.');
                var methodName = identifiers[identifiers.Length - 1];
                var typeName = string.Join(".", identifiers.Take(identifiers.Length - 1));
                var type = assembly.GetType(typeName);
                return type?.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            }

            var dllPath = System.IO.Path.Combine(Path, Info.FileName);
            if (!File.Exists(dllPath))
            {
                throw new Exception($"Mod {Info.Id} file not found!");
            }

            var assembly = Assembly.LoadFile(dllPath);
            var entryMethod = GetEntryPoint(assembly, Info.EntryPoint);
            if (entryMethod == null)
            {
                throw new Exception($"Mod {Info.Name} entry not found!");
            }

            this.Assembly = entryMethod.DeclaringType!.Assembly;
            var entry = Integration.CreateModEntry(this);
            Debug.Log($"Created mod entry {entry} for {Info.Name} assembly {entry.Assembly}");
            var res = entryMethod.Invoke(null, new object[] {entry});
            if (res is false) throw new Exception($"Mod {Info.Name} entry method returned false!");
            _entry = entry;
            Loaded = true;
        }

        public Action OnGUI
        {
            get
            {
                if (Info.IsUMM) return () => _entry.OnGUI(_entry);
                return _onGUI;
            }

            set
            {
                _onGUI = value;
            }
        }

        private UnityModManager.ModEntry _entry;
        private Action _onGUI;
    }
}

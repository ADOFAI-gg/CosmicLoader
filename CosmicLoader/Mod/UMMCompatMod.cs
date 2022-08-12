using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CosmicLoader.UMMCompatibility;
using UnityEngine;
using UnityModManagerNet;

namespace CosmicLoader.Mod
{
    public sealed class UMMCompatMod : ModBase
    {
        public new UMMModInfo Info => (UMMModInfo)base.Info;
        public UnityModManager.ModEntry ModEntry;

        public override ModState State
        {
            get
            {
                if (!ModEntry.Started) return ModState.BeforeLoad;
                if (ModEntry.ErrorOnLoading) return ModState.LoadFailed;
                if (!ModEntry.Enabled) return ModState.Error;
                return ModEntry.Active ? ModState.Active : ModState.Inactive;
            }
            internal set
            {
                switch (value)
                {
                    case ModState.BeforeLoad:
                        ModEntry.Started = false;
                        break;
                    case ModState.Active:
                        if (!ModEntry.Started) Load();
                        
                        ModEntry.Started = true;
                        ModEntry.Enabled = true;
                        ModEntry.Active = true;
                        break;
                    case ModState.Inactive:
                        ModEntry.Enabled = true;
                        ModEntry.Active = false;
                        break;
                    case ModState.Error:
                        ModEntry.Enabled = false;
                        break;
                    case ModState.LoadFailed:
                        ModEntry.ErrorOnLoading = true;
                        break;
                }
            }
        }

        protected internal override void OnToggle(bool active)
        {
            Debug.Log($"[{Info.Id}] {(active ? "Enabled" : "Disabled")}");
            if (!ModEntry.Started) return;
            ModEntry.OnToggle?.Invoke(ModEntry, active);
        }

        protected internal override void OnUpdate()
        {
            ModEntry.OnUpdate?.Invoke(ModEntry, Time.deltaTime);
        }

        protected internal override void OnExit() { }

        protected internal override void OnGUI()
        {
            ModEntry.OnGUI?.Invoke(ModEntry);
        }

        protected internal override void Initialize()
        {
            ModEntry = UMMHelper.CreateModEntry(this);
            Debug.Log($"Created mod entry {ModEntry} for {Info.Name} assembly {ModEntry.Assembly}");
            UMMHelper.RegisterEntry(this, ModEntry);
            base.Initialize();
        }

        protected override void Load(bool active)
        {
            if (active) Load();
        }

        private void Load()
        {
            static MethodInfo GetEntryPoint(Assembly assembly, string entry)
            {
                var identifiers = entry.Split('.');
                var methodName = identifiers[identifiers.Length - 1];
                var typeName = string.Join(".", identifiers.Take(identifiers.Length - 1));
                var type = assembly.GetType(typeName);
                return type?.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            }

            var dllPath = System.IO.Path.Combine(Info.Path, Info.ModInfo.AssemblyName);
            if (!File.Exists(dllPath))
            {
                throw new Exception($"Mod {Info.Id} file not found!");
            }

            var assembly = Assembly.LoadFile(dllPath);
            var entryMethod = GetEntryPoint(assembly, Info.ModInfo.EntryMethod);
            if (entryMethod == null)
            {
                throw new Exception($"Mod {Info.Name} entry not found!");
            }

            Info.Assembly = assembly;
            ModEntry.Assembly = entryMethod.DeclaringType!.Assembly;
            var res = entryMethod.Invoke(null, new object[] {ModEntry});
            if (res is false) throw new Exception($"Mod {Info.Name} entry method returned false!");
        }
    }
}

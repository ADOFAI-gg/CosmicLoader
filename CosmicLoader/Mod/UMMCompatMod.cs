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
    public class UMMCompatMod : ModBase
    {
        public UnityModManager.ModEntry ModEntry;

        public override Assembly Assembly
        {
            get => ModEntry.Assembly;
            protected set => ModEntry.Assembly = value;
        }

        public override bool Loaded
        {
            get => ModEntry.Started;
            protected set => ModEntry.Started = value;
        }

        public override bool LoadFailed
        {
            get => ModEntry.ErrorOnLoading;
            protected set => ModEntry.ErrorOnLoading = value;
        }

        public override Action<bool> OnToggle
        {
            get => a => ModEntry.OnToggle?.Invoke(ModEntry, a);
            set => throw new NotSupportedException();
        }

        public override Action<bool> OnInitialize
        {
            get => _ => { };
            set => throw new NotSupportedException();
        }

        public override Action OnUpdate
        {
            get => () => ModEntry.OnUpdate?.Invoke(ModEntry, Time.deltaTime);
            set => throw new NotSupportedException();
        }

        public override Action OnExit
        {
            get => () => { };
            set => throw new NotSupportedException();
        }

        public override Action OnGUI
        {
            get => () => ModEntry.OnGUI?.Invoke(ModEntry);
            set => throw new NotSupportedException();
        }

        public override void Initialize()
        {
            ModEntry = Integration.CreateModEntry(this);
            Debug.Log($"Created mod entry {ModEntry} for {Info.Name} assembly {ModEntry.Assembly}");
            Integration.RegisterEntry(this, ModEntry);
            base.Initialize();
        }

        protected override void Load()
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
            var res = entryMethod.Invoke(null, new object[] {ModEntry});
            if (res is false) throw new Exception($"Mod {Info.Name} entry method returned false!");
            Loaded = true;
        }

        public UMMCompatMod(ModInfo mInfo) : base(mInfo) { }
    }
}

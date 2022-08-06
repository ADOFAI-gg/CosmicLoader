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
    public class CosmicMod : ModBase
    {
        public override Assembly Assembly { get; protected set; }
        public override bool Loaded { get; protected set; }
        public override bool LoadFailed { get; protected set; }

        public override Action<bool> OnToggle { get; set; }
        public override Action<bool> OnInitialize { get; set; }
        public override Action OnUpdate { get; set; }
        public override Action OnExit { get; set; }
        public override Action OnGUI { get; set; }

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
            var res = entryMethod.Invoke(null, new object[] {this});
            if (res is false) throw new Exception($"Mod {Info.Name} entry method returned false!");
            Loaded = true;
        }

        public CosmicMod(ModInfo mInfo) : base(mInfo) { }
    }
}

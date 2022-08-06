using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace CosmicLoader.UMM
{
    public static class Integration
    {
        private static Dictionary<Mod, UnityModManager.ModEntry> _mods = new Dictionary<Mod, UnityModManager.ModEntry>();
        public static UnityModManager.ModEntry GetMod(this Mod mod) => _mods[mod];

        public static readonly Version LoaderVersion = new Version(0, 23, 4, 0);

        public static ModInfo ParseUMMInfo(JObject info)
        {
            var modInfo = new ModInfo();
            modInfo.Id = info["Id"].ToString();
            modInfo.Name = info["DisplayName"].ToString();
            modInfo.FileName = info["AssemblyName"].ToString();
            modInfo.EntryPoint = info["EntryMethod"].ToString();

            modInfo.Author = info["Author"].ToString();
            modInfo.Version = info["Version"].ToString();
            if (info.TryGetValue("GameVersion", out var gameVersion))
            {
                modInfo.GameVersion = gameVersion.ToString();
            }

            if (info.TryGetValue("LoadAfter", out var loadAfter))
            {
                modInfo.LoadAfter = loadAfter.ToObject<string[]>();
            }

            if (info.TryGetValue("LoadBefore", out var loadBefore))
            {
                modInfo.LoadBefore = loadBefore.ToObject<string[]>();
            }

            modInfo.References = Array.Empty<string>();
            modInfo.IsUMM = true;
            return modInfo;
        }

        public static UnityModManager.ModEntry CreateModEntry(Mod mod)
        {
            var info = new UnityModManager.ModInfo()
            {
                AssemblyName = mod.Info.FileName,
                Author = mod.Info.Author,
                DisplayName = mod.Info.Name,
                EntryMethod = mod.Info.EntryPoint,
                Version = mod.Info.Version,
                GameVersion = mod.Info.GameVersion,
                Id = mod.Info.Id,
                LoadAfter = mod.Info.LoadAfter,
            };
            return new UnityModManager.ModEntry(info, mod.Path, mod);
        }

        public static void IntegrateUMM()
        {
            var logger = new ModLogger("UMM-Compat");
            UnityModManager.Logger.LogAction = m => logger.Log(m);
            UnityModManager.Logger.LogErrorAction = m => logger.LogError(m);
            UnityModManager.Logger.LogExceptionAction = (m, e) => logger.LogException(m, e);
            UnityModManager.ModSettings.SaveAction = (o, s) => ((ModSettings)o).Save(s);
            UnityModManager.ModSettings.LoadAction = s => ModSettings.Load(s);
            UnityModManager.ModEntry.ActiveAction = o => ((Mod)o).Active;
            UnityModManager.ModEntry.AssemblyAction = o => ((Mod)o).Assembly;
            UnityModManager.ModEntry.LoadedAction = o => ((Mod)o).Loaded;
            UnityModManager.ModEntry.LoadFailedAction = o => ((Mod)o).LoadFailed;
            UnityModManager.ModEntry.SetActiveAction = (o, b) => ((Mod)o).Active = b;
        }
    }
}
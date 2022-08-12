using System;
using System.Collections.Generic;
using System.Linq;
using CosmicLoader.Mod;
using Newtonsoft.Json.Linq;
using UnityModManagerNet;

namespace CosmicLoader.UMMCompatibility
{
    public static class UMMHelper
    {
        private static Dictionary<UMMCompatMod, UnityModManager.ModEntry> _mods = new();
        public static UnityModManager.ModEntry GetMod(this UMMCompatMod mod) => _mods[mod];
        public static void RegisterEntry(UMMCompatMod mod, UnityModManager.ModEntry entry) => _mods.Add(mod, entry);
        

        public static readonly Version LoaderVersion = new Version(0, 23, 4, 0);

        public static ModInfo ParseUMMInfo(JObject info)
        {
            const StringComparison ignoreCase = StringComparison.OrdinalIgnoreCase;
            var modInfo = new ModInfo();
            modInfo.FileName = info["AssemblyName"].ToString();
            
            var dict = info.ToObject<Dictionary<string, object>>()!;
            modInfo.Id = dict.First(pair => pair.Key.Equals("Id", ignoreCase)).Value.ToString();
            modInfo.Name = dict.First(pair => pair.Key.Equals("DisplayName", ignoreCase)).Value.ToString();
            modInfo.EntryPoint = dict.First(pair => pair.Key.Equals("EntryMethod", ignoreCase)).Value.ToString();
            modInfo.Author = dict.First(pair => pair.Key.Equals("Author", ignoreCase)).Value.ToString();
            modInfo.Version = dict.First(pair => pair.Key.Equals("Version", ignoreCase)).Value.ToString();

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

        public static UnityModManager.ModEntry CreateModEntry(UMMCompatMod mod)
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
            var logger = new ModLogger(null);
            logger.LogFormat = "{0}{1}";
            logger.LogWarningFormat = "<color=#F5B417>{0}{1}</color>";
            logger.LogErrorFormat = "<color=#AF2543>{0}{1}</color>";
            logger.LogExceptionFormat = "<color=#AF2543>{0}{1}: {2}</color>";
            UnityModManager.Logger.LogAction = (msg, prefix) =>
            {
                logger.LogPrefix = prefix == null ? "[UMM_Compat] " : $"[UMM_Compat] {prefix}";
                logger.Log(msg);
            };
            UnityModManager.Logger.LogErrorAction = (msg, prefix) =>
            {
                logger.LogPrefix = prefix == null ? "[UMM_Compat: Error] " : $"[UMM_Compat] {prefix}";
                logger.LogError(msg);
            };
            UnityModManager.Logger.LogExceptionAction = (msg, ex, prefix) =>
            {
                logger.LogPrefix = prefix == null ? "[UMM_Compat] " : $"[UMM_Compat] {prefix}";
                logger.LogException(msg, ex);
            };
            UnityModManager.ModSettings.SaveAction = (o, s) => ((ModSettings)o).Save(s);
            UnityModManager.ModSettings.LoadAction = s => ModSettings.Load(s);
        }
    }
}

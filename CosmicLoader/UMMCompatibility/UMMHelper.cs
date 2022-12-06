using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CosmicLoader.Core;
using CosmicLoader.Mod;
using Newtonsoft.Json.Linq;
using TinyJson;
using UnityEngine;
using UnityModManagerNet;

namespace CosmicLoader.UMMCompatibility
{
    public static class UMMHelper
    {
        private static Dictionary<UMMCompatMod, UnityModManager.ModEntry> _mods = new();
        public static UnityModManager.ModEntry GetMod(this UMMCompatMod mod) => _mods[mod];
        public static void RegisterEntry(UMMCompatMod mod, UnityModManager.ModEntry entry)
        {
            _mods.Add(mod, entry);
            UnityModManager.modEntries.Add(entry);
        }


        public static readonly Version LoaderVersion = new Version(0, 23, 4, 0);

        public static ModInfo ParseUMMInfo(string info)
        {
            var ummModInfo = info.FromJson<UnityModManager.ModInfo>();
            var modInfo = new UMMModInfo(ummModInfo);
            return modInfo;
        }

        public static UnityModManager.ModEntry CreateModEntry(UMMCompatMod mod)
        {
            CosmicManager.Logger.Log("Creating mod entry for " + mod.Info.Id);
            var info = mod.Info.ModInfo;
            return new UnityModManager.ModEntry(info, mod.Info.Path, mod);
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
                logger.LogPrefix = prefix == null ? "[CosmicLoader.UMMCompat] " : $"[CosmicLoader.UMMCompat] {prefix}";
                logger.Log(msg);
            };
            UnityModManager.Logger.LogErrorAction = (msg, prefix) =>
            {
                logger.LogPrefix = prefix == null ? "[CosmicLoader.UMMCompat: Error] " : $"[CosmicLoader.UMMCompat] {prefix}";
                logger.LogError(msg);
            };
            UnityModManager.Logger.LogExceptionAction = (msg, ex, prefix) =>
            {
                logger.LogPrefix = prefix == null ? "[CosmicLoader.UMMCompat] " : $"[CosmicLoader.UMMCompat] {prefix}";
                logger.LogException(msg, ex);
            };
            UnityModManager.Textures.Init();
            new GameObject().AddComponent<UnityModManager.UI.UIObj>();
        }
    }
}

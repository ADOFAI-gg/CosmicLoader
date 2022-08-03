using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Threading;
using HarmonyLib;

namespace AdofaiModManager
{
    public static class ModManager
    {
        public static ManagerObject Instance { get; private set; }
        public static bool Loaded { get; private set; }
        public static ManagerConfig Config { get; private set; }
        public static List<Mod> Mods { get; private set; }
        public static ModLogger Logger { get; private set; }
        public static void Initialize()
        {
            if (Loaded) return;
            try
            {
                Instance = new GameObject("ModManager").AddComponent<ManagerObject>();
                UnityEngine.Object.DontDestroyOnLoad(Instance.gameObject);
                OpenUnityFileLog();
                Mods = new List<Mod>();
                Logger = new ModLogger("Manager");
                if (File.Exists(ManagerConfig.Path))
                    Config = File.ReadAllText(ManagerConfig.Path).FromJson<ManagerConfig>() ?? new ManagerConfig();
                else
                {
                    Config = new ManagerConfig();
                    File.WriteAllText(ManagerConfig.Path, Config.ToJson());
                }
                const string mods = "Mods/";
                if (Directory.Exists(mods))
                {
                    string[] dirs = Directory.GetDirectories(mods, "*");
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        string dir = dirs[i];
                        IEnumerable<FileInfo> files = Directory.GetFiles(dir, "*").Select(f => new FileInfo(f));
                        FileInfo info = files.FirstOrDefault(f => f.Name.Equals("info.json", StringComparison.OrdinalIgnoreCase));
                        if (info == null) continue;
                        ModInfo mInfo = File.ReadAllText(info.FullName).FromJson<ModInfo>();
                        Mods.Add(new Mod(mInfo, dir));
                    }
                }
                else Directory.CreateDirectory(mods);
                Loaded = true;
            }
            catch (Exception e) { Logger.LogException("Manager Load Failed", e); OpenUnityFileLog(); }
        }
        public static void OpenUnityFileLog()
        {
            string[] dataPaths = new string[]
            {
                Application.persistentDataPath,
                Application.dataPath
            };
            string[] logPaths = new string[]
            {
                "Player.log",
                "output_log.txt"
            };
            foreach (string path in dataPaths)
            {
                foreach (string path2 in logPaths)
                {
                    string text = Path.Combine(path, path2);
                    if (File.Exists(text))
                    {
                        Application.OpenURL(text);
                        return;
                    }
                }
            }
        }
    }
}

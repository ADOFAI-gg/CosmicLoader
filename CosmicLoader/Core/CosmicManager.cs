using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CosmicLoader.Mod;
using CosmicLoader.UI;
using CosmicLoader.UMMCompatibility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CosmicLoader.Core
{
    public static class CosmicManager
    {
        static CosmicManager()
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version;
        }
        
        public static readonly Version Version;
        public static ManagerObject Instance { get; private set; }
        public static bool Loaded { get; private set; }
        public static ManagerConfig Config { get; private set; }
        public static List<ModBase> Mods { get; private set; }
        public static ModLogger Logger { get; private set; }

        public static void Initialize()
        {
            if (Loaded) return;
            UMMHelper.IntegrateUMM();
            try
            {
                Instance = new GameObject("ModManager").AddComponent<ManagerObject>();
                UnityEngine.Object.DontDestroyOnLoad(Instance.gameObject);
                OpenUnityFileLog();
                Mods = new List<ModBase>();
                Logger = new ModLogger("CosmicLoader");
                if (File.Exists(ManagerConfig.Path))
                    Config = File.ReadAllText(ManagerConfig.Path).FromJson<ManagerConfig>() ?? new ManagerConfig();
                else
                {
                    Config = new ManagerConfig();
                    File.WriteAllText(ManagerConfig.Path, Config.ToJson());
                }

                const string modsPath = "Mods/";
                if (Directory.Exists(modsPath))
                {
                    string[] mods = Directory.GetDirectories(modsPath);
                    var infos = new List<ModInfo>();
                    mods = mods.Distinct().ToArray();
                    foreach (var modPath in mods)
                    {
                        try
                        {
                            var infoPath = Path.Combine(modPath, "Info.json");
                            if (!File.Exists(infoPath)) continue;
                            var jObject = JObject.Parse(File.ReadAllText(infoPath));
                            ModInfo info;
                            if (jObject.ContainsKey("AssemblyName"))
                            {
                                info = UMMHelper.ParseUMMInfo(jObject);
                                if (info == null) continue;
                            }
                            else
                            {
                                info = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(infoPath));
                                if (info == null) continue;
                                info.IsUMM = false;
                            }

                            info.Path = Path.Combine(Directory.GetCurrentDirectory(), modPath);

                            info.References ??= Array.Empty<string>();
                            info.LoadAfter ??= Array.Empty<string>();
                            info.LoadBefore ??= Array.Empty<string>();
                            infos.Add(info);
                        }
                        catch (Exception e)
                        {
                            Logger.LogException($"Error while loading mod at path {modPath}", e);
                        }
                    }

                    var sortedInfos = new List<ModInfo>();
                    for (int a = 0; a < infos.Count; a++)
                    {
                        foreach (var info in infos)
                        {
                            if (sortedInfos.Contains(info)) continue;
                            foreach (var after in info.LoadAfter)
                            {
                                if (sortedInfos.All(x => x.Name != after)) goto exit;
                            }

                            foreach (var i in infos)
                            {
                                if (sortedInfos.Contains(i)) continue;
                                if (i.LoadBefore.Contains(info.Name)) goto exit;
                            }

                            sortedInfos.Add(info);

                            exit:;
                        }
                    }

                    if (sortedInfos.Count != infos.Count)
                    {
                        var circs = new List<string>();
                        foreach (var info in infos)
                        {
                            if (sortedInfos.Contains(info)) continue;
                            circs.Add(info.Id);
                        }

                        Logger.LogError("Circular dependency detected! (" + string.Join(", ", circs) + ")");
                    }

                    foreach (var info in sortedInfos)
                    {
                        Logger.Log("Loading mod: " + info.Id);
                        try
                        {
                            ModBase mod;
                            if (info.IsUMM)
                            {
                                mod = new UMMCompatMod(info);
                            }
                            else
                            {
                                mod = new ModPlaceholder(info);
                            }

                            Mods.Add(mod);
                            mod.Initialize();
                        }
                        catch (Exception e)
                        {
                            Logger.LogException($"Error while loading mod {info.Id}", e);
                        }
                    }

                    ModWindow.Instance.Initialize(CosmicManager.Mods);
                }
                else Directory.CreateDirectory(modsPath);

                Loaded = true;
            }
            catch (Exception e)
            {
                Logger.LogException("Manager Load Failed", e);
                OpenUnityFileLog();
            }
        }

        public static void OpenUnityFileLog()
        {
            string[] dataPaths = new string[] {Application.persistentDataPath, Application.dataPath};
            string[] logPaths = new string[] {"Player.log", "output_log.txt"};
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

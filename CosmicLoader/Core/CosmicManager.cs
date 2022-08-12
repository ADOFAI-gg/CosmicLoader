using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CosmicLoader.Attributes;
using CosmicLoader.Mod;
using CosmicLoader.UI;
using CosmicLoader.UMMCompatibility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TinyJson;
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
        
        public const string LibrariesPath = "Libraries";
        public const string ModulesPath = "Modules";
        
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

                var modsPath = Path.Combine(Directory.GetCurrentDirectory(), "Mods/");
                if (Directory.Exists(modsPath))
                {
                    string[] mods = Directory.GetDirectories(modsPath);
                    var infos = new List<ModInfo>();
                    Action afterLoad = null;
                    foreach (var modPath in mods)
                    {
                        try
                        {
                            var infoPath = Path.Combine(modPath, "Info.json");
                            if (File.Exists(infoPath))
                            {
                                var info = UMMHelper.ParseUMMInfo(File.ReadAllText(infoPath));
                                info.Path = modPath;
                                infos.Add(info);
                                if (info == null) continue;
                            }
                            else
                            {
                                var assemblies = Directory.GetFiles(infoPath, "*.dll")
                                        .Concat(Directory.GetFiles(Path.Combine(infoPath, LibrariesPath), "*.dll"))
                                        .Concat(Directory.GetFiles(Path.Combine(infoPath, ModulesPath), "*.dll"))
                                        .ToArray();
                                
                                if (assemblies.Length == 0) continue;
                                if (assemblies.Length > 1) Logger.LogWarning($"Found multiple assemblies in {infoPath}");
                                Assembly assembly = null;
                                var modDirName = Path.GetDirectoryName(modPath) + ".dll";
                                foreach (var assemblyPath in assemblies.Reverse())
                                {
                                    var fileName = Path.GetFileName(assemblyPath);
                                    if (fileName == modDirName) assembly = Assembly.LoadFrom(assemblyPath);
                                    else Assembly.LoadFrom(assemblyPath);
                                }
                                assembly ??= Assembly.LoadFrom(assemblies[0]);

                                afterLoad += () =>
                                {
                                    try
                                    {
                                        var attr = assembly!.GetCustomAttribute<CosmicModAttribute>();
                                        if (attr == null) return;
                                        var loadAfter = assembly.GetCustomAttributes<LoadAfterAttribute>();
                                        var loadBefore = assembly.GetCustomAttributes<LoadBeforeAttribute>();
                                        var info = new ModInfo
                                        {
                                            Id = attr.Id,
                                            Name = attr.Name,
                                            Version = attr.Version,
                                            Author = attr.Author,
                                            GameVersion = attr.GameVersion,
                                            Category = attr.Category,
                                            LoadAfter = loadAfter.SelectMany(l => l.LoadAfter).ToArray(),
                                            LoadBefore = loadBefore.SelectMany(l => l.LoadBefore).ToArray(),
                                            ModType = attr.MainClass,
                                            Assembly = assembly,
                                            Path = modPath
                                        };
                                        infos.Add(info);
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.LogException($"Error while loading mod at path {modPath}", e);
                                    }
                                };
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.LogException($"Error while loading mod at path {modPath}", e);
                        }
                    }

                    Logger.Log("Loaded UMM Compat infos and CosmicLoader assemblies");
                    afterLoad?.Invoke();
                    Logger.Log("Loaded CosmicLoader infos");
                    
                    infos.Sort((a, b) => String.Compare(a.Id, b.Id, StringComparison.Ordinal));

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
                    
                    Logger.Log("Sorted infos");

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
                                mod = new UMMCompatMod();
                            }
                            else
                            {
                                var type = info.ModType;
                                if (type.IsSubclassOf(typeof(CosmicMod)))
                                {
                                    Logger.LogError($"{info.Id} is not a valid mod type!");
                                    mod = new ModPlaceholder();
                                    goto Load;
                                }

                                var instance = Activator.CreateInstance(type);
                                if (instance == null)
                                {
                                    Logger.LogError($"Could not create instance of {type}");
                                    mod = new ModPlaceholder();
                                    goto Load;
                                }
                                
                                mod = (ModBase) instance;
                            }
                            
                            Load:
                            Mods.Add(mod);
                            mod.Setup(info);
                            mod.Initialize();
                        }
                        catch (Exception e)
                        {
                            Logger.LogException($"Error while initializing mod {info.Id}", e);
                        }
                    }

                    ModWindow.Instance.Initialize(Mods);
                }
                else Directory.CreateDirectory(modsPath);
                
                Logger.Log("Loading mods complete");

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

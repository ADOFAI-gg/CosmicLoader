using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;
//using TinyJson;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace UnityModManagerNet {
    public class UnityModManager {
        public static readonly List<ModEntry> modEntries = new List<ModEntry>();

        /// <summary>Contains version of UnityEngine</summary>
        public static Version unityVersion { get; private set; }

        /// <summary>Contains version of a game, if configured [0.15.0]</summary>
        public static Version gameVersion { get; private set; } = new Version();

        /// <summary>Contains version of UMM</summary>
        public static Version version { get; private set; } = typeof(UnityModManager).Assembly.GetName().Version;

        public static string modsPath { get; private set; }

        public static ModEntry FindMod(string id) =>
            modEntries.FirstOrDefault(
                x => x.Info.Id == id);

        public static Version GetVersion() => version;

        public static void SaveSettingsAndParams() { }

        public static bool HasNetworkConnection() {
            try {
                using (var ping = new System.Net.NetworkInformation.Ping())
                    return ping.Send("www.google.com.mx", 3000).Status == IPStatus.Success;
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public static void OpenUnityFileLog() {
            string[] strArray1 = new string[2] {
                Application.persistentDataPath,
                Application.dataPath
            };
            string[] strArray2 = new string[2] {
                "Player.log",
                "output_log.txt"
            };
            foreach (string path1 in strArray1) {
                foreach (string path2 in strArray2) {
                    string str = Path.Combine(path1, path2);
                    if (File.Exists(str)) {
                        Thread.Sleep(500);
                        Application.OpenURL(str);
                        return;
                    }
                }
            }
        }

        public static Version ParseVersion(string str) {
            string[] strArray = str.Split('.');
            if (strArray.Length >= 3) {
                Regex regex = new Regex("\\D");
                return new Version(int.Parse(regex.Replace(strArray[0], "")), int.Parse(regex.Replace(strArray[1], "")),
                    int.Parse(regex.Replace(strArray[2], "")));
            }

            if (strArray.Length >= 2) {
                Regex regex = new Regex("\\D");
                return new Version(int.Parse(regex.Replace(strArray[0], "")),
                    int.Parse(regex.Replace(strArray[1], "")));
            }

            if (strArray.Length >= 1)
                return new Version(int.Parse(new Regex("\\D").Replace(strArray[0], "")), 0);
            Logger.Error("Error parsing version " + str);
            return new Version();
        }

        public static bool IsUnixPlatform() {
            int platform = (int)Environment.OSVersion.Platform;
            switch (platform) {
                case 4:
                case 6:
                    return true;
                default:
                    return platform == 128;
            }
        }

        public static bool IsMacPlatform() => Environment.OSVersion.Platform == PlatformID.MacOSX;

        public static bool IsLinuxPlatform() {
            int platform = (int)Environment.OSVersion.Platform;
            return platform == 4 || platform == 128;
        }

        [XmlRoot("Config")]
        public class GameInfo {
            [XmlAttribute]
            public string Name;

            public string Folder;
            public string ModsDirectory;
            public string ModInfo;
            public string EntryPoint;
            public string StartingPoint;
            public string UIStartingPoint;
            public string GameExe;
            public string GameVersionPoint;
            public string MinimalManagerVersion;

            private static readonly string filepath =
                Path.Combine(Path.GetDirectoryName(typeof(GameInfo).Assembly.Location), "Config.xml");

            public static GameInfo Load() {
                try {
                    using (FileStream fileStream = File.OpenRead(GameInfo.filepath))
                        return new XmlSerializer(typeof(GameInfo)).Deserialize(fileStream) as
                            GameInfo;
                }
                catch (Exception ex) {
                    Logger.Error("Can't read file '" + GameInfo.filepath + "'.");
                    Debug.LogException(ex);
                    return null;
                }
            }
        }

        public class ModEntry {
            public readonly object Mod; // CosmicLoader.Mod

            public readonly ModInfo Info;

            /// <summary>Path to mod folder</summary>
            public readonly string Path;

            /// <summary>Version of a mod</summary>
            public readonly Version Version;

            /// <summary>Required UMM version</summary>
            public readonly Version ManagerVersion;

            /// <summary>Required game version [0.15.0]</summary>
            public readonly Version GameVersion;

            /// <summary>Not used</summary>
            public Version NewestVersion;

            /// <summary>Required mods</summary>
            public readonly Dictionary<string, Version> Requirements = new Dictionary<string, Version>();

            /// <summary>
            /// List of mods after which this mod should be loaded [0.22.5]
            /// </summary>
            public readonly List<string> LoadAfter = new List<string>();

            /// <summary>
            /// Displayed in UMM UI. Add <color></color> tag to change colors. Can be used when custom verification game version [0.15.0]
            /// </summary>
            public string CustomRequirements = string.Empty;

            public readonly ModEntry.ModLogger Logger;

            /// <summary>Not used</summary>
            public bool HasUpdate;

            /// <summary>Called to unload old data for reloading mod [0.14.0]</summary>
            public Func<ModEntry, bool> OnUnload;

            /// <summary>Called to activate / deactivate the mod</summary>
            public Func<ModEntry, bool, bool> OnToggle;

            /// <summary>
            /// Called by MonoBehaviour.OnGUI when mod options are visible.
            /// </summary>
            public Action<ModEntry> OnGUI;

            /// <summary>Called by MonoBehaviour.OnGUI, always [0.21.0]</summary>
            public Action<ModEntry> OnFixedGUI;

            /// <summary>Called when opening mod GUI [0.16.0]</summary>
            public Action<ModEntry> OnShowGUI;

            /// <summary>Called when closing mod GUI [0.16.0]</summary>
            public Action<ModEntry> OnHideGUI;

            /// <summary>Called when the game closes</summary>
            public Action<ModEntry> OnSaveGUI;

            /// <summary>Called by MonoBehaviour.Update [0.13.0]</summary>
            public Action<ModEntry, float> OnUpdate;

            /// <summary>Called by MonoBehaviour.LateUpdate [0.13.0]</summary>
            public Action<ModEntry, float> OnLateUpdate;

            /// <summary>Called by MonoBehaviour.FixedUpdate [0.13.0]</summary>
            public Action<ModEntry, float> OnFixedUpdate;

            private readonly Dictionary<long, MethodInfo> mCache = new Dictionary<long, MethodInfo>();

            /// <summary>UI checkbox</summary>
            public bool Enabled = true;

            public Assembly Assembly => AssemblyAction(Mod);

            /// <summary>Show button to reload the mod [0.14.0]</summary>
            public bool CanReload { get; private set; }

            public bool Started => LoadedAction(Mod);

            public bool ErrorOnLoading => LoadFailedAction(Mod);

            /// <summary>If OnToggle exists</summary>
            public bool Toggleable => this.OnToggle != null;

            /// <summary>If Assembly is loaded [0.13.1]</summary>
            public bool Loaded => this.Assembly != null;

            public bool Active {
                get => ActiveAction(Mod);
                set => SetActiveAction(Mod, value);
            }

            public static Func<object, Assembly> AssemblyAction;
            public static Func<object, bool> LoadedAction;
            public static Func<object, bool> LoadFailedAction;
            public static Func<object, bool> ActiveAction;
            public static Action<object, bool> SetActiveAction;

            public ModEntry(ModInfo info, string path, object mod) {
                this.Mod = mod;
                this.Info = info;
                this.Path = path;

                this.Logger = new ModEntry.ModLogger(this.Info.Id);
                this.Version = ParseVersion(info.Version);
                this.ManagerVersion = new Version();
                this.GameVersion = new Version();

                if (info.LoadAfter == null || info.LoadAfter.Length == 0)
                    return;
                this.LoadAfter.AddRange(info.LoadAfter);
            }

            public class ModLogger {
                protected readonly string Prefix;
                protected readonly string PrefixError;
                protected readonly string PrefixCritical;
                protected readonly string PrefixWarning;
                protected readonly string PrefixException;

                public ModLogger(string Id) {
                    this.Prefix = "[" + Id + "] ";
                    this.PrefixError = "[" + Id + "] [Error] ";
                    this.PrefixCritical = "[" + Id + "] [Critical] ";
                    this.PrefixWarning = "[" + Id + "] [Warning] ";
                    this.PrefixException = "[" + Id + "] [Exception] ";
                }

                public void Log(string str) => UnityModManager.Logger.Log(str, this.Prefix);

                public void Error(string str) => UnityModManager.Logger.Log(str, this.PrefixError);

                public void Critical(string str) => UnityModManager.Logger.Log(str, this.PrefixCritical);

                public void Warning(string str) => UnityModManager.Logger.Log(str, this.PrefixWarning);

                public void NativeLog(string str) => UnityModManager.Logger.NativeLog(str, this.Prefix);

                /// <summary>[0.17.0]</summary>
                public void LogException(string key, Exception e) =>
                    UnityModManager.Logger.LogException(key, e, this.PrefixException);

                /// <summary>[0.17.0]</summary>
                public void LogException(Exception e) =>
                    UnityModManager.Logger.LogException(null, e, this.PrefixException);
            }
        }

        public static class Logger {
            public static readonly string filepath =
                Path.Combine(Path.Combine(Application.dataPath, Path.Combine("Managed", nameof(UnityModManager))),
                    "Log.txt");

            public static void NativeLog(string str) => NativeLog(str, "[UMM-Compat] ");

            public static void NativeLog(string str, string prefix) => LogAction(prefix + str);

            public static void Log(string str) => Logger.Log(str, "[UMM-Compat] ");

            public static void Log(string str, string prefix) => LogAction(prefix + str);

            public static void Error(string str) => Logger.Error(str, "[UMM-Compat] [Error] ");

            public static void Error(string str, string prefix) => LogErrorAction(prefix + str);

            /// <summary>[0.17.0]</summary>
            public static void LogException(Exception e) =>
                Logger.LogException(null, e, "[UMM-Compat] [Exception] ");

            /// <summary>[0.17.0]</summary>
            public static void LogException(string key, Exception e) =>
                Logger.LogException(key, e, "[UMM-Compat] [Exception] ");

            /// <summary>[0.17.0]</summary>
            public static void LogException(string key, Exception e, string prefix) => LogExceptionAction(key, e);

            public static Action<string> LogAction { get; set; }
            public static Action<string> LogErrorAction { get; set; }
            public static Action<string, Exception> LogExceptionAction { get; set; }

            public static void Clear() { }
        }

        public class Repository {
            public Release[] Releases;

            [Serializable]
            public class Release : IEquatable<Release> {
                public string Id;
                public string Version;
                public string DownloadUrl;

                public bool Equals(Repository.Release other) => this.Id.Equals(other.Id);

                public override bool Equals(object obj) =>
                    obj != null && obj is Repository.Release other && this.Equals(other);

                public override int GetHashCode() => this.Id.GetHashCode();
            }
        }

        public class ModSettings {
            public object Settings { get; private set; } // CosmicLoader.ModSettings
            public static Action<object, string> SaveAction;
            public static Func<string, object> LoadAction;

            public virtual void Save(ModEntry modEntry) => SaveAction(Settings, modEntry.Info.Id);

            public virtual string GetPath(ModEntry modEntry) =>
                Path.Combine(modEntry.Path, "Settings.xml");

            public static void Save<T>(T data, ModEntry modEntry)
                where T : ModSettings, new() =>
                ModSettings.Save<T>(data, modEntry, null);

            /// <summary>[0.20.0]</summary>
            public static void Save<T>(
                T data,
                ModEntry modEntry,
                XmlAttributeOverrides attributes)
                where T : ModSettings, new() {
                string path = data.GetPath(modEntry);
                try {
                    SaveAction(data.Settings, path);
                }
                catch (Exception ex) {
                    modEntry.Logger.Error("Can't save " + path + ".");
                    modEntry.Logger.LogException(ex);
                }
            }

            public static T Load<T>(ModEntry modEntry) where T : ModSettings, new() {
                T obj = new T();
                string path = obj.GetPath(modEntry);
                if (File.Exists(path)) {
                    try {
                        obj.Settings = LoadAction(path);
                    }
                    catch (Exception ex) {
                        modEntry.Logger.Error("Can't read " + path + ".");
                        modEntry.Logger.LogException(ex);
                    }
                }

                return obj;
            }

            public static T Load<T>(ModEntry modEntry, XmlAttributeOverrides attributes)
                where T : ModSettings, new() => ModSettings.Load<T>(modEntry);
        }

        public class ModInfo : IEquatable<ModInfo> {
            public string Id;
            public string DisplayName;
            public string Author;
            public string Version;
            public string ManagerVersion;
            public string GameVersion;
            public string[] Requirements;
            public string[] LoadAfter;
            public string AssemblyName;
            public string EntryMethod;
            public string HomePage;
            public string Repository;

            /// <summary>Used for RoR2 game [0.17.0]</summary>
            [NonSerialized]
            public bool IsCheat = true;

            public static implicit operator bool(ModInfo exists) => exists != null;

            public bool Equals(ModInfo other) => this.Id.Equals(other.Id);

            public override bool Equals(object obj) =>
                obj != null && obj is ModInfo other && this.Equals(other);

            public override int GetHashCode() => this.Id.GetHashCode();
        }
    }
}

using System;
using System.Reflection;
using Newtonsoft.Json;

namespace CosmicLoader.Mod
{
    public class ModInfo
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }

        public string Author { get; internal set; }
        public string Version { get; internal set; }
        public string GameVersion { get; internal set; }
        public string[] LoadAfter { get; internal set; }
        public string[] LoadBefore { get; internal set; }
        public ModCategory Category { get; internal set; }
        public Type ModType { get; internal set; }
        public Assembly Assembly { get; internal set; }

        [JsonIgnore] public string Path { get; internal set; }
        [JsonIgnore] public virtual bool IsUMM => false;
    }
}

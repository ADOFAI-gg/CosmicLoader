using Newtonsoft.Json;

namespace CosmicLoader.Mod
{
    public class ModInfo
    {
        public string Id;
        public string Name;
        public string FileName;
        public string EntryPoint;
        public string[] References;

        public string Author;
        public string Version;
        public string GameVersion;
        public string[] LoadAfter;
        public string[] LoadBefore;

        public string ModType;

        [JsonIgnore] public string Path { get; internal set; }
        [JsonIgnore] public bool IsUMM { get; internal set; }
    }
}

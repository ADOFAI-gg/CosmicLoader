using System;
using CosmicLoader.Mod;

namespace CosmicLoader.Attributes
{
    /// <summary>
    /// Metadata attribute for mod loader.
    /// For under C# 10
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class CosmicModAttribute : Attribute
    {
        public virtual Type MainClass { get; set; }
        public string Id;
        public string Name;
        public string Author;
        public string Version;
        public string GameVersion;
        public ModCategory Category;
    }
    
    /// <summary>
    /// Metadata attribute for mod loader.
    /// For C# 11 or higher
    /// </summary>
    /// <typeparam name="T">Main class for the mod</typeparam>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class CosmicModAttribute<T> : CosmicModAttribute where T : CosmicMod, new()
    {
        public override Type MainClass => typeof(T);
    }
}

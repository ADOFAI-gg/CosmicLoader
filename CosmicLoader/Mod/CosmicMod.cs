using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CosmicLoader.Mod
{
    public abstract class CosmicMod : ModBase
    {
        public override Assembly Assembly { get; }
        public sealed override ModState State { get; internal set; }
        public CosmicMod(ModInfo mInfo) : base(mInfo) { }

        public CosmicMod(ModInfo info, Assembly assembly) : base(info)
        {
            Assembly = assembly;
        }
    }
}

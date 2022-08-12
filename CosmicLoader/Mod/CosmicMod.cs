using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CosmicLoader.Mod
{
    public abstract class CosmicMod : ModBase
    {
        public sealed override ModState State { get; internal set; }
    }
}

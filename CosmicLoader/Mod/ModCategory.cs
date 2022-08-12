using System;

namespace CosmicLoader.Mod
{
    public enum ModCategory
    {
        Overlay         = 1 << 0,
        BugFix          = 1 << 1,
        Editor          = 1 << 2,
        Gameplay        = 1 << 3,
        Tweaks          = 1 << 4,
        Library         = 1 << 5,
        Misc            = 1 << 6,
        Debug           = 1 << 7,
        
        [Obsolete("This is for only UMM mods")]
        UMM            = 1 << 31,
    }
}

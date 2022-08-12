using System;
using CosmicLoader.Mod;
using UnityModManagerNet;

namespace CosmicLoader.UMMCompatibility
{
    public class UMMModInfo : ModInfo
    {
        public UnityModManager.ModInfo ModInfo;
        public override bool IsUMM => true;

        public UMMModInfo(UnityModManager.ModInfo modInfo)
        {
            ModInfo = modInfo;
            this.Id = modInfo.Id;
            this.Name = modInfo.DisplayName;
            this.Author = modInfo.Author;
            this.Version = modInfo.Version;
            this.LoadAfter = modInfo.LoadAfter ?? Array.Empty<string>();
            this.LoadBefore = Array.Empty<string>();
#pragma warning disable CS0618
            this.Category = ModCategory.UMM;
#pragma warning restore CS0618
            this.ModType = typeof(UMMCompatMod);
        }
    }
}

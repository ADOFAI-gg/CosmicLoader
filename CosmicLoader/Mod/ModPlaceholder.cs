using System;
using System.Reflection;

namespace CosmicLoader.Mod
{
    public class ModPlaceholder : ModBase
    {
        public ModPlaceholder(ModInfo mInfo) : base(mInfo) { }
        public override Assembly Assembly => null;

        public override ModState State
        {
            get => ModState.LoadFailed;
            internal set => throw new NotSupportedException();
        }
        protected override void Load(bool active) { }
        protected internal override void OnToggle(bool active) { }
        protected internal override void OnUpdate() { }
        protected internal override void OnExit() { }
        protected internal override void OnGUI() { }
    }
}

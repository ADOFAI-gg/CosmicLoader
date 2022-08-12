using CosmicLoader.Mod;

namespace CosmicLoader.Utils
{
    public static class Extensions
    {
        public static bool Ready(this ModState state)
        {
            return state is ModState.Active or ModState.Inactive;
        }
    }
}

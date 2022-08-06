using CosmicLoader.UI;
using UnityEngine;

namespace CosmicLoader.Starter
{
    public static class Injection
    {
        public static void Start() { }

        static Injection()
        {
            Debug.Log("[CosmicLoader] Injection started");
            ModManager.Initialize();
        }
    }
}

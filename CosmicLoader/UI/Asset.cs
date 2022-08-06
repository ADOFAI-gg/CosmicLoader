using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace CosmicLoader.UI
{
    public static class Asset
    {
        public static GameObject Window { get; private set; }
        public static GameObject ModButton { get; private set; }

        public static AssetBundle Bundle { get; private set; }

        public static void Load()
        {
            const string resourceName = "CosmicLoader.UIAsset";
            var assembly = Assembly.GetExecutingAssembly();
            using var resourceStream = assembly.GetManifestResourceStream(resourceName);
            var bundle = AssetBundle.LoadFromStream(resourceStream);
            Bundle = bundle;
            Debug.Log($"Loaded {bundle}");
            Window = bundle.LoadAsset<GameObject>("ModWindow").gameObject;
            ModButton = bundle.LoadAsset<GameObject>("ModBtn");
        }
    }
}

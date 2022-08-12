using System;

namespace CosmicLoader.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class LoadAfterAttribute : Attribute
    {
        public string[] LoadAfter { get; }
        public LoadAfterAttribute(params string[] loadAfter) => LoadAfter = loadAfter;
    }
}

using System;

namespace CosmicLoader.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class LoadBeforeAttribute : Attribute
    {
        public string[] LoadBefore { get; }
        public LoadBeforeAttribute(params string[] loadBefore) => LoadBefore = loadBefore;
    }
}

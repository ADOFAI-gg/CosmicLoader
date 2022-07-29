using System;
using System.IO;

namespace AdofaiModManager
{
    public class Library
    {
        public string FileName;
        public Library[] References;
        public void Resolve(string modPath)
        {
            for (int i = 0; i < References?.Length; i++)
                References[i].Resolve(modPath);
            AppDomain.CurrentDomain.Load(File.ReadAllBytes($"{modPath}/{FileName}"));
        }
    }
}

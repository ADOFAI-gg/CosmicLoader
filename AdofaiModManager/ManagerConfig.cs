﻿using System.Collections.Generic;

namespace AdofaiModManager
{
    public class ManagerConfig
    {
        public static readonly string Path = "ManagerConfig.json";
        public Dictionary<string, bool> States = new Dictionary<string, bool>();
        public List<string> Logs = new List<string>();
    }
}

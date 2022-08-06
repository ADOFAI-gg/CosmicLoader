using System;
using System.Reflection;
using HarmonyLib;

namespace CosmicLoader
{
    public abstract class ModBase
    {
        public ModInfo Info { get; }
        public ModLogger Logger { get; }
        public Harmony Harmony { get;  }
        public string Path { get; }
        public abstract Assembly Assembly { get; protected set; }
        public abstract bool Loaded { get; protected set; }
        public abstract bool LoadFailed { get; protected set; }
        
        
        private bool _active = false;
        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                if (_active)
                    TryLoad();
                OnToggle?.Invoke(_active);
            }
        }

        public abstract Action<bool> OnToggle { get; set; }
        public abstract Action<bool> OnInitialize { get; set; }
        public abstract Action OnUpdate { get; set; }
        public abstract Action OnExit { get; set; }
        public abstract Action OnGUI { get; set; }


        public bool TryLoad()
        {
            if (Loaded)
            {
                LoadFailed = false;
                return true;
            }

            try
            {
                Load();
            }
            catch (Exception e)
            {
                Logger.LogException($"Error while loading mod {Info.Id}", e);
                LoadFailed = true;
                return false;
            }

            LoadFailed = false;
            return true;
        }

        protected abstract void Load();

        protected ModBase(ModInfo mInfo)
        {
            Info = mInfo;
            Logger = new ModLogger(mInfo.Name);
            Harmony = new Harmony(Info.Id);
            Path = Info.Path;
        }
        
        public virtual void Initialize()
        {
            var dict = ModManager.Config.States;
            if (!dict.TryGetValue(Info.Id, out bool state))
            {
                dict[Info.Id] = true;
                state = true;
            }
            
            OnInitialize?.Invoke(state);
            Active = state;
        }
    }
}

using System;
using System.Reflection;
using CosmicLoader.Core;
using CosmicLoader.Utils;
using HarmonyLib;

namespace CosmicLoader.Mod
{
    public abstract class ModBase
    {
        public ModInfo Info { get; private set; }
        public ModLogger Logger { get; private set; }
        public Harmony Harmony { get; private set; }
        public abstract ModState State { get; internal set; }
        public bool Active => State == ModState.Active;

        
        /// <summary>
        /// Toggle mod state.
        /// </summary>
        /// <exception cref="Exception">Mod state is invalid or mod toggle failed</exception>
        public void Toggle()
        {
            if (State == ModState.BeforeLoad) throw new Exception($"Mod {Info.Id} is not loaded");
            if (!State.Ready()) throw new Exception($"Mod {Info.Id} is in error state");
            try
            {
                if (State == ModState.Active)
                {
                    State = ModState.Inactive;
                    OnToggle(false);
                }
                else
                {
                    State = ModState.Active;
                    OnToggle(true);
                }
            } catch (Exception e)
            {
                State = ModState.Error;
                throw new Exception($"Mod {Info.Id} toggle failed", e);
            }
        }


        public bool TryLoad()
        {
            if (State != ModState.BeforeLoad)
            {
                return true;
            }

            var dict = CosmicManager.Config.States;
            if (!dict.TryGetValue(Info.Id, out bool state))
            {
                dict[Info.Id] = true;
            }
            try
            {
                Load(state);
                State = state ? ModState.Active : ModState.Inactive;
                OnToggle(state);
            }
            catch (Exception e)
            {
                Logger.LogException($"Error while loading mod {Info.Id}", e);
                State = ModState.LoadFailed;
                return false;
            }

            return true;
        }

        protected abstract void Load(bool active);

        protected ModBase() { }

        internal void Setup(ModInfo info)
        {
            Info = info;
            Logger = new ModLogger(info.Name);
            Harmony = new Harmony(Info.Id);
        }
        
        protected internal virtual void Initialize()
        {
            TryLoad();
        }

        /// <summary>
        /// An event function called right after mod goes active/inactive
        /// </summary>
        /// <param name="active">Whether mod is active or not</param>
        protected internal abstract void OnToggle(bool active);

        /// <summary>
        /// An event function called in every frame
        /// </summary>
        protected internal abstract void OnUpdate();
        
        /// <summary>
        /// An event function called when exiting the game
        /// </summary>
        protected internal abstract void OnExit();
        
        /// <summary>
        /// An event function to display mod settings
        /// </summary>
        protected internal abstract void OnGUI();
    }
}

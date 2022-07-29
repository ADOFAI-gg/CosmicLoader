using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

namespace AdofaiModManager
{
    public class ManagerObject : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(this);
            Application.quitting += () =>
            {
                File.WriteAllText(ManagerConfig.Path, ModManager.Config.ToJson());
                foreach (Mod mod in ModManager.Mods)
                {
                    if (!mod.Active) continue;
                    try { mod.OnExit?.Invoke(); }
                    catch (Exception e) { mod.Logger.LogException("OnExit", e); }
                }
            };
        }
        public void Update()
        {
            foreach (Mod mod in ModManager.Mods)
            {
                if (!mod.Active) continue;
                try { mod.OnUpdate?.Invoke(); }
                catch (Exception e) { mod.Logger.LogException("OnUpdate", e); }
            }
        }
        public void OnGUI()
        {
            if (GUILayout.Button("Open Log"))
                ModManager.OpenUnityFileLog();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

namespace CosmicLoader
{
    public class ManagerObject : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(this);
            Application.quitting += () =>
            {
                File.WriteAllText(ManagerConfig.Path, ModManager.Config.ToJson());
                foreach (ModBase mod in ModManager.Mods)
                {
                    if (!mod.Active) continue;
                    try
                    {
                        mod.OnExit?.Invoke();
                    }
                    catch (Exception e)
                    {
                        mod.Logger.LogException("OnExit", e);
                    }
                }
            };
        }

        public void Update()
        {
            foreach (ModBase mod in ModManager.Mods)
            {
                if (!mod.Active) continue;
                try
                {
                    mod.OnUpdate?.Invoke();
                }
                catch (Exception e)
                {
                    mod.Logger.LogException("OnUpdate", e);
                }
            }
        }

        public void OnGUI()
        {
            if (GUILayout.Button("Open Log"))
                ModManager.OpenUnityFileLog();
            if (GUILayout.Button("Settings"))
                temp_openGui = !temp_openGui;

            if (!temp_openGui) return;
            GUILayout.BeginScrollView(scroll, GUILayout.Width(600), GUILayout.Height(800));
            foreach (var m in ModManager.Mods)
            {
                GUILayout.Label("-----------------------------------------------------");
                GUILayout.Label(m.Info.Name);
                try
                {
                    m.OnGUI?.Invoke();
                }
                catch (Exception e)
                {
                    GUILayout.TextArea(e.ToString());
                }

                GUILayout.Space(10);
            }

            GUILayout.EndScrollView();
        }

        public static bool temp_openGui = false;
        public static Vector2 scroll = new Vector2();
    }
}

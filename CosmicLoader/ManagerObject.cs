using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using CosmicLoader.UI;
using UnityEngine.UI;

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

            Asset.Load();
            var obj = Instantiate(Asset.Window).AddComponent<ModWindow>();
            
            var panel = obj.transform.GetChild(0);
            obj.scroll = panel.GetChild(3).GetComponent<ScrollRect>();
            obj.title = panel.GetChild(0).GetComponent<Text>();
            obj.exitBtn = obj.title.transform.GetChild(0).GetComponent<Button>();
            obj.selectedModEnabled = panel.GetChild(4).GetChild(2).GetComponent<Button>();
            obj.selectedModTitle = panel.GetChild(4).GetChild(0).GetComponent<Text>();
            obj.selectedModVersion = panel.GetChild(4).GetChild(1).GetComponent<Text>();
            obj.modGUIRect = panel.GetChild(5).GetComponent<RectTransform>();

            obj.gameObject.SetActive(false);
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
            
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.F10))
            {
                ModWindow.Instance.gameObject.SetActive(!ModWindow.Instance.gameObject.activeSelf);
            }
        }

        public void OnGUI()
        {
            if (GUILayout.Button("Open Log"))
                ModManager.OpenUnityFileLog();
        }
    }
}

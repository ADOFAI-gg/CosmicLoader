using System;
using System.Collections.Generic;
using Newtonsoft.Json.Utilities.LinqBridge;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace CosmicLoader.UI
{
    public class ModWindow : MonoBehaviour
    {
        public static ModWindow Instance { get; private set; }

        public Text title;
        public ScrollRect scroll;
        public Text selectedModTitle;
        public Text selectedModVersion;
        public Button selectedModEnabled;
        public Button exitBtn;
        public RectTransform modGUIRect;

        [NonSerialized] public ModBase SelectedMod;
        [NonSerialized] public Button SelectedModBtn;


        public void SelectMod(ModBase mod)
        {
            SelectedMod = mod;
            selectedModTitle.text = mod.Info.Name;
            selectedModVersion.text = mod.Info.Version;

            if (!mod.Loaded || mod.LoadFailed) selectedModEnabled.targetGraphic.color = Color.red;
            else if (mod.Active) selectedModEnabled.targetGraphic.color = Color.green;
            else selectedModEnabled.targetGraphic.color = Color.gray;
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        public void Initialize(List<ModBase> mods)
        {
            GetComponent<Canvas>().sortingOrder = 100000;
            selectedModEnabled.onClick.AddListener(() =>
            {
                if (!SelectedMod.Loaded || SelectedMod.LoadFailed)
                {
                    selectedModEnabled.targetGraphic.color = Color.red;
                    return;
                }

                SelectedMod.Active = !SelectedMod.Active;
                if (SelectedMod.Active) selectedModEnabled.targetGraphic.color = Color.green;
                else selectedModEnabled.targetGraphic.color = Color.gray;
            });
            exitBtn.onClick.AddListener(() => gameObject.SetActive(false));

            title.text = "CosmicLoader v" + ModManager.Version;

            var first = true;
            foreach (var mod in mods)
            {
                var modbtn = Instantiate(Asset.ModButton, scroll.content);
                modbtn.GetComponent<Text>().text = mod.Info.Name;
                var btn = modbtn.GetComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.onClick.AddListener(() =>
                {
                    SelectMod(mod);
                    SelectedModBtn.targetGraphic.color = new Color32(0x9C, 0x9C, 0x9C, 0xFF);
                    btn.targetGraphic.color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
                    SelectedModBtn = btn;
                });

                btn.targetGraphic.color = new Color32(0x9C, 0x9C, 0x9C, 0xFF);
                if (first)
                {
                    SelectMod(mod);
                    SelectedModBtn = btn;
                    btn.targetGraphic.color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
                    first = false;
                }
            }

            var img = modGUIRect.gameObject.AddComponent<Image>();
            img.sprite = transform.GetChild(0).GetComponent<Image>().sprite;
            img.color = Color.white;
        }

        private void OnGUI()
        {
            if (SelectedMod.Loaded && !SelectedMod.LoadFailed && SelectedMod.Active)
            {
                var rect = new Rect(-268.444f, -200, 782.222f, 533.333f);
                rect.x += Screen.width / 2.0f;
                rect.y += Screen.height / 2.0f;
                GUILayout.BeginArea(rect);
                try
                {
                    SelectedMod.OnGUI?.Invoke();
                }
                catch (Exception e)
                {
                    GUILayout.TextArea(e.ToString());
                }

                GUILayout.EndArea();
            }
        }
    }
}

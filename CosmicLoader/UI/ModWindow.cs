using System;
using System.Collections.Generic;
using CosmicLoader.Core;
using CosmicLoader.Mod;
using CosmicLoader.Utils;
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
        [NonSerialized] public Vector2 ScrollPosition;


        public void SelectMod(ModBase mod)
        {
            SelectedMod = mod;
            selectedModTitle.text = mod.Info.Name;
            selectedModVersion.text = mod.Info.Version;

            switch (mod.State)
            {
                case ModState.BeforeLoad:
                case ModState.LoadFailed:
                case ModState.Error:
                    selectedModEnabled.targetGraphic.color = Color.red;
                    break;
                case ModState.Active:
                    selectedModEnabled.targetGraphic.color = Color.green;
                    break;
                case ModState.Inactive:
                    selectedModEnabled.targetGraphic.color = Color.gray;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            ScrollPosition = Vector2.zero;
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
            selectedModEnabled.onClick.AddListener(() =>
            {
                if (!SelectedMod.State.Ready())
                {
                    selectedModEnabled.targetGraphic.color = Color.red;
                    return;
                }

                SelectedMod.Toggle();
                if (SelectedMod.Active) selectedModEnabled.targetGraphic.color = Color.green;
                else selectedModEnabled.targetGraphic.color = Color.gray;
            });
            exitBtn.onClick.AddListener(() => gameObject.SetActive(false));

            var lg = scroll.content.GetComponent<VerticalLayoutGroup>();
            lg.padding = new RectOffset(15, 15, 15, 15);
            lg.spacing = 15;

            title.text = "CosmicLoader v" + CosmicManager.Version;

            var first = true;
            foreach (var mod in mods)
            {
                var modbtn = Instantiate(Asset.ModButton, scroll.content);
                modbtn.GetComponent<Text>().text = mod.Info.Name;
                var btn = modbtn.GetComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.onClick.AddListener(() =>
                {
                    if (SelectedMod.State.Ready()) SelectedModBtn.targetGraphic.color = new Color32(0x9C, 0x9C, 0x9C, 0xFF);
                    else SelectedModBtn.targetGraphic.color = new Color32(0xAF, 0x5C, 0x5C, 0xFF);
                    SelectMod(mod);
                    if (mod.State.Ready()) btn.targetGraphic.color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
                    else btn.targetGraphic.color = new Color32(0xFF, 0x7C, 0x7C, 0xFF);
                    SelectedModBtn = btn;
                });

                if (mod.State.Ready()) btn.targetGraphic.color = new Color32(0x9C, 0x9C, 0x9C, 0xFF);
                else btn.targetGraphic.color = new Color32(0xAF, 0x5C, 0x5C, 0xFF);
                if (first)
                {
                    SelectMod(mod);
                    SelectedModBtn = btn;
                    if (mod.State.Ready()) btn.targetGraphic.color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
                    else btn.targetGraphic.color = new Color32(0xFF, 0x7C, 0x7C, 0xFF);
                    first = false;
                }
            }
            
            GetComponent<Canvas>().sortingOrder = -100000;
        }

        private void OnGUI()
        {
            if (SelectedMod.Active)
            {
                var rect = new Rect(-268.444f, -200, 782.222f, 533.333f);
                rect.x += Screen.width / 2.0f;
                rect.y += Screen.height / 2.0f;
                GUILayout.BeginArea(rect);
                ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, GUILayout.Width(782.222f), GUILayout.Height(533.333f));
                try
                {
                    SelectedMod.OnGUI();
                }
                catch (Exception e)
                {
                    ModLogger.LogRaw(e.ToString());
                    GUILayout.TextArea(e.ToString());
                }

                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
        }
    }
}

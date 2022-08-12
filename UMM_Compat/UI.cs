using System;
using UnityEngine;

namespace UnityModManagerNet {
    public partial class UnityModManager {
        public static partial class UI {
            public class UIObj : MonoBehaviour {
                private void OnGUI() {
                    PrepareGUI();
                    Destroy(gameObject);
                }
            }
            
            public static GUIStyle window = null;
            public static GUIStyle h1 = null;
            public static GUIStyle h2 = null;
            public static GUIStyle bold = null;

            private static void PrepareGUI() {
                window = new GUIStyle();
                window.name = "umm window";
                //window.normal.background = Textures.Window;
                //window.normal.background.wrapMode = TextureWrapMode.Repeat;

                h1 = new GUIStyle();
                h1.name = "umm h1";
                h1.normal.textColor = Color.white;
                h1.fontStyle = FontStyle.Bold;
                h1.alignment = TextAnchor.MiddleCenter;

                h2 = new GUIStyle();
                h2.name = "umm h2";
                h2.normal.textColor = new Color(0.6f, 0.91f, 1f);
                h2.fontStyle = FontStyle.Bold;

                bold = new GUIStyle(GUI.skin.label);
                bold.name = "umm bold";
                bold.normal.textColor = Color.white;
                bold.fontStyle = FontStyle.Bold;
            }

            private static int mLastWindowId = 0;

            public static int GetNextWindowId() {
                return ++mLastWindowId;
            }


            public static int Scale(int value) {
                return value;
            }

            private static float Scale(float value) {
                return value;
            }
        }
    }
}
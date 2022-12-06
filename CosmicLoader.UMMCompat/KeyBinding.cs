using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityModManagerNet
{
    public class KeyBinding
    {
        public KeyCode keyCode = KeyCode.None;
        public bool Ctrl
        {
            get => (modifiers & 1) == 1;
            set => modifiers = (byte)((modifiers & ~1) + (value ? 1 : 0));
        }

        public bool Shift
        {
            get => (modifiers & 2) == 2;
            set => modifiers = (byte)((modifiers & ~2) + (value ? 2 : 0));
        }

        public bool Alt
        {
            get => (modifiers & 4) == 4;
            set => modifiers = (byte)((modifiers & ~4) + (value ? 4 : 0));
        }

        public byte modifiers;

        private int m_Index = -1;
        internal int Index
        {
            get
            {
                if (m_Index == -1)
                {
                    m_Index = Array.FindIndex(KeysCode, x => x == keyCode.ToString());
                }
                if (m_Index == -1)
                {
                    Change(KeyCode.None);
                    return 0;
                }
                return m_Index;
            }
        }

        static KeyBinding()
        {
            KeysCode = EnabledKeys.Keys.Intersect(Enum.GetNames(typeof(KeyCode))).ToArray();
            KeysName = KeysCode.Select(x => EnabledKeys[x]).ToArray();
        }

        internal static string[] KeysCode;
        internal static string[] KeysName;

        public static bool CheckCtrl()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        public static bool CheckShift()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        public static bool CheckAlt()
        {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }

        public void Change(KeyCode key, bool ctrl, bool shift, bool alt)
        {
            Change(key, (byte)((ctrl ? 1 : 0) + (shift ? 2 : 0) + (alt ? 4 : 0)));
        }

        public void Change(KeyCode key, byte modifier = 0)
        {
            keyCode = key;
            modifiers = modifier;
            m_Index = -1;
        }

        public bool Pressed()
        {
            var b = keyCode != KeyCode.None && ((modifiers & 1) == 0 || CheckCtrl()) && ((modifiers & 2) == 0 || CheckShift()) && ((modifiers & 4) == 0 || CheckAlt());
            return b && Input.GetKey(keyCode);
        }

        /// <summary>
        /// Use to poll key status.
        /// </summary>
        public bool Down()
        {
            var b = keyCode != KeyCode.None && ((modifiers & 1) == 0 || CheckCtrl()) && ((modifiers & 2) == 0 || CheckShift()) && ((modifiers & 4) == 0 || CheckAlt());
            return b && Input.GetKeyDown(keyCode);
        }

        /// <summary>
        /// Use to poll key status.
        /// </summary>
        public bool Up()
        {
            var b = keyCode != KeyCode.None && ((modifiers & 1) == 0 || CheckCtrl()) && ((modifiers & 2) == 0 || CheckShift()) && ((modifiers & 4) == 0 || CheckAlt());
            return b && Input.GetKeyUp(keyCode);
        }

        private readonly static Dictionary<string, string> EnabledKeys = new Dictionary<string, string>
        {
            { "None", "None" },
            { "BackQuote", "~" },
            { "Tab", "Tab" },
            { "Space", "Space" },
            { "Return", "Enter" },

            { "Alpha0", "0" },
            { "Alpha1", "1" },
            { "Alpha2", "2" },
            { "Alpha3", "3" },
            { "Alpha4", "4" },
            { "Alpha5", "5" },
            { "Alpha6", "6" },
            { "Alpha7", "7" },
            { "Alpha8", "8" },
            { "Alpha9", "9" },
            { "Minus", "-" },
            { "Equals", "=" },
            { "Backspace", "Backspace" },

            { "F1", "F1" },
            { "F2", "F2" },
            { "F3", "F3" },
            { "F4", "F4" },
            { "F5", "F5" },
            { "F6", "F6" },
            { "F7", "F7" },
            { "F8", "F8" },
            { "F9", "F9" },
            { "F10", "F10" },
            { "F11", "F11" },
            { "F12", "F12" },

            { "A", "A" },
            { "B", "B" },
            { "C", "C" },
            { "D", "D" },
            { "E", "E" },
            { "F", "F" },
            { "G", "G" },
            { "H", "H" },
            { "I", "I" },
            { "J", "J" },
            { "K", "K" },
            { "L", "L" },
            { "M", "M" },
            { "N", "N" },
            { "O", "O" },
            { "P", "P" },
            { "Q", "Q" },
            { "R", "R" },
            { "S", "S" },
            { "T", "T" },
            { "U", "U" },
            { "V", "V" },
            { "W", "W" },
            { "X", "X" },
            { "Y", "Y" },
            { "Z", "Z" },

            { "LeftBracket", "[" },
            { "RightBracket", "]" },
            { "Semicolon", ";" },
            { "Quote", "'" },
            { "Backslash", "\\" },
            { "Comma", "," },
            { "Period", "." },
            { "Slash", "/" },

            { "Insert", "Insert" },
            { "Home", "Home" },
            { "Delete", "Delete" },
            { "End", "End" },
            { "PageUp", "Page Up" },
            { "PageDown", "Page Down" },
            { "UpArrow", "Up Arrow" },
            { "DownArrow", "Down Arrow" },
            { "RightArrow", "Right Arrow" },
            { "LeftArrow", "Left Arrow" },

            { "KeypadDivide", "Numpad /" },
            { "KeypadMultiply", "Numpad *" },
            { "KeypadMinus", "Numpad -" },
            { "KeypadPlus", "Numpad +" },
            { "KeypadEnter", "Numpad Enter" },
            { "KeypadPeriod", "Numpad Del" },
            { "Keypad0", "Numpad 0" },
            { "Keypad1", "Numpad 1" },
            { "Keypad2", "Numpad 2" },
            { "Keypad3", "Numpad 3" },
            { "Keypad4", "Numpad 4" },
            { "Keypad5", "Numpad 5" },
            { "Keypad6", "Numpad 6" },
            { "Keypad7", "Numpad 7" },
            { "Keypad8", "Numpad 8" },
            { "Keypad9", "Numpad 9" },

            { "RightShift", "Right Shift" },
            { "LeftShift", "Left Shift" },
            { "RightControl", "Right Ctrl" },
            { "LeftControl", "Left Ctrl" },
            { "RightAlt", "Right Alt" },
            { "LeftAlt", "Left Alt" },

            { "Pause", "Pause" },
            { "Escape", "Escape" },
            { "Numlock", "Num Lock" },
            { "CapsLock", "Caps Lock" },
            { "ScrollLock", "Scroll Lock" },
            { "Print", "Print Screen" },
        };
    }
}

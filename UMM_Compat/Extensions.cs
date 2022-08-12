using System;
using UnityEngine;

namespace UnityModManagerNet
{
    // Token: 0x02000010 RID: 16
    public static class Extensions
    {
        /// <summary>
        /// Renders fields with mask OnlyDrawAttr. [0.18.0]
        /// </summary>
        // Token: 0x06000057 RID: 87 RVA: 0x00005ABC File Offset: 0x00003CBC
        public static void Draw<T>(this T instance, UnityModManager.ModEntry mod) where T : class, IDrawable, new()
        {
            // TODO
            GUILayout.Label($"<color=#FFFF88>TODO:</color> <color=#BBBBBB>Draw {instance}</color>");
        }

        /// <summary>
        /// Renders fields with mask OnlyDrawAttr. [0.22.15]
        /// </summary>
        // Token: 0x06000058 RID: 88 RVA: 0x00005AD9 File Offset: 0x00003CD9
        public static void Draw<T>(this T instance, UnityModManager.ModEntry mod, int unique) where T : class, IDrawable, new() 
        {
            // TODO
            GUILayout.Label($"<color=#FFFF88>TODO:</color> <color=#BBBBBB>Draw {instance}</color>");
        }
    }
}
﻿using System;

namespace CosmicLoader.UI.UIDraw
{
    public enum DrawType
    {
        Auto,
        Ignore,
        Field,
        Slider,
        Toggle,
        ToggleGroup,
        PopupList,
        KeyBinding,
    };

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DrawAttribute : Attribute
    {
        public DrawType Type = DrawType.Auto;
        public string Label;
        public int Width = 0;
        public int Height = 0;
        public double Min = double.MinValue;
        public double Max = double.MaxValue;

        public int Precision = 2;
        public int MaxLength = int.MaxValue;
        public string VisibleOn;
        public string InvisibleOn;
        public bool Box;
        public bool Collapsible;
        public bool Vertical;

        public DrawAttribute() { }

        public DrawAttribute(string Label)
        {
            this.Label = Label;
        }

        public DrawAttribute(string Label, DrawType Type)
        {
            this.Label = Label;
            this.Type = Type;
        }

        public DrawAttribute(DrawType Type)
        {
            this.Type = Type;
        }
    }

    public static class Drawer
    {
        public static void DrawObject(object obj) { }
    }
}
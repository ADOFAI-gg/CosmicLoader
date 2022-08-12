using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityModManagerNet
{
    public partial class UnityModManager
    {
        public static class Textures
        {
            // Generates via https://www.base64-image.de/ http://angrytools.com/gradient/image/

            //private static string WindowBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAIAAAEACAYAAACZCaebAAAAnElEQVRIS63MtQHDQADAwPdEZmaG/fdJCq2g7qqLvu/7hRBCZOF9X0ILz/MQWrjvm1DHdV3MFs7zJLRwHAehhX3fCS1s20ZoYV1XQgvLshDqmOeZ2cI0TYQWxnEktDAMA6GFvu8JLXRdR2ihbVtCHU3TMFuo65rQQlVVhBbKsiS0UBQFoYU8zwktZFlGqCNNU2YLSZIQWojjmFDCH22GtZAncD8TAAAAAElFTkSuQmCC";
            public static Texture2D Window = null;

            public static void Init() {
                Window = new Texture2D(2, 2);
            }
        }
    }
}
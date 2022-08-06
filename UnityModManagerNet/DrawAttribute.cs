using System;

namespace UnityModManagerNet
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class DrawAttribute : Attribute
	{
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

		public DrawType Type;

		public string Label;

		public int Width;

		public int Height;

		public double Min = double.MinValue;

		public double Max = double.MaxValue;

		public int Precision = 2;

		public int MaxLength = int.MaxValue;

		public string VisibleOn;

		public string InvisibleOn;

		public bool Box;

		public bool Collapsible;

		public bool Vertical;
	}
}
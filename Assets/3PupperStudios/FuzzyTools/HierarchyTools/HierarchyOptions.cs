using System;
using UnityEngine;

namespace FuzzyTools
{
	[Serializable]public struct HierarchyOptions
	{
		//public Object gameObject;
		public Color backgroundColor;
		public Color fontColor;
		public FontStyle style;

		public void Set(Color background, Color font, FontStyle stylize)
		{
			backgroundColor = background;
			fontColor = font;
			style = stylize;
		}
	}
}
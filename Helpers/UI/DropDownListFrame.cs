using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;

namespace Pather.Helpers.UI
{
	public class DropDownListFrame
	{
		public static GInterfaceObject GetFrame(int num)
		{
			return GContext.Main.Interface.GetByName("DropDownList" + num);
		}

		public static GInterfaceObject GetFrame()
		{
			return GetFrame(1);
		}

		public static bool IsVisible(int num)
		{
			GInterfaceObject obj = GetFrame(num);
			if (obj != null && obj.IsVisible)
				return true;
			return false;
		}

		public static bool IsVisible()
		{
			return IsVisible(1);
		}

		public static GInterfaceObject GetButton(int frameNum, int buttonNum)
		{
			return GContext.Main.Interface.GetByName("DropDownList" + frameNum + "Button" + buttonNum);
		}

		public static GInterfaceObject GetButton(int buttonNum)
		{
			return GetButton(1, buttonNum);
		}

		public static string GetButtonText(int frameNum, int buttonNum)
		{
			GInterfaceObject obj = GetButton(frameNum, buttonNum);

			//PPather.WriteLine("GetButtonText(" + frameNum + ", " + buttonNum + "): obj=" + obj + ", visible=" + (obj != null && obj.IsVisible));

			if (obj != null && obj.IsVisible)
			{
				GInterfaceObject text = obj.GetChildObject("DropDownList" + frameNum + "Button" + buttonNum + "NormalText");

				//PPather.WriteLine("GetButtonText(" + frameNum + ", " + buttonNum + "): text=" + text);

				if (text != null)
				{
					return text.LabelText;
				}
			}
			return null;
		}

		public static string GetButtonText(int buttonNum)
		{
			return GetButtonText(1, buttonNum);
		}
	}
}

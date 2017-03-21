using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;

namespace Pather.Helpers.UI
{
	public class TrainerFrame
	{
		public static GInterfaceObject GetFrame()
		{
			return GContext.Main.Interface.GetByName("ClassTrainerFrame");
		}

		public static bool IsVisible()
		{
			GInterfaceObject obj = GetFrame();
			if (obj != null && obj.IsVisible)
				return true;
			return false;
		}

		public static int[] AvailableSkills()
		{
			List<int> options = new List<int>();
			for (int i = 1; i <= 11; i++)
			{
				GInterfaceObject btn = GContext.Main.Interface.GetByName("ClassTrainerSkill" + i);
				if (btn != null && btn.IsVisible)
				{
					GInterfaceObject textObj = btn.GetChildObject("ClassTrainerSkill" + i + "Text");
					string text = textObj.LabelText;
					//PPather.WriteLine("Item " + i + " label '" + text + "'");
					if (text.StartsWith("  "))
						options.Add(i);
				}
			}
			return options.ToArray();
		}

		public static void LearnAllSkills()
		{
			//PPather.WriteLine("------------ START TRAIN -------");
			int[] skills = AvailableSkills();
			int x = 0;
			while (x < skills.Length)
			{
				ClickSkill(skills[x]);
				Thread.Sleep(100);
				if (IsTrainEnabled())
				{
					//PPather.WriteLine("Train item " + x + " number " + skills[x]);
					ClickTrain();
					Thread.Sleep(500);
					//PPather.WriteLine("------------ REFRESH TRAIN -------");
					if (Popup.IsVisible(1))
					{
						Popup.ClickButton(1, 1);
						Thread.Sleep(500);
					}
					skills = AvailableSkills();
					x = 0;
				}
				else
					x++;
				Thread.Sleep(200);
				// Check if the frams lines are updated, if so stay on the same item

			}
		}

		public static void ClickSkill(int i)
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("ClassTrainerSkill" + i);
			if (btn != null && btn.IsVisible)
				Functions.Click(btn, false); // left click
		}


		public static void ClickTrain()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("ClassTrainerTrainButton");
			Functions.Click(btn, false);
		}

		public static bool IsTrainEnabled()
		{
			GInterfaceObject btn = GContext.Main.Interface.GetByName("ClassTrainerTrainButton");
			return btn.IsEnabledInFrame;
		}

		public static string GetSkillName(int i)
		{
			return GContext.Main.Interface.GetByName("ClassTrainerSkill" + i + "Text").LabelText;
		}
	}
}

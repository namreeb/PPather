using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;
using Pather.Tasks;
using Pather.Graph;
using Pather.Parser;
using Pather.Helpers.UI;

namespace Pather.Activities
{
	public class GTalentLocation
	{
		private int tab;
		private int spot;
	
		public GTalentLocation ( int tab, int spot )
		{
			this.tab = tab;
			this.spot = spot;
		}

		public int getTab()
		{
			return this.tab;
		}

		public int getSpot()
		{
			return this.spot;
		}
	}

	public class ActivityTalents : Activity
	{
		List<string> talents;
		private bool hasNext = true;

		public ActivityTalents(Task t, List<string> talents)
			: base(t, "Talents")
		{
			this.talents = talents;
		}

		public override Location GetLocation()
		{
			return null;
		}

		public bool HasNext()
		{
			return this.hasNext;
		}

		private GTalentLocation GetNextTalent()
		{
			string[] currentTalents = TalentFrame.GetTalentString();	
			
			foreach (string build in this.talents)
			{
				int offset = 0;
				int h = 0;
				int w = 0;

				// iterate over tabs
				for (int t = 0; t < 3; t++)
				{
					// iterate over current tree

					for (int i = 0; i < currentTalents[t].Length; i++)
					{
						w = int.Parse(build.Substring(offset + i, 1));
						h = int.Parse(currentTalents[t].Substring(i, 1));

						if ( h < w )
						{
							return new GTalentLocation(t + 1, i + 1);
						}
					}
					offset += currentTalents[t].Length;
				}
			}

			// We made it through all the available talent build steps, clearly we're done. 
			// Set someting so the Task can check this and stop
			this.hasNext = false;
			return null;
		}

		public override bool Do()
		{
			GTalentLocation nextTalent = GetNextTalent();

			if (nextTalent != null)
			{
				TalentFrame.SpendPoint(nextTalent.getTab(), nextTalent.getSpot());
			}

			return true;
		}
	}
}

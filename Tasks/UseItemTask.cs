/*
  This file is part of PPather.

	PPather is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	PPather is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have received a copy of the GNU Lesser General Public License
	along with PPather.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Glider.Common.Objects;
using Pather;
using Pather.Activities;
using Pather.Graph;
using Pather.Parser;
using Pather.Helpers.UI;

namespace Pather.Tasks
{
	class UseItemTask:  ActivityFreeTask
	{
		string vItemName = "";
		int vItemDelay = 500;
		int vTimes = 0;
		int TimesUsed = 0;

		public UseItemTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
						   
			Value vin = node.GetValueOfId("Name");
			if(vin != null)
				vItemName = vin.GetStringValue();
			Value vid = node.GetValueOfId("Delay");
			if (vid != null)
				vItemDelay = vid.GetIntValue();
			Value vt = node.GetValueOfId("Times");
			if (vt != null)
				vTimes = vt.GetIntValue();  
		}

		public override void GetParams(List<string> l)
		{
			l.Add("Name");
			l.Add("Delay");
			l.Add("Times");

			base.GetParams(l);
		}

		public override string ToString()
		{
			return "Using item: " + vItemName;
		}

		public override void Restart()
		{
			TimesUsed = 0;
		}
		public override bool IsFinished()
		{
			if (vTimes == 0) return false;
			return TimesUsed >= vTimes;
		}

		public override bool WantToDoSomething()
		{
			// hmm, check for presence of the item?!
			if (vTimes == 0) return true;
			return TimesUsed < vTimes;
		}

		public override bool DoActivity()
		{
		   BagManager bm = new BagManager();

			GItem[] items = bm.GetAllItems();
			foreach (GItem item in items)
			{
				if (item.Name == vItemName)
				{
					PPather.WriteLine("Use item " + item.Name);
					bm.ClickItem(item, true);
					Thread.Sleep(vItemDelay);
					TimesUsed++;
					return true;
				}
			}
			bm.CloseAllBags();
			bm.UpdateItems();
			return true; // done
		}

		public override bool ActivityDone(Activity task)
		{
		   
			return true;
		}
	}
}

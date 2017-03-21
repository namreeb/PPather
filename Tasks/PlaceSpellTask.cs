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

using Glider.Common.Objects;
using Pather;
using Pather.Activities;
using Pather.Graph;
using Pather.Parser;
using System.Threading;

namespace Pather.Tasks
{
	class PlaceSpellTask:ActivityFreeTask
	{
		bool done=false;

		string SpellTab="";
		string SpellName="";
		string SpellRank="";
		string TargetBar="";
		string TargetButton="";

		GInterfaceObject SpellTabObject;
		GInterfaceObject SpellTab1;
		GInterfaceObject SpellTab2;
		GInterfaceObject SpellTab3;
		GInterfaceObject SpellTab4;
		GInterfaceObject SpellBookMicroButton;
		GInterfaceObject SpellbookFrame;
		GInterfaceObject NextPageButton;
		GInterfaceObject PrevPageButton;
		GInterfaceObject TargetButtonObject;

		public PlaceSpellTask(PPather pather, NodeTask node)
			: base(pather, node) 
		{
			Value v=nodetask.GetValueOfId("SpellTab");
			if(v==null)
				this.SpellTab="";
			else
				this.SpellTab=v.GetStringValue();

			v=nodetask.GetValueOfId("SpellName");
			if(v==null)
				this.SpellName="";
			else
				this.SpellName=v.GetStringValue();

			v=nodetask.GetValueOfId("SpellRank");
			if(v==null)
				this.SpellRank="";
			else
				this.SpellRank=v.GetStringValue();

			v=nodetask.GetValueOfId("TargetBar");
			if(v==null)
				this.TargetBar="";
			else
				this.TargetBar=v.GetStringValue();

			v=nodetask.GetValueOfId("TargetButton");
			if(v==null)
				this.TargetButton="";
			else
				this.TargetButton=v.GetStringValue();
		}

		public override void GetParams(List<string> l) {
			l.Add("SpellTab");
			l.Add("SpellName");
			l.Add("SpellRank");
			l.Add("TargetBar");
			l.Add("TargetButton");
			base.GetParams(l);
		}

		public override string ToString() 
		{
			return "Place "+SpellName;
		}

		public override Location GetLocation() {
			return null;
		}

		public override void Restart() {
			done=false;
		}

		public override bool IsFinished() {
			return done; // after doing it's thing once this task is finished and will not execute again this session.
		}

		public override bool WantToDoSomething() {
			return true; // always wants to do something. This is supposed to be used inside If, When, or a Sequence. 
		}

		private void GotoFirstPage() {
			while(PrevPageButton.IsEnabledInFrame) // as long as the previous page button is clickable, click it!
			{
				if(!SpellbookFrame.IsVisible)
				Functions.Click(SpellBookMicroButton);
				Thread.Sleep(200);
				Functions.Click(PrevPageButton);
			}
		}

		private void GotoLastPage() {
			while(NextPageButton.IsEnabledInFrame) // as long as the next page button is clickable, click it!
			{
				if(!SpellbookFrame.IsVisible)
				Functions.Click(SpellBookMicroButton);
				Thread.Sleep(200);
				Functions.Click(NextPageButton);
			}
		}

		public override bool DoActivity() {

			bool AllRanks=false;
			bool AllTabs=false;
			int SearchedTab=0;
			if((SpellTab=="Unknown")||(SpellTab=="unknown"))
				AllTabs=true;
			if((SpellRank=="Any")||(SpellRank=="any"))
				AllRanks=true;

			// assigning a bunch of interface objects for later use.
			SpellBookMicroButton=GContext.Main.Interface.GetByName("SpellbookMicroButton");
			SpellbookFrame=GContext.Main.Interface.GetByName("SpellBookFrame");
			NextPageButton=GContext.Main.Interface.GetByName("SpellBookNextPageButton");
			PrevPageButton=GContext.Main.Interface.GetByName("SpellBookPrevPageButton");
			TargetButtonObject=GContext.Main.Interface.GetByName("ActionButton"+TargetButton);


			if(AllTabs)
			{
				SpellTab1=GContext.Main.Interface.GetByName("SpellBookSkillLineTab1");
				SpellTab2=GContext.Main.Interface.GetByName("SpellBookSkillLineTab2");
				SpellTab3=GContext.Main.Interface.GetByName("SpellBookSkillLineTab3");
				SpellTab4=GContext.Main.Interface.GetByName("SpellBookSkillLineTab4");
			}
			else
			{
				SpellTabObject=GContext.Main.Interface.GetByName("SpellBookSkillLineTab"+SpellTab);
			}

			// initialize the variables that will later make sure we got the correct spell.
			bool correctspell=false;
			bool correctrank=false;

			// Make sure the spellbook is open before we proceed.
			if(!SpellbookFrame.IsVisible)
				Functions.Click(SpellBookMicroButton);


			// Here I'm going to populate the list of all 12 Spellbook buttons so I can later iterate through them to find what we are looking for.
			// I removed this in favor of manually adding everything to an array in the correct order to allow searching through the spell list backwards
			// List<GInterfaceObject> SpellBookButtonsList = new List<GInterfaceObject>();
			//for (int i = 1; i <= 12; i++)
			//{
			//	SpellBookButtonsList.Add(GContext.Main.Interface.GetByName("SpellButton" + i.ToString()));
			//}

			GInterfaceObject[] SpellBookButtons=new GInterfaceObject[12];

			SpellBookButtons[0]=GContext.Main.Interface.GetByName("SpellButton12");
			SpellBookButtons[1]=GContext.Main.Interface.GetByName("SpellButton10");
			SpellBookButtons[2]=GContext.Main.Interface.GetByName("SpellButton8");
			SpellBookButtons[3]=GContext.Main.Interface.GetByName("SpellButton6");
			SpellBookButtons[4]=GContext.Main.Interface.GetByName("SpellButton4");
			SpellBookButtons[5]=GContext.Main.Interface.GetByName("SpellButton2");
			SpellBookButtons[6]=GContext.Main.Interface.GetByName("SpellButton11");
			SpellBookButtons[7]=GContext.Main.Interface.GetByName("SpellButton9");
			SpellBookButtons[8]=GContext.Main.Interface.GetByName("SpellButton7");
			SpellBookButtons[9]=GContext.Main.Interface.GetByName("SpellButton5");
			SpellBookButtons[10]=GContext.Main.Interface.GetByName("SpellButton3");
			SpellBookButtons[11]=GContext.Main.Interface.GetByName("SpellButton1");

			// now copy the whole thing into an array.


			Thread.Sleep(500);

			// Making sure that we are on the right spellbook tab
			if(AllTabs)
				Functions.Click(SpellTab1);
			else
				Functions.Click(SpellTabObject);


			// Now that we're on the right tab, we gotta make sure that we're on the first page too.
			GotoLastPage();

			// Make sure we got the right action bar selected
			GContext.Main.SendKey("Common.Bar"+TargetBar);

			// Alright, now it gets interesting. Make sure the spellbook frame is open.
			if(!SpellbookFrame.IsVisible)
				Functions.Click(SpellBookMicroButton);

			Thread.Sleep(500);

			SpellSearch:
			if(!SpellbookFrame.IsVisible)
				Functions.Click(SpellBookMicroButton);
			foreach(GInterfaceObject button in SpellBookButtons)
			{
				foreach(GInterfaceObject child in button.Children)
				{
					if(child.LabelText==SpellName)
					{
						correctspell=true; // found the right spell name
					}
					if((child.LabelText==SpellRank)||AllRanks)
					{
						correctrank=true; // found the right spell rank, or all ranks are fine
					}
					if(correctrank&&correctspell) // only go here if we found a button where both spell name and rank match
					{
						Thread.Sleep(500);
						GContext.Main.EnableCursorHook();
						button.BeginDrag(true);
						Thread.Sleep(250);
						Functions.Hover(TargetButtonObject);
						Thread.Sleep(250);
						TargetButtonObject.EndDrag(false);
						GContext.Main.DisableCursorHook(); // i know it defeats the purpose of the functions for hover but dont have time to make a new function for begindrag

						correctrank=false;
						correctspell=false;

						if(SpellbookFrame.IsVisible)
							Functions.Click(SpellBookMicroButton); // close the spellbook if it's open (and it most likely is)

						if(GContext.Main.Interface.CursorItemType!=GCursorItemTypes.None)
							Functions.Click(TargetButtonObject);

						PPather.WriteLine("Successfully placed "+SpellName+" on action bar "+TargetBar+", button "+TargetButton+".");

						return true; // we stop here if we found the right button
					}
				}

				correctrank=false; // after checking all children of a button these variables are reset
				correctspell=false; // this is required for the spell/rank matching to work properly
			}


			if(PrevPageButton.IsEnabledInFrame)
			{
				if(!SpellbookFrame.IsVisible)
					Functions.Click(SpellBookMicroButton);

				Functions.Click(PrevPageButton);
				goto SpellSearch; // if the previous page button is clickable and we didn't find the right skill yet then go to the previous page and restart the search
			}

			if(AllTabs)
			{
				SearchedTab++;
				switch(SearchedTab)
				{
					case 1:
					if(SpellTab2.IsVisible)
						Functions.Click(SpellTab2);
					else
						goto PlaceSpellFailed;
					Thread.Sleep(200);
					GotoLastPage();
					Thread.Sleep(200);
					goto SpellSearch;
					case 2:
					if(SpellTab3.IsVisible)
						Functions.Click(SpellTab3);
					else
						goto PlaceSpellFailed;
					Thread.Sleep(200);
					GotoLastPage();
					Thread.Sleep(200);
					goto SpellSearch;
					case 3:
					if(SpellTab4.IsVisible)
						Functions.Click(SpellTab4);
					else
						goto PlaceSpellFailed;
					Thread.Sleep(200);
					GotoLastPage();
					Thread.Sleep(200);
					goto SpellSearch;
					case 4:
					goto PlaceSpellFailed;
				}

			}

			PlaceSpellFailed:
			PPather.WriteLine("PlaceSpell was unable to find the specified spell... giving up.");
			if(SpellbookFrame.IsVisible)
				Functions.Click(SpellBookMicroButton); // make sure the Spellbook is closed
			done=true;
			return false; // If we get here then something went wrong :-(
		}

		public override bool ActivityDone(Activity task) {
			done=true;
			return true;
		}
	}
}
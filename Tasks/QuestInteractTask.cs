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
using Pather.Helpers;

namespace Pather.Tasks
{
	public abstract class QuestInteractTask : ParserTask
	{
		protected string Object;
		protected string NPC;
		protected string Item;
		protected const float InteractDistance = 2.5f;
		protected Location location = null;
		bool UseMount;

		public QuestInteractTask(PPather pather, NodeTask node)
			: base(pather, node)
		{
			Object = node.GetValueOfId("Object").GetStringValue();
			NPC = node.GetValueOfId("NPC").GetStringValue();
			Item = node.GetValueOfId("Item").GetStringValue();
			location = node.GetValueOfId("Location").GetLocationValue();
			if (location == null && Item == "")
			{
				if (NPC != "")
				{
					PPather.WriteLine("!Warning:No $Location for $NPC = " + NPC);
				}
				else if (Object != "")
				{
					PPather.WriteLine("!Warning:No $Location for $Object = " + Object);
				}
				else
				{
					PPather.WriteLine("!Warning:$Item, $NPC ,$Object and $Location isn't set");
				}
			}
			UseMount = node.GetBoolValueOfId("UseMount");
		}


		public override void GetParams(List<string> l)
		{
			l.Add("Object");
			l.Add("NPC");
			l.Add("Item");
			l.Add("Location");
			base.GetParams(l);
		}

		private GUnit FindNPC()
		{
			return GObjectList.FindUnit(NPC);
		}

		private GNode FindNode()
		{
			GNode[] nodes = GObjectList.GetNodes();
			GNode node = null;

			double bestDistanceToLoc = double.MaxValue;

			foreach (GNode n in nodes)
			{
				if (n.Name == Object)
				{
					if (location != null) //this is cause sometimes there are more than 1 object visible, like "Wanted: Poster", if there are more than one, choose the one closest to the location specified.
					{
						float distance = location.GetDistanceTo(new Location(n.Location));
						if (distance < bestDistanceToLoc)
						{
							bestDistanceToLoc = distance;
							node = n;
						}
					}
					else
					{
						node = n;
						break;
					}

				}
			}
			return node;
		}

		private GItem FindItem()
		{
			GItem[] items = GObjectList.GetItems();
			foreach (GItem i in items)
			{
				if (i.Name == Item)
				{
					return i;
				}
			}

			return null;
		}


		public GObject FindObject()
		{
			if (NPC.Length > 0)
			{
				GUnit npc = FindNPC();
				if (npc != null)
				{
					return npc;
				}
			}
			else if (Object.Length > 0)
			{
				GNode node = FindNode();
				if (node != null)
				{
					return node;
				}
			}
			else if (Item.Length > 0)
			{
				GItem item = FindItem();
				if (item != null)
				{
					return item;
				}
			}

			return null;

		}




		public override Location GetLocation()
		{
			if (Item != "")
			{

				return null; //anywhere, you carry the item :)
			}

			GObject obj = FindObject();
			if (obj != null)
			{
				return new Location(obj.Location);
			}
			else
			{
				if (location == null)
				{
					return ppather.FindNPCLocation(NPC);
				}
				else
				{
					return location;
				}
			}
		}

		public bool CanSeeObject()
		{
			return FindObject() != null;
		}

		public bool IsCloseToObject()
		{
			if (!CanSeeObject())
			{
				return false;
			}
			Location objl = GetLocation();
			if (objl == null)
			{
				return true; // should be an item
			}
			Location mel = new Location(GContext.Main.Me.Location);

			float d = mel.GetDistanceTo(objl);
			if (d < InteractDistance && Math.Abs(mel.Z - objl.Z) < 2.0)
				return true;
			return false;
		}

		public Location GetLocationOfObject()
		{
			GObject obj = FindObject();
			if (obj != null)
				return new Location(obj.Location);

			return location;
		}

		protected ActivityWalkTo walkToTask;
		protected ActivityApproach approachTask;

		string prevObj = "";
		public Activity GetWalkToActivity()
		{

			GObject obj = FindObject();
			Location l = null;
			if (obj == null || obj.Type == GObjectType.Node)
			{
				if (obj == null)
				{
					l = GetLocationOfObject();
				}
				else
				{
					l = new Location(obj.Location);
				}
				if (walkToTask == null || Object != prevObj)
				{
					walkToTask = new ActivityWalkTo(this, l, InteractDistance, UseMount);
				}
				prevObj = Object;
				return walkToTask;
			}
			else if (obj.Type == GObjectType.Monster)
			{
				if (approachTask == null || NPC != prevObj)
					approachTask = new ActivityApproach(this, (GUnit)obj, InteractDistance, UseMount);
				prevObj = NPC;
				return approachTask;
			}
			else
			{
				return null; // /cry
			}
		}
	}
}


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

using Pather.Helpers.UI;
using Glider.Common.Objects;

namespace Pather
{
    public class PathObject
    {
        private GUnit unit;
        private GNode node;
        private GItem item;

        public PathObject(GObject obj)
        {
            if (obj.Type == GObjectType.Monster)
            {
                unit = (GUnit)obj;
            }
            else if (obj.Type == GObjectType.Node)
            {
                node = (GNode)obj;
            }
            else if (obj.Type == GObjectType.Item)
            {
                item = (GItem)obj;
            } else 
            { 
                throw new Exception("couldn't initialize object " + obj.Name + " to GUnit, GNode or GItem");
            }
        }

        public double getDistanceToSelf()
        {
            if (isUnit())
            {
                return unit.DistanceToSelf;
            }
            else if (isNode())
            {
                return node.DistanceToSelf;
            }
            else
            {
                return 0; //item
            }
        }

        public void Interact()
        {
            if (isUnit())
            {
				Functions.Interact(unit);
            }
            else if (isNode())
            {
				Functions.Interact(node);
            }
            else //item interaction means click the item
            {
                BagManager bm = new BagManager();
                bm.UpdateItems();
                bm.ClickItem(item, true);
            }
        }


        public GLocation getLocation()
        {
            if (isUnit())
            {
                return unit.Location;
            }
            else if (isNode())
            {
                return node.Location;
            }
            else //item
            {
                return null;
            }
        }

        public bool isUnit()
        {
            return unit != null;
        }

        public bool isNode()
        {
            return node != null;
        }

        public bool isItem()
        {
            return item != null;
        }


        public GUnit getUnit()
        {
            return unit;
        }

        public GNode getNode()
        {
            return node;
        }

        public GItem getItem()
        {
            return item;
        }

    }
}

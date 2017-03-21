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
using System.Globalization;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Text;
using Glider.Common.Objects;

using Pather;
using Pather.Activities;
using Pather.Parser;
using Pather.Graph;

namespace Pather.Tasks
{
	/*
	 * Design notes:    
	 * 
	 *  GetLocation
	 *  WantToDoSomething
	 *  IsFinished
	 *  
	 *  GetActivity
	 *  ActivityDone
	 */

	public abstract class Task
	{
		public enum State_e { Idle, Want, Active, Done };
		public bool isActive;

		public Task parent;

		public virtual bool IsParserTask() { return false; }
		public virtual State_e State
		{
			get
			{
				if (isActive) return State_e.Active;
				if (IsFinished()) return State_e.Done;
				return State_e.Idle;
			}
		}

		public PPather ppather;
		public Task(PPather pather)
		{
			ppather = pather;
		}

		public abstract Location GetLocation();

		public virtual void Restart()
		{

		}

		public virtual Task[] GetChildren()
		{
			return null;
		}

		public abstract bool WantToDoSomething();
		public abstract bool IsFinished();

		public abstract Activity GetActivity();
		public abstract bool ActivityDone(Activity task); // called when activity is done

		public virtual void GetParams(List<string> l) { }

	}
}
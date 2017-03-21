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
using System.Threading;
using System.Text;

using Glider.Common.Objects;
using Pather;
using Pather.Tasks;
using Pather.Graph;
using Pather.Parser;

namespace Pather.Activities
{ 
    public abstract class Activity
    {
        public string name;
        public PPather ppather;
        public Task task; // Who produced me

        public Activity(Task task, String name)
        {
            this.name = name;
            this.task = task;
            ppather = task.ppather;
        }

        public override string ToString()
        {
            return name;
        }
        public abstract Location GetLocation(); // return the target location for this task
        public virtual void Start() { } // Task selected for execution
        public abstract bool Do();    // Called periodically when this task is activated, return true if task is done
        public virtual void Stop() { }  // 
    }
}
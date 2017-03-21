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
using System.Reflection;

using Glider.Common.Objects;
using Pather;
using Pather.Activities;
using Pather.Graph;
using Pather.Parser;

namespace Pather.Tasks {
	public abstract class ParserTask : Task {
		// this stuff dynamically gets a task object using reflection
		// so that we don't have to hard code in the different task
		// types in PPather.CreateTaskFromNode()

		// public just for debug, SHOULD BE PRIVATE
		public static Dictionary<string, Type> registeredTasks = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

		public static void RegisterTask(string taskName, Type classType) {
			registeredTasks[taskName] = classType;

			//		PPather.WriteLine("Registered task: " + taskName + " -> " + classType);
			//		MessageBox.Show("Registered task: " + taskName + " -> " + classType.FullName);
		}

		public static ParserTask GetTask(PPather ppather, NodeTask node) {
			//		string s = "";

			//		foreach (string key in registeredTasks.Keys)
			//			s += key + ",";

			//		PPather.WriteLine("Types: " + s);

			if (!registeredTasks.ContainsKey(node.type)) {
				PPather.WriteLine("No registered task for " + node.type);
				return null;
			}

			//PPather.WriteLine("Attempting to instantiate " + registeredTasks[node.type]);
			// (In some big task files, the above line can crash Glider)

			ConstructorInfo ci = null;
			Object o = null;

			try {
				ci = registeredTasks[node.type].GetConstructor(new Type[] { ppather.GetType(), node.GetType() });
				o = ci.Invoke(new object[] { ppather, node });
			} catch (Exception e) {
				// an exception here is fatal, popup a dialog
				// to give some feedback and rethrow the error
				string s = "Error instantiating \"" + node.type + "\" task\n\n" + e.GetType().FullName + "\n" + e.Message;

				if (null != e.InnerException) {
					s += "\n\nInner exception:" + e.InnerException.GetType().FullName + "\n" + e.InnerException.Message;
				}

				System.Windows.Forms.MessageBox.Show(s);
				throw e;
			}

			if (null == o) {
				PPather.WriteLine("Created null");
			}

			return (ParserTask)o;
		}


		public NodeTask nodetask;
		public ParserTask(PPather pather, NodeTask nodetask)
			: base(pather) {
			this.nodetask = nodetask;
		}

		public override bool IsParserTask() { return true; }

		public virtual void Unload()
		{
			
			/*
			if (this.IsFinished() && !this.isActive)
			{
				lock (this)
				{
					PPather.WriteLine("Unloading Task: " + this.nodetask.name);
					this.nodetask.parent.subTasks.Remove(this.nodetask);
					PPather.WriteLine("Unloaded Task: " + this.nodetask.name);
				}
			}
			 */
		}
	}
}

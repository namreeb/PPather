using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser
{
	public abstract class FuncDefinition
	{
		public abstract bool IsNamed(string s);
		public abstract Value call(List<Value> parms);
	}
}

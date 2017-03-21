using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser
{
	public class NegExpression : NodeExpression
	{
		NodeExpression e;

		public NegExpression(NodeTask task, NodeExpression e)
			: base(task)
		{
			this.e = e;
		}

		public override Value GetValue()
		{
			Value v = e.GetValue();

			if (v.IsInt())
			{
				int i = -v.GetIntValue();
				v = new Value(i.ToString());
			}
			else if (v.IsFloat())
			{
				float f = -v.GetFloatValue();
				v = new Value(f.ToString());
			}
			else
			{
				Error("Negating non numerical value " + v);
			}

			return v;
		}

		public override void dump(int d)
		{
			Console.Write("-");
			e.dump(d);
		}

		public override bool BindSymbols()
		{
			return e.BindSymbols();
		}
	}
}

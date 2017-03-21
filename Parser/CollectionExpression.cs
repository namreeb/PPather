using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser
{
	public class CollectionExpression : NodeExpression
	{
		List<NodeExpression> expressions;

		public CollectionExpression(NodeTask task, List<NodeExpression> expressions)
			: base(task)
		{
			this.expressions = expressions;
		}

		public override Value GetValue()
		{
			List<Value> vals = new List<Value>();

			foreach (NodeExpression e in expressions)
			{
				vals.Add(e.GetValue());
			}

			return new Value(vals);
		}

		public override void dump(int d)
		{
			Console.Write("[");

			foreach (NodeExpression e in expressions)
			{
				e.dump(d);
				Console.Write(", ");
			}

			Console.Write("]");
		}

		public override bool BindSymbols()
		{
			bool ok = true;

			foreach (NodeExpression e in expressions)
			{
				ok &= e.BindSymbols();
			}

			return ok;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser
{
	public class IDExpression : NodeExpression
	{
		string id;
		NodeExpression expression;

		public IDExpression(NodeTask task, string id)
			: base(task)
		{
			this.id = id;
		}

		public override Value GetValue()
		{
			if (expression != null)
			{
				// know where it is
				expression.GetValue();
			}

			return task.GetValueOfId(id);
		}

		public override void dump(int d)
		{
			Console.Write("$" + id);
		}

		public override bool BindSymbols()
		{
			expression = task.GetExpressionOfId(id);
			return expression != null;
		}
	}
}

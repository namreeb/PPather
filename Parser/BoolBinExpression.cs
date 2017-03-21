using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser
{
	public abstract class BoolBinExpression : BinExpresssion
	{
		public BoolBinExpression(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1)
		{
		}

		public override Value GetValue()
		{
			Value val0 = left.GetValue();
			Value val1 = right.GetValue();

			if (val0 == null || val1 == null)
				return null;

			bool c0 = val0.GetBoolValue();
			bool c1 = val1.GetBoolValue();

			bool res = BoolOp(c0, c1);

			return res ? Value.TrueValue : Value.FalseValue;
		}

		public abstract bool BoolOp(bool a, bool b);
	}
}

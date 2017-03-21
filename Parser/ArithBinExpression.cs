using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser
{
	public abstract class ArithBinExpression : BinExpresssion
	{
		public ArithBinExpression(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1)
		{
		}

		public override Value GetValue()
		{
			Value val0 = left.GetValue();
			Value val1 = right.GetValue();

			if (val0.IsInt() && val1.IsInt())
			{
				int c0 = val0.GetIntValue();
				int c1 = val1.GetIntValue();
				return new Value(IntOp(c0, c1).ToString());

			}

			if ((val0.IsFloat() && val1.IsFloat()) ||
				(val0.IsInt() && val1.IsFloat()) ||
				(val0.IsFloat() && val1.IsInt()))
			{
				float c0 = val0.GetFloatValue();
				float c1 = val1.GetFloatValue();
				float sum = c0 + c1;
				return new Value(FloatOp(c0, c1).ToString());
			}

			return GenericOp(val0, val1);
		}

		public abstract float FloatOp(float a, float b);
		public abstract int IntOp(int a, int b);

		public virtual Value GenericOp(Value a, Value b)
		{
			Error("op " + this + " can not handle " + a + " and " + b);
			return null;
		}
	}
}

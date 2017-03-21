using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser {
	public abstract class CmpExpression : BinExpresssion {
		public CmpExpression(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1) { }

		public override Value GetValue() {
			Value val0 = left.GetValue();
			Value val1 = right.GetValue();

			if (val0 == null || val1 == null)
				return Value.FalseValue;

			if (val0.IsInt() && val1.IsInt()) {
				int c0 = val0.GetIntValue();
				int c1 = val1.GetIntValue();
				bool v = IntOp(c0, c1);

				return v ? Value.TrueValue : Value.FalseValue;
			}

			if ((val0.IsFloat() && val1.IsFloat()) ||
				(val0.IsInt() && val1.IsFloat()) ||
				(val0.IsFloat() && val1.IsInt())) {
				float c0 = val0.GetFloatValue();
				float c1 = val1.GetFloatValue();
				bool v = FloatOp(c0, c1);

				return v ? Value.TrueValue : Value.FalseValue;
			}

			bool sv = StringOp(val0.GetStringValue(), val1.GetStringValue());

			return sv ? Value.TrueValue : Value.FalseValue;
		}

		public abstract bool FloatOp(float a, float b);
		public abstract bool IntOp(int a, int b);
		public abstract bool StringOp(string a, string b);
	}
}

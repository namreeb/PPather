using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser
{
	public class ExprCmpGt : CmpExpression
	{
		public ExprCmpGt(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1)
		{
		}
		public override bool FloatOp(float a, float b)
		{
			return a > b;
		}
		public override bool IntOp(int a, int b)
		{
			return a > b;
		}
		public override bool StringOp(string a, string b)
		{
			return a.CompareTo(b) > 0;
		}
		public override string OpName()
		{
			return ">";
		}
	}

	public class ExprCmpGe : CmpExpression
	{
		public ExprCmpGe(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1)
		{
		}
		public override bool FloatOp(float a, float b)
		{
			return a >= b;
		}
		public override bool IntOp(int a, int b)
		{
			return a >= b;
		}
		public override bool StringOp(string a, string b)
		{
			return a.CompareTo(b) >= 0;
		}
		public override string OpName()
		{
			return ">=";
		}
	}

	public class ExprCmpEq : CmpExpression
	{
		public ExprCmpEq(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1)
		{
		}
		public override bool FloatOp(float a, float b)
		{
			return a == b;
		}
		public override bool IntOp(int a, int b)
		{
			return a == b;
		}
		public override bool StringOp(string a, string b)
		{
			return a == b;
		}
		public override string OpName()
		{
			return "==";
		}
	}

	public class ExprCmpLe : CmpExpression
	{
		public ExprCmpLe(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1)
		{
		}
		public override bool FloatOp(float a, float b)
		{
			return a <= b;
		}
		public override bool IntOp(int a, int b)
		{
			return a <= b;
		}
		public override bool StringOp(string a, string b)
		{
			return a.CompareTo(b) <= 0;
		}
		public override string OpName()
		{
			return "<=";
		}
	}

	public class ExprCmpLt : CmpExpression
	{
		public ExprCmpLt(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1)
		{
		}
		public override bool FloatOp(float a, float b)
		{
			return a < b;
		}
		public override bool IntOp(int a, int b)
		{
			return a < b;
		}
		public override bool StringOp(string a, string b)
		{
			return a.CompareTo(b) < 0;
		}
		public override string OpName()
		{
			return "<";
		}
	}

	public class ExprCmpNe : CmpExpression
	{
		public ExprCmpNe(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1)
		{
		}
		public override bool FloatOp(float a, float b)
		{
			return a != b;
		}
		public override bool IntOp(int a, int b)
		{
			return a != b;
		}
		public override bool StringOp(string a, string b)
		{
			return a.CompareTo(b) != 0;
		}
		public override string OpName()
		{
			return "!=";
		}
	}
}

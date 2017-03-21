using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser
{
	public class ExprAdd : ArithBinExpression
	{
		public ExprAdd(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1)
		{
		}

		public override float FloatOp(float a, float b)
		{
			return a + b;
		}

		public override int IntOp(int a, int b)
		{
			return a + b;
		}

		public override string OpName()
		{
			return "+";
		}
	}

	public class ExprDiv : ArithBinExpression
	{
		public ExprDiv(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1)
		{
		}

		public override float FloatOp(float a, float b)
		{
			return a / b;
		}

		public override int IntOp(int a, int b)
		{
			return a / b;
		}

		public override string OpName()
		{
			return "/";
		}
	}

	public class ExprMul : ArithBinExpression
	{
		public ExprMul(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1)
		{
		}

		public override float FloatOp(float a, float b)
		{
			return a * b;
		}

		public override int IntOp(int a, int b)
		{
			return a * b;
		}

		public override string OpName()
		{
			return "*";
		}
	}

	public class ExprSub : ArithBinExpression
	{
		public ExprSub(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1)
		{
		}

		public override float FloatOp(float a, float b)
		{
			return a - b;
		}

		public override int IntOp(int a, int b)
		{
			return a - b;
		}

		public override string OpName()
		{
			return "-";
		}
	}

	public class ExprMod : ArithBinExpression
	{
		public ExprMod(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1)
		{
		}

		public override float FloatOp(float a, float b)
		{
			return a % b;
		}

		public override int IntOp(int a, int b)
		{
			return a % b;
		}

		public override string OpName()
		{
			return "%";
		}
	}
}

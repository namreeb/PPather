using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser {
	public class ExprAnd : BoolBinExpression {
		public ExprAnd(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1) { }

		public override bool BoolOp(bool a, bool b) {
			return a && b;
		}

		public override string OpName() {
			return "&&";
		}
	}

	public class ExprOr : BoolBinExpression {
		public ExprOr(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1) { }

		public override bool BoolOp(bool a, bool b) {
			return a || b;
		}

		public override string OpName() {
			return "||";
		}
	}
}

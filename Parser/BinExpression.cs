using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser {
	public abstract class BinExpresssion : NodeExpression {
		protected NodeExpression left;
		protected NodeExpression right;

		public BinExpresssion(NodeTask task, NodeExpression left, NodeExpression right)
			: base(task) {
			this.left = left;
			this.right = right;
		}

		public abstract string OpName();

		public override void dump(int d) {
			Console.Write("(");
			left.dump(d);
			Console.Write(OpName());
			right.dump(d);
			Console.Write(")");
		}

		public override bool BindSymbols() {
			return left.BindSymbols() && right.BindSymbols();
		}
	}
}

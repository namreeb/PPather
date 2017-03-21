using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser {
	public class LiteralExpression : NodeExpression {
		Value val;

		public LiteralExpression(NodeTask task, string val)
			: base(task) {
			this.val = new Value(val);
		}

		public LiteralExpression(NodeTask task, Value val)
			: base(task) {
			this.val = val;
		}

		public override Value GetValue() {
			return val;
		}

		public override void dump(int d) {
			Console.Write(val);
		}

		public override bool BindSymbols() {
			return true;
		}
	}
}

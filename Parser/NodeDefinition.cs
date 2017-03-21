using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser {
	public class NodeDefinition : ASTNode {
		NodeTask task;
		string name;
		NodeExpression expression;

		public NodeDefinition(NodeTask task, string name, NodeExpression expression) {
			this.task = task;
			this.name = name;
			this.expression = expression;
		}

		public bool IsNamed(string s) {
			return string.Compare(s, name) == 0;
		}

		public NodeExpression GetExpression() {
			return expression;
		}

		public override void dump(int d) {
			Console.Write(prefix(d) + "$" + name + " = ");
			expression.dump(d);
			Console.WriteLine(";");
		}
	}
}

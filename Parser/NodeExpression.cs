using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser {
	public abstract class NodeExpression : ASTNode {
		protected NodeTask task;

		public NodeExpression(NodeTask task) {
			this.task = task;
		}

		public abstract Value GetValue();

		public abstract bool BindSymbols();
	}
}

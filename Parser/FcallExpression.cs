using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser {
	public class FcallExpression : NodeExpression {
		string id;
		List<NodeExpression> parms;

		public FcallExpression(NodeTask task, string id, List<NodeExpression> parms)
			: base(task) {
			this.id = id;
			this.parms = parms;
		}

		public override Value GetValue() {
			List<Value> vals = new List<Value>(parms.Count);

			foreach (NodeExpression e in parms) {
				Value v = e.GetValue();
				if (v == null) return null;
				vals.Add(v);
			}

			return task.GetValueOfFcall(id, vals);
		}

		public override void dump(int d) {
			Console.Write("call " + id + "(");

			foreach (NodeExpression e in parms) {
				e.dump(d);
				Console.Write(", ");
			}

			Console.Write(")");
		}

		public override bool BindSymbols() {
			bool ok = true;

			foreach (NodeExpression e in parms) {
				ok &= e.BindSymbols();
			}

			// TODO check fcall name
			return ok;
		}
	}
}

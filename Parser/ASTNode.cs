using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser {
	public abstract class ASTNode {
		public void Error(string err) {
			Console.WriteLine("!Error:" + err);
		}

		public abstract void dump(int d);

		public string prefix(int d) {
			String s = "";

			while (d-- > 0)
				s += "  ";

			return s;
		}
	}
}

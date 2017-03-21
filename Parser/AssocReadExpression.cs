using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser
{
	public class AssocReadExpression : BinExpresssion
	{
		public AssocReadExpression(NodeTask task, NodeExpression a0, NodeExpression a1)
			: base(task, a0, a1)
		{
		}

		public override Value GetValue()
		{
			Value assoc = left.GetValue();
			Value key = right.GetValue();

			if (assoc == null || key == null)
				return null;

			String key_s = key.GetStringValue();
			Value val = assoc.GetAssocValue(key_s);
			//PPather.WriteLine("lookup " + key + " in "+ assoc + " found "+ val);

			return val;
		}

		public override string OpName()
		{
			return null;
		} // not used

		public override void dump(int d)
		{
			left.dump(d);
			Console.Write("{");
			right.dump(d);
			Console.Write("}");
		}
	}
}

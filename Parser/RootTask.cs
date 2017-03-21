using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser {
	class RootTask : NodeTask {
		public RootTask()
			: base(null) {
		}

		public override Value GetValueOfId(string def) {
			Error("Someone is getting value of " + def);
			return null;
		}
	}
}

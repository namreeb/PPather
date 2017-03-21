using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser {
	class Tokenizer {
		public int line;
		System.IO.TextReader reader;

		public Tokenizer(System.IO.TextReader reader) {
			this.reader = reader;
			line = 1;
		}

		Token current = null;

		public Token Peek() {
			if (current == null)
				current = Next();
			return current;
		}

		public Token Next() {
			if (current != null) {
				Token t = current;
				current = null;
				return t;
			}

			Token r = null;

			do {
				r = new Token(reader);
				if (r.type == Token.Type.Newline) line++;
			} while (r.type == Token.Type.Newline);

			return r;
		}
	}
}

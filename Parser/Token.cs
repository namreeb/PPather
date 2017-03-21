using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser {
	class Token {
		public enum Type { ID, Literal, Newline, Keyword, EOF };
		public Type type;
		public string val;

		public Token(string s) {
			if (s[0] == '$') {
				type = Type.ID;
				val = s.Substring(1);
			} else {
				type = Type.Literal;
				val = s;
			}
		}

		private bool IsIDChar(char c) {
			if ((c >= 'a' && c <= 'z') ||
			   (c >= 'A' && c <= 'Z') ||
			   (c >= '0' && c <= '9') ||
			   c == '.' ||
			   c == '_'
			   )
				return true;
			return false;
		}

		public Token(System.IO.TextReader reader) {
			type = Type.EOF;

			try {
				bool done = false;
				bool foundNonWhite = false;

				do {
					int c = reader.Peek();

					if (c == -1) break;

					if (!foundNonWhite && Char.IsWhiteSpace((char)c)) {
						reader.Read();

						if ((char)c == '\n') {
							type = Type.Newline;
							return;
						}
					} else {
						foundNonWhite = true;

						if (c == '$') {
							reader.Read(); // chomp $
							readLiteral(reader);
							type = Type.ID;
							return;
						}

						if (c == '"') {
							readString(reader);
							return;
						} else if (IsKeyChar(c)) {
							if (readKeyWord(reader)) {
								// a comment, keep on reading
								foundNonWhite = false;
								type = Type.Newline;
							}

							return;
						} else {
							readLiteral(reader);
							type = Type.Literal;
							return;
						}
					}
				} while (!done);
			} catch (ObjectDisposedException) {
				type = Type.EOF;
			} catch (System.IO.IOException) {
				type = Type.EOF;
			}

			type = Type.EOF;
		}


		private bool IsKeyChar(int c) {
			if (c == '+') return true;
			if (c == '-') return true;
			if (c == '*') return true;
			if (c == '/') return true;
			if (c == '%') return true;
			if (c == '>') return true;
			if (c == '=') return true;
			if (c == '!') return true;
			if (c == '<') return true;
			if (c == '{') return true;
			if (c == '}') return true;
			if (c == ':') return true;
			if (c == ';') return true;
			if (c == '(') return true;
			if (c == ')') return true;
			if (c == ',') return true;
			if (c == ']') return true;
			if (c == '[') return true;
			if (c == '&') return true;
			if (c == '|') return true;
			if (c == '^') return true;
			return false;
		}

		// return true if it is a comment
		private bool readKeyWord(System.IO.TextReader reader) {
			int i = reader.Read();
			char c = (char)i;
			type = Type.Keyword;
			// some are 2 chars
			char c2 = (char)reader.Peek();

			if (c == '>' && c2 == '=') {
				reader.Read();
				val = "" + c + c2;
			} else if (c == '<' && c2 == '=') {
				reader.Read();
				val = "" + c + c2;
			} else if (c == '=' && c2 == '=') {
				reader.Read();
				val = "" + c + c2;
			} else if (c == '!' && c2 == '=') {
				reader.Read();
				val = "" + c + c2;
			} else if (c == '&' && c2 == '&') {
				reader.Read();
				val = "" + c + c2;
			} else if (c == '|' && c2 == '|') {
				reader.Read();
				val = "" + c + c2;
			} else if (c == '/' && c2 == '/') {
				reader.ReadLine();
				return true;
			} else
				val = "" + c;
			return false;
		}

		private void readString(System.IO.TextReader reader) {
			val = "";
			type = Type.Literal;
			int c = reader.Read(); // read "
			bool esc = false;
			do {
				c = reader.Read();
				if (c == '\\' && !esc) {
					esc = true;
				} else {
					val += (char)c;
					esc = false;
				}
				c = reader.Peek();
			} while (c != -1 && (c != '"' || esc == true));
			if (c == '"')
				reader.Read(); // read "
		}

		private void readLiteral(System.IO.TextReader reader) {
			val = "";
			type = Type.Literal;
			int c = -1;
			do {
				c = reader.Read();
				val += (char)c;
				c = reader.Peek();
			} while (IsIDChar((char)c) && c != -1);
		}
	}
}

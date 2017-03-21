using System;
using System.Collections.Generic;
using System.Text;

using Glider.Common.Objects;

namespace Pather.Parser {
	public class TaskParser {
		Tokenizer tn;

		public TaskParser(System.IO.TextReader reader) {
			tn = new Tokenizer(reader);
		}

		public void Error(string msg) {
			string s = "Line " + tn.line + ": " + msg;
			PPather.WriteLine("Line " + tn.line + ": " + msg);

			System.Windows.Forms.MessageBox.Show(s, "Error parsing task file", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
		}

		private bool Expect(Token.Type type, string val) {
			Token t = tn.Next();

			if (t.type != type || t.val != val) {
				Error("Expected " + val + " found " + t.val);
				return false;
			}

			return true;
		}

		private NodeExpression ParseExpressionP(NodeTask t) {
			Token next = tn.Peek();

			if (next.type == Token.Type.Literal) {
				// literal expression
				tn.Next();
				Token lpar = tn.Peek();

				if (lpar.type == Token.Type.Keyword && lpar.val == "(") {
					//fcall

					List<NodeExpression> exprs = new List<NodeExpression>();
					Token comma = null;

					do {
						tn.Next(); // eat ( or , 
						NodeExpression n = ParseExpression(t);
						exprs.Add(n);
						comma = tn.Peek();
					} while (comma.type == Token.Type.Keyword && comma.val == ",");

					Expect(Token.Type.Keyword, ")");

					NodeExpression e = new FcallExpression(t, next.val, exprs);
					return e;
				} else {
					NodeExpression e = new LiteralExpression(t, next.val);
					return e;
				}
			} else if (next.type == Token.Type.ID) {
				// ID expression
				tn.Next();

				Token lpar = tn.Peek();

				// assoc lookup
				if (lpar.type == Token.Type.Keyword && lpar.val == "{") {
					tn.Next(); // eat {
					NodeExpression id = new IDExpression(t, next.val);
					NodeExpression key = ParseExpression(t);
					Expect(Token.Type.Keyword, "}");
					NodeExpression e = new AssocReadExpression(t, id, key);
					return e;
				} else {
					NodeExpression e = new IDExpression(t, next.val);
					return e;
				}

			} else if (next.type == Token.Type.Keyword && next.val == "(") {
				// par expressions
				tn.Next();
				NodeExpression n = ParseExpression(t);
				Expect(Token.Type.Keyword, ")");
				return n;
			} else if (next.type == Token.Type.Keyword && next.val == "[") {
				// collection expressions

				List<NodeExpression> exprs = new List<NodeExpression>();
				Token comma = null;

				do {
					tn.Next();
					Token p = tn.Peek();
					if (p.type == Token.Type.Keyword && p.val == "]") break; // empty collection
					NodeExpression n = ParseExpression(t);
					exprs.Add(n);
					comma = tn.Peek();
				} while (comma.type == Token.Type.Keyword && comma.val == ",");

				Expect(Token.Type.Keyword, "]");
				CollectionExpression ce = new CollectionExpression(t, exprs);
				return ce;
			} else if (next.type == Token.Type.Keyword && next.val == "-") {
				// unary neg
				Token neg = tn.Next();
				NodeExpression P = ParseExpressionP(t);
				NodeExpression e = new NegExpression(t, P);
				return e;
			} else {
				Error("Currupt expression");
			}

			return null;
		}

		private NodeExpression ParseExpressionT(NodeTask t) {
			// this is the hard one
			NodeExpression o0 = ParseExpressionP(t);

			Token next = tn.Peek();

			while (next.type == Token.Type.Keyword &&
				(next.val == "*" || next.val == "/")) {
				Token op = tn.Next();
				NodeExpression o1 = ParseExpressionP(t);

				if (op.val == "*") {
					o0 = new ExprMul(t, o0, o1);
				} else if (op.val == "/") {
					o0 = new ExprDiv(t, o0, o1);
				}

				next = tn.Peek();
			}

			return o0;
		}

		private NodeExpression ParseExpressionE(NodeTask t) {
			// this is the hard one
			NodeExpression o0 = ParseExpressionT(t);

			Token next = tn.Peek();

			while (next.type == Token.Type.Keyword &&
				(next.val == "+" || next.val == "-" || next.val == "%")) {
				Token op = tn.Next();
				NodeExpression o1 = ParseExpressionT(t);

				if (op.val == "+") {
					o0 = new ExprAdd(t, o0, o1);
				} else if (op.val == "-") {
					o0 = new ExprSub(t, o0, o1);
				} else if (op.val == "%") {
					o0 = new ExprMod(t, o0, o1);
				}

				next = tn.Peek();
			}

			return o0;
		}

		private NodeExpression ParseExpressionC(NodeTask t) {
			NodeExpression o0 = ParseExpressionE(t);

			Token next = tn.Peek();

			while (next.type == Token.Type.Keyword &&
				(next.val == "<" || next.val == "<=" || next.val == "==" ||
				 next.val == ">=" || next.val == ">" || next.val == "!=")) {
				Token op = tn.Next();
				NodeExpression o1 = ParseExpressionE(t);

				if (op.val == "<")
					o0 = new ExprCmpLt(t, o0, o1);
				else if (op.val == "<=")
					o0 = new ExprCmpLe(t, o0, o1);
				else if (op.val == "==")
					o0 = new ExprCmpEq(t, o0, o1);
				else if (op.val == ">=")
					o0 = new ExprCmpGe(t, o0, o1);
				else if (op.val == ">")
					o0 = new ExprCmpGt(t, o0, o1);
				else if (op.val == "!=")
					o0 = new ExprCmpNe(t, o0, o1);

				next = tn.Peek();
			}

			return o0;
		}

		private NodeExpression ParseExpressionBAnd(NodeTask t) {
			NodeExpression o0 = ParseExpressionC(t);

			Token next = tn.Peek();

			while (next.type == Token.Type.Keyword &&
				(next.val == "&&")) {
				Token op = tn.Next();
				NodeExpression o1 = ParseExpressionBOr(t);

				if (op.val == "&&")
					o0 = new ExprAnd(t, o0, o1);

				next = tn.Peek();
			}

			return o0;
		}

		private NodeExpression ParseExpressionBOr(NodeTask t) {
			NodeExpression o0 = ParseExpressionBAnd(t);

			Token next = tn.Peek();

			while (next.type == Token.Type.Keyword &&
				(next.val == "||")) {
				Token op = tn.Next();
				NodeExpression o1 = ParseExpressionBAnd(t);

				if (op.val == "||")
					o0 = new ExprOr(t, o0, o1);

				next = tn.Peek();
			}

			return o0;
		}

		private NodeExpression ParseExpression(NodeTask t) {
			// this is the hard one
			NodeExpression e = ParseExpressionBOr(t);

			Token next = tn.Peek();

			if (next.type == Token.Type.Keyword && next.val == ";") {
				// all fine
			} else {

			}

			return e;
		}

		public NodeTask ParseTask(NodeTask parent) {
			NodeTask t = new NodeTask(parent);
			Token r = tn.Peek();

			if (r.type == Token.Type.EOF) return null;

			String s_name = null;
			String s_type = null;

			// Type (or name)
			Token t1 = tn.Next();
			if (t1.type != Token.Type.Literal) // !!!
			{
				Error("Task must have a type or name");
				return null;
			}

			Token colon = tn.Peek();
			if (colon.type == Token.Type.Keyword && colon.val == ":") {
				tn.Next(); // eat colon
				Token t2 = tn.Next(); // type

				if (t2.type != Token.Type.Literal) // !!!
				{
					Error("Expected task type after : in task definition");
					return null;
				}

				s_name = t1.val;
				s_type = t2.val;
			} else {
				s_name = null;
				s_type = t1.val;
			}

			t.type = s_type;
			t.name = s_name;

			// {            

			if (!Expect(Token.Type.Keyword, "{"))
				return null;

			// definitions
			Token next = tn.Peek();
			while (next.type == Token.Type.ID) {
				Token ID = tn.Next(); // chomp ID

				if (!Expect(Token.Type.Keyword, "="))
					return null;

				NodeExpression expr = ParseExpression(t);

				if (!Expect(Token.Type.Keyword, ";"))
					return null;

				t.AddDefinition(new NodeDefinition(t, ID.val, expr));
				next = tn.Peek();
			}

			// child tasks
			next = tn.Peek();
			while (next.type == Token.Type.Literal) {
				NodeTask child = ParseTask(t);
				t.AddTask(child);
				next = tn.Peek();
			}

			// }

			if (!Expect(Token.Type.Keyword, "}"))
				return null;

			return t;
		}
	}
}

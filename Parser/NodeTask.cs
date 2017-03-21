using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Parser
{
	public class NodeTask : ASTNode
	{
		public NodeTask parent;
		public string type;
		public string name; // might be null
		List<NodeDefinition> definitions = new List<NodeDefinition>();
		List<FuncDefinition> funcDefinitions = new List<FuncDefinition>();
		public List<NodeTask> subTasks = new List<NodeTask>();

		public NodeTask(NodeTask parent)
		{
			this.parent = parent;
		}

		public void AddDefinition(NodeDefinition def)
		{
			definitions.Add(def);
		}

		public void AddTask(NodeTask task)
		{
			subTasks.Add(task);
			task.parent = this;
		}

		public int GetPrio()
		{
			Value val = GetValueOfId("Prio");

			if (!val.IsInt())
				Error("$Prio has bad value " + val);

			return val.GetIntValue();
		}

		public void AddFunction(FuncDefinition fd)
		{
			funcDefinitions.Add(fd);
		}

		public virtual Value GetValueOfFcall(string def, List<Value> parms)
		{
			int dotIndex = def.IndexOf('.');

			if (dotIndex != -1)
			{
				// dot in name, search in named chilren tasks for it
				String cname = def.Substring(0, dotIndex);
				String cdef = def.Substring(dotIndex + 1);

				foreach (NodeTask t in subTasks)
				{
					if (t.name != null && t.name == cname)
					{
						Value v = t.GetValueOfFcall(cdef, parms);
						return v;
					}
				}
			}
			else
			{
				foreach (FuncDefinition d in funcDefinitions)
				{
					if (d.IsNamed(def))
					{
						Value val = d.call(parms);
						return val;
					}
				}
				if (parent != null)
					return parent.GetValueOfFcall(def, parms);
			}

			Error("Undefined function " + def);
			return null;
		}

		public bool GetBoolValueOfId(string id)
		{
			Value v = GetValueOfId(id);
			return v.GetBoolValue();
		}

		public float GetFloatValueOfId(string id)
		{
			Value v = GetValueOfId(id);
			return v.GetFloatValue();
		}

		public int GetIntValueOfId(string id)
		{
			Value v = GetValueOfId(id);
			return v.GetIntValue();
		}

		public void SetValueOfId(string def, Value val)
		{
			NodeDefinition n = null;

			foreach (NodeDefinition d in definitions)
			{
				if (d.IsNamed(def))
					n = d;
			}

			if (n != null)
				definitions.Remove(n);

			n = new NodeDefinition(this, def, new LiteralExpression(this, val));
			definitions.Add(n);

		}

		public virtual Value GetValueOfId(string def)
		{
			int dotIndex = def.IndexOf('.');

			if (dotIndex != -1)
			{
				// dot in name, search in named chilren tasks for it
				String cname = def.Substring(0, dotIndex);
				String cdef = def.Substring(dotIndex + 1);

				foreach (NodeTask t in subTasks)
				{
					if (t.name != null && t.name == cname)
					{
						Value v = t.GetValueOfId(cdef);
						return v;
					}
				}
			}
			else
			{
				foreach (NodeDefinition d in definitions)
				{
					if (d.IsNamed(def))
					{
						NodeExpression e = d.GetExpression();
						Value val = e.GetValue();

						//  PPather.WriteLine(" def: " + def + " got e " + e + " and val " + val);

						return val;
					}
				}

				if (parent != null)
				{
					Value e = parent.GetValueOfId(def);
					return e;
				}
			}

			Error("No definition of idenfifier " + def);
			return null;
		}

		public virtual NodeExpression GetExpressionOfId(string def)
		{
			/*int dotIndex = def.IndexOf('.'); 

			if (dotIndex != -1)
			{
				// dot in name, search in named chilren tasks for it
				String cname = def.Substring(0, dotIndex);
				String cdef  = def.Substring(dotIndex + 1);
				foreach (
					NodeTask t in subTasks)
				{
					if (t.name != null && t.name == cname)
						return t.GetExpressionOfId(def); 
				}
			}
			else*/
			{
				foreach (NodeDefinition d in definitions)
				{
					if (d.IsNamed(def))
					{
						return d.GetExpression();
					}
				}
			}

			if (parent != null)
			{
				NodeExpression e = parent.GetExpressionOfId(def);
				return e;
			}

			Error("No definition of idenfifier " + def);
			return null;
		}

		public override void dump(int d)
		{
			Console.WriteLine(prefix(d) + type);
			Console.WriteLine(prefix(d) + "{");

			foreach (NodeDefinition nd in definitions)
			{
				nd.dump(d + 1);
			}

			foreach (NodeTask task in subTasks)
			{
				task.dump(d + 1);
			}

			Console.WriteLine(prefix(d) + "}");
		}

		public bool BindSymbols()
		{
			bool ok = true;

			// Traverse the tree and connect ID expression to their expression
			foreach (NodeDefinition d in definitions)
			{
				NodeExpression e = d.GetExpression();
				ok &= e.BindSymbols();
			}

			foreach (NodeTask t in subTasks)
			{
				ok &= t.BindSymbols();
			}

			return ok;
		}
	}
}

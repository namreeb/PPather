using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Glider.Common.Objects;
using Pather.Graph;

namespace Pather.Parser {
	public class RootNode : NodeTask {
		#region Static stuff
		// need empty method so we can call it to ensure the
		// static constructor is called early
		public static void Init() { }

		static Dictionary<string, FcallDelegate> fcallMap = new Dictionary<string, FcallDelegate>(StringComparer.InvariantCultureIgnoreCase);
		static Dictionary<string, PredefinedVarDelegate> predefvarMap = new Dictionary<string, PredefinedVarDelegate>(StringComparer.InvariantCultureIgnoreCase);

		/// <summary>
		/// The static constructor initializes mappings between strings representing
		/// function/variable names within a psc file and the associated C# method
		/// to get the corresponding value.
		/// 
		/// Since this is achieved via reflection, new functions/variables can be
		/// added simply by adding a corresponding method to Fcalls or PredefinedVars
		/// with no further modification required.
		/// </summary>
		static RootNode() {
			BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly;

			// init fcallMap
			Type delegateType = typeof(FcallDelegate);
			string s = "";

			try {
				foreach (MethodInfo mi in typeof(Fcalls).GetMethods(flags)) {
					s += mi.Name + ", ";
					fcallMap[mi.Name] = (FcallDelegate)Delegate.CreateDelegate(delegateType, mi, true);
				}

				//System.Windows.Forms.MessageBox.Show(s);
			} catch (Exception e) {
				System.Windows.Forms.MessageBox.Show(s + "\n\n" + e.GetType().FullName + "\n" + e.Message);
			}


			// init predefvarMap
			delegateType = typeof(PredefinedVarDelegate);
			s = "";

			try {
				foreach (MethodInfo mi in typeof(PredefinedVars).GetMethods(flags)) {
					s += mi.Name + ", ";
					predefvarMap[mi.Name] = (PredefinedVarDelegate)Delegate.CreateDelegate(delegateType, mi, true);
				}

				//System.Windows.Forms.MessageBox.Show(s);
			} catch (Exception e) {
				System.Windows.Forms.MessageBox.Show(s + "\n\n" + e.GetType().FullName + "\n" + e.Message);
			}
		}	

		#endregion

		public RootNode()
			: base(null) {
			type = "Oneshot";
		}

		public override Value GetValueOfFcall(string def, List<Value> parms) {
			Console.WriteLine("fcall " + def + "(" + parms + ")");

			FcallDelegate fn = null; // the function we're going to call

			fcallMap.TryGetValue(def, out fn);

			if (null != fn) {
				return fn(parms.ToArray());
			}

			return null;
		}

		public override Value GetValueOfId(string def) {
			PredefinedVarDelegate fn = null;

			predefvarMap.TryGetValue(def, out fn);

			if (null != fn) {
				return fn();
			}

			return new Value(GContext.Main.GetConfigString(def));
		}

		public override NodeExpression GetExpressionOfId(string def) {
			return null; // dynamic value, not defined my expression
		}
	}
}

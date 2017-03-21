using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using Pather.Graph;

namespace Pather.Parser
{
	public class Value
	{
		public static Value NilValue = new Value("");
		public static Value FalseValue = new Value("false");
		public static Value TrueValue = new Value("true");
		public static Value ZeroValue = new Value("0");

		string value = null; // for scalars
		List<Value> values = null; // for collections
		Dictionary<String, Value> dic = null; // for associative arrays

		public Value(string val)
		{
			value = val;
		}

		public Value(int val)
		{
			value = val.ToString();
		}

		public Value(float val)
		{
			value = val.ToString();
		}

		public Value(List<Value> val)
		{
			values = val;
		}

		public Value(Dictionary<string, Value> val)
		{
			if (val.Comparer != StringComparer.InvariantCultureIgnoreCase)
			{
				Dictionary<string, Value> tmp = new Dictionary<string, Value>(StringComparer.InvariantCultureIgnoreCase);

				foreach (string key in val.Keys)
				{
					tmp[key] = val[key];
				}

				val = tmp;
			}

			dic = val;
		}

		// handy utility function 
		public Value(Dictionary<String, int> val)
		{
			dic = new Dictionary<string, Value>(StringComparer.InvariantCultureIgnoreCase);

			foreach (string key in val.Keys)
			{
				int c = 0;
				val.TryGetValue(key, out c);
				Value v = new Value(c);
				SetAssocValue(key, v);
			}
		}

		public Value(Dictionary<string, string> val)
		{
			dic = new Dictionary<string, Value>(StringComparer.InvariantCultureIgnoreCase);

			foreach (string key in val.Keys)
			{
				string s = "";
				val.TryGetValue(key, out s);
				Value v = new Value(s);
				SetAssocValue(key, v);
			}
		}

		public bool GetBoolValue()
		{
			string s = GetStringValue();
			if (s == "false" || s == "False")
				return false;
			if (s == "0")
				return false;
			if (s == "")
				return false;
			return true;
		}

		public string GetStringValue()
		{
			if (value != null)
				return value;

			if (values != null)
			{
				StringBuilder sb = new StringBuilder("[");

				foreach (Value v in values)
				{
					sb.Append(v.GetStringValue());
					sb.Append(", ");
				}

				sb.Append("]");
				return sb.ToString();
			}

			if (dic != null)
			{
				StringBuilder sb = new StringBuilder("{");

				foreach (String s in dic.Keys)
				{
					Value val = dic[s];
					sb.Append(s);
					sb.Append(" => ");
					sb.Append(val);
					sb.Append(", ");
				}

				sb.Append("}");
				return sb.ToString();
			}

			return "";
		}

		public bool IsInt()
		{
			if (this == NilValue)
				return true;

			try
			{
				Int32.Parse(GetStringValue());
				return true;
			}
			catch
			{
			}

			return false;
		}

		public bool IsFloat()
		{
			if (this == NilValue)
				return true;

			try
			{
				Single.Parse(GetStringValue(), CultureInfo.InvariantCulture);
				return true;
			}
			catch
			{
			}
			return false;
		}

		public bool IsCollection()
		{
			return values != null;
		}

		public bool IsAssocArray()
		{
			return dic != null;
		}

		public void SetAssocValue(string key, Value value)
		{
			if (dic == null)
				dic = new Dictionary<string, Value>(StringComparer.InvariantCultureIgnoreCase);

			this.value = null;
			this.values = null;

			dic.Remove(key);
			dic.Add(key, value);
		}

		public Value GetAssocValue(string key)
		{
			if (dic == null)
				return null;

			Value val = null;

			dic.TryGetValue(key, out val);

			if (val == null)
				return NilValue;

			return val;
		}

		public int GetIntValue()
		{
			if (this == NilValue)
				return 0;

			try
			{
				return Int32.Parse(GetStringValue());
			}
			catch
			{
			}

			return 0;
		}

		public float GetFloatValue()
		{
			if (this == NilValue)
				return 0;

			try
			{
				return Single.Parse(GetStringValue(), CultureInfo.InvariantCulture);
			}
			catch
			{
			}

			return 0f;
		}

		public List<string> GetStringCollectionValues()
		{
			List<string> vals = new List<string>();

			if (values == null)
			{
				if (value != null && this != NilValue)
					vals.Add(GetStringValue()); // not a collection 
			}
			else
			{
				foreach (Value v in values)
				{
					vals.Add(v.GetStringValue());
				}
			}

			return vals;
		}

		public List<int> GetIntCollectionValues()
		{
			List<int> vals = new List<int>();

			if (values == null)
			{
				if (value != null && this != NilValue)
					vals.Add(GetIntValue()); // not a collection 
			}
			else
			{
				foreach (Value v in values)
				{
					vals.Add(v.GetIntValue());
				}
			}

			return vals;
		}


		// this has to be a collection of floats with at least 3 items
		public Location GetLocationValue()
		{
			if (values == null)
				return null;

			float x = 0f, y = 0f, z = 0f;

			try
			{
				x = values[0].GetFloatValue();
				y = values[1].GetFloatValue();
				z = values[2].GetFloatValue();
			}
			catch (Exception e)
			{
				throw new InvalidOperationException(
					"Current value doesn't seem to be a valid Location", e);
			}

			return new Location(x, y, z);
		}

		public Pather.Tasks.BuySet GetBuySetValue()
		{
			if (values == null)
				return null;
			string item = "", minAmount = "", buyAmount = "";
			try
			{
				item = values[0].GetStringValue();
				minAmount = values[1].GetStringValue();
				buyAmount = values[2].GetStringValue();
			}
			catch (Exception e)
			{
				throw new InvalidOperationException(
					"Current value doesn't seem to be a valid buy set", e);
			}
			return new Pather.Tasks.BuySet(item, minAmount, buyAmount);
		}

		public List<Value> GetCollectionValue()
		{
			if (values != null)
				return values;

			List<Value> c = new List<Value>();

			if (value != null)
				c.Add(new Value(value));

			return c;
		}

		public override string ToString()
		{
			return GetStringValue();
		}
	}
}

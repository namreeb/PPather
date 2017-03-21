using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Pather;
using Glider.Common.Objects;

namespace Pather.Parser
{

	public class Preprocessor
	{
		private List<string> lines;
		public TextReader reader;
		private MemoryStream memStream;
		private Stack<bool> conditions;

		public Preprocessor(TextReader reader)
		{
			this.lines = new List<string>();
			this.conditions = new Stack<bool>();
			this.memStream = new MemoryStream();
			this.conditions.Push(true);
			this.reader = reader;

			process();
			writeStream();
		}

		private Preprocessor(string file, List<string> lines, Stack<bool> conditions)
		{
			string basePath = Settings.Instance.TaskFile;
			basePath = basePath.Substring(0, basePath.LastIndexOf('\\') + 1);
			this.reader = this.reader = File.OpenText(basePath + file);
			this.lines = lines;
			this.conditions = conditions;

			process();
		}

		public Stream ProcessedStream
		{
			get
			{
				return this.memStream;
			}
		}

		private void process()
		{
			string line = "";
			string arg = "";

			while (reader.Peek() > 0)
			{
				line = reader.ReadLine().TrimStart(new char[] { '\t', ' ' });
				if (line.StartsWith("#"))
				{
					try
					{
						arg = line.Substring(line.IndexOf('<') + 1, line.IndexOf('>') - line.IndexOf('<') - 1);
					}
					catch (Exception)
					{
					} // preprocess that don't have args
				}


				if (line.StartsWith("#ifclass "))
				{
					if (!this.conditions.Peek())
					{
						this.conditions.Push(false);
						continue;
					}

					this.conditions.Push(arg.ToLower().Equals(GContext.Main.Me.PlayerClass.ToString().ToLower()));
				}
				else if (line.StartsWith("#ifrace "))
				{
					if (!this.conditions.Peek())
					{
						this.conditions.Push(false);
						continue;
					}

					this.conditions.Push(arg.ToLower().Equals(GContext.Main.Me.PlayerRace.ToString().ToLower()));
				}
				else if (this.conditions.Peek() && line.StartsWith("#include "))
				{

					new Preprocessor(arg, this.lines, this.conditions);
				}
				else if (line.Equals("#endif"))
				{
					this.conditions.Pop();
				}
				else if (this.conditions.Peek())
					lines.Add(line);
			}
		}

		private void writeStream()
		{
			char[] chars;
			foreach (string line in lines)
			{
				chars = line.ToCharArray();
				foreach (char c in chars)
				{
					this.memStream.WriteByte((byte)c);
				}
				this.memStream.WriteByte((byte)13);
				this.memStream.WriteByte((byte)10);
			}
			this.memStream.Seek(0, SeekOrigin.Begin);
		}
	}
}

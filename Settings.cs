using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace Pather
{
	public class Settings
	{
		// default settings
		public string FormTitle = "Form1";
		public string UseMount = "Let Task Decide";
		public float MountRange = 50f;
		public bool MaxResurrection = false;
		public int MaxResurrectionAmount = 10;
		public int StopAtLevel = -1;
		public string TaskFile = "PPather\\tasks.psc";

		#region load/save
		public const string SettingsFile = "PPather\\PPather.xml";

		public void Save()
		{
			try
			{
				Stream stream = File.Create(SettingsFile);

				XmlSerializer serializer = new XmlSerializer(typeof(Settings));
				serializer.Serialize(stream, this);
				stream.Close();
			}
			catch
			{
			}
		}

		public void MakeDefault()
		{
			FormTitle = "Form1";
			UseMount = "Let Task Decide";
			MountRange = 50f;
			MaxResurrection = false;
			MaxResurrectionAmount = 10;
			StopAtLevel = -1;
			TaskFile = "PPather\\tasks.psc";
			Save();
		}

		public static Settings Load()
		{
			try
			{
				Stream stream = File.OpenRead(SettingsFile);
				XmlSerializer serializer = new XmlSerializer(typeof(Settings));
				instance = (Settings)serializer.Deserialize(stream);
			}
			catch
			{
				// in case the xml file does not exist, use the default settings
				// that are hard coded in the class definition
				instance = new Settings();
			}

			return instance;
		}

		#endregion

		#region Singleton
		private Settings()
		{
		}

		private static Settings instance = null;

		public static Settings Instance
		{
			get
			{
				if (null == instance)
					throw new NullReferenceException("Trying to access Settings instance before it has been loaded");

				return instance;
			}
		}
		#endregion
	}
}

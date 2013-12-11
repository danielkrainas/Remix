namespace Atlana.Configuration
{
    using System;
    using System.Configuration;

	/// <summary>
	/// Description of SettingsManager.
	/// </summary>
	public sealed class SettingsManager
	{			
		private static DateTime startTime;
		private static DateTime restartTime;
		private static bool mudRunning;

		public static bool ImportCommands
		{
		    get
		    {
		        return (ConfigurationManager.AppSettings["ImportCommands"] == bool.TrueString);
		    }
		}
		
		public static bool UseAutoRestart
		{
		    get
		    {
                return (ConfigurationManager.AppSettings["UseAutoRestart"] == bool.TrueString);
		    }
		}
		
		public static string CopyrightDate
		{
		    get
		    {
                return ConfigurationManager.AppSettings["CopyrightDate"];
		    }
		}
		
		public static string CopyrightOwner
		{
		    get
		    {
                return ConfigurationManager.AppSettings["CopyrightOwner"];
		    }
		}
		
		public static DateTime RestartTime
		{
		    get
		    {
		        return restartTime;
		    }
		    set
		    {
		        restartTime = value;
		    }
		}
		
		public static bool MudRunning
		{
		    get
		    {
		        return mudRunning;
		    }
		    set
		    {
		        mudRunning = value;
		    }
		}

		public static string PluginDirectory
		{
		    get
		    {
                return ConfigurationManager.AppSettings["PluginDirectory"];
		    }
		}
		
		public static string AreaDirectory
		{
		    get
		    {
                return ConfigurationManager.AppSettings["AreaDirectory"];
		    }
		}
		
		public static string CreationFile
		{
		    get
		    {
                return ConfigurationManager.AppSettings["CreationFile"];
		    }
		}
		
		public static string PlayersDirectory
		{
		    get
		    {
                return ConfigurationManager.AppSettings["PlayersDirectory"];
		    }
		}
		
		public static string SystemDirectory
		{
		    get
		    {
                return ConfigurationManager.AppSettings["SystemDirectory"];
		    }
		}
		
		public static string CommandsFile
		{
		    get
		    {
                return ConfigurationManager.AppSettings["CommandsFile"];
		    }
		}
		
		public static string HelpDirectory
		{
			get
			{
                return ConfigurationManager.AppSettings["HelpDirectory"];
			}
		}
				
		public static DateTime StartTime
		{
			get
			{
				return startTime;
			}
			set
			{
				startTime = value;
				SettingsManager.RestartTime = SettingsManager.StartTime.AddDays(1);
			}
		}
		
		public static int Port
		{
			get
			{
                return Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
			}
		}
		
		public static string MudVersion
		{
		    get
		    {
                return ConfigurationManager.AppSettings["Version"];
		    }
		}
		
		public static string MudName
		{
			get
			{
                return ConfigurationManager.AppSettings["Name"];
			}
		}
		
		public static int MaxBacklogSize
		{
			get
			{
                return Convert.ToInt32(ConfigurationManager.AppSettings["MaxBacklogSize"]);
			}
		}		
	}
}

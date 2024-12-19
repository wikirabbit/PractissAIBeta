using System;

namespace DataAccess
{
	public class Logger
	{
		public static void LogError(string message)
		{
			Console.WriteLine(message);
			System.Diagnostics.Trace.TraceError(message);
		}

		public static void LogInfo(string message)
		{
			System.Diagnostics.Trace.TraceInformation(message);
		}

		public static void LogWarning(string message)
		{
			Console.WriteLine(message);
			System.Diagnostics.Trace.TraceWarning(message);
		}
	}
}

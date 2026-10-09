using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace FeHeTool;

internal static class Program
{
	[STAThread]
	private static void Main(string[] args)
	{
		try
		{
			if (args.Length >= 3 && args[0].ToLower() == "convert")
			{
				MapCore.Convert(args[1], args[2], Console.WriteLine);
				return;
			}
			if (args.Length >= 4 && args[0].ToLower() == "merge")
			{
				MapCore.Merge(args[1], args[2], args[3], Console.WriteLine, (args.Length >= 5) ? args[4] : null);
				return;
			}
			if (args.Length >= 4 && args[0].ToLower() == "copyassets")
			{
				string text = ((args.Length >= 5) ? args[4].ToLower() : "precise");
				int aetMode = ((text == "whole") ? 2 : ((text == "deep") ? 1 : 0));
				MapCore.CopyAssetResources(args[1], args[2], args[3], aetMode, Console.WriteLine);
				return;
			}
			if (args.Length >= 3 && args[0].ToLower() == "vbs")
			{
				MapCore.BatchModifyVbs(args[1], args[2], Console.WriteLine);
				return;
			}
			if (args.Length >= 3 && args[0].ToLower() == "fixmapid")
			{
				MapCore.FixMsbCollisionMapID(args[1], args[2], Console.WriteLine);
				return;
			}
			if (args.Length >= 3 && args[0].ToLower() == "fixenemymapid")
			{
				MapCore.FixMsbEnemyMapID(args[1], args[2], Console.WriteLine);
				return;
			}
			if (args.Length >= 4 && args[0].ToLower() == "fixrotcollision")
			{
				MapCore.FixRotatedCollisionMsb(args[1], args[2], args[3], Console.WriteLine);
				return;
			}
			if (args.Length >= 5 && args[0].ToLower() == "fixnpcparam")
			{
				bool secondPass = args.Length >= 6 && (args[5].ToLower() == "2nd" || args[5].ToLower() == "second" || args[5] == "Secondary");
				MapCore.FixNpcParams(args[1], args[2], args[3], args[4], secondPass, Console.WriteLine);
				return;
			}
			if (args.Length >= 4 && args[0].ToLower() == "convertmappieces")
			{
				MapCore.ConvertMapPieces(args[1], args[2], args[3], Console.WriteLine);
				return;
			}
			if (args.Length >= 4 && args[0].ToLower() == "mergeevent")
			{
				MapCore.MergeEmevd(args[1], args[2], args[3], Console.WriteLine, (args.Length >= 5) ? args[4] : null);
				return;
			}
			if (args.Length >= 5 && args[0].ToLower() == "mergecommonfunc")
			{
				MapCore.MergeCommonFunc(args[1], args[2], args[3], args[4], Console.WriteLine);
				return;
			}
			if (args.Length >= 8 && args[0].ToLower() == "mergemapcollision")
			{
				string baseLBhd = ((args[3] == "-") ? null : args[3]);
				string addLBhd = ((args[4] == "-") ? null : args[4]);
				string outLBhd = ((args[6] == "-") ? null : args[6]);
				bool dropHighPieces = args.Length >= 9 && args[8].ToLower() == "drophigh";
				MapCore.MergeMapCollision(args[1], args[2], baseLBhd, addLBhd, args[5], outLBhd, args[7], Console.WriteLine, dropHighPieces);
				return;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error: " + ex);
			LogCrash(ex);
			return;
		}
		try
		{
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += (object s, ThreadExceptionEventArgs e) =>
			{
				LogCrash(e.Exception);
			};
			AppDomain.CurrentDomain.UnhandledException += (object s, UnhandledExceptionEventArgs e) =>
			{
				LogCrash(e.ExceptionObject as Exception);
			};
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			Application.Run(new MainForm());
		}
		catch (Exception ex2)
		{
			LogCrash(ex2);
		}
	}

	private static void LogCrash(Exception ex)
	{
		string path;
		try
		{
			path = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
		}
		catch
		{
			path = AppContext.BaseDirectory;
		}
		string path2 = Path.Combine(path, "Tool crash log.txt");
		try
		{
			File.WriteAllText(path2, "Time: " + DateTime.Now.ToString() + Environment.NewLine + ".NET: " + Environment.Version?.ToString() + Environment.NewLine + "System: " + Environment.OSVersion?.ToString() + Environment.NewLine + "--------- Error details ---------" + Environment.NewLine + (ex?.ToString() ?? "(null)"));
		}
		catch
		{
		}
		try
		{
			MessageBox.Show("The program encountered an error:\n\n" + (ex?.Message ?? "Unknown error") + "\n\nDetails have been written to Tool crash log.txt next to the program, you can send it to me and I'll take a look", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		catch
		{
		}
	}
}

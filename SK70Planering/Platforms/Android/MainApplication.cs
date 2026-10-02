using Android.App;
using Android.Runtime;

namespace SK70Planering;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
		AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
		{
			args.Handled = true;
			LogCrash("AndroidEnvironment", args.Exception);
			App.ShowError("Android Unhandled Exception", args.Exception);
		};
		AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
		{
			Exception ex = args.ExceptionObject is Exception e ? e : new Exception(args.ExceptionObject?.ToString() ?? "Unknown error");
			LogCrash("AppDomain", ex);
			App.ShowError("AppDomain Unhandled Exception", ex);
		};
	}

	private static void LogCrash(string source, Exception ex)
	{
		try
		{
			string logText = $"[{DateTime.Now}] Crash from {source}:\n{ex}\n\n";
			System.Diagnostics.Debug.WriteLine($"[CRASH] {logText}");
			Android.Util.Log.Error("SK70Planering", logText);
			
			// 1. App-specific external files dir (always writable without any permissions: Android/data/com.companyname.sk70planering/files/sk70_crash.txt)
			if (Android.App.Application.Context?.GetExternalFilesDir(null)?.AbsolutePath is string extDir)
			{
				string extPath = System.IO.Path.Combine(extDir, "sk70_crash.txt");
				System.IO.File.AppendAllText(extPath, logText);
			}

			// 2. Internal app files dir (data/user/0/com.companyname.sk70planering/files/sk70_crash.txt)
			if (Android.App.Application.Context?.FilesDir?.AbsolutePath is string filesDir)
			{
				string appPath = System.IO.Path.Combine(filesDir, "sk70_crash.txt");
				System.IO.File.AppendAllText(appPath, logText);
			}

			// 3. Public Download folder
			try
			{
				if (Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath is string downloadsDir)
				{
					string downloadsPath = System.IO.Path.Combine(downloadsDir, "sk70_crash.txt");
					System.IO.File.AppendAllText(downloadsPath, logText);
				}
			}
			catch { }
		}
		catch { }
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

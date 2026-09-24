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
			
			if (Android.App.Application.Context?.FilesDir?.AbsolutePath is string filesDir)
			{
				string appPath = System.IO.Path.Combine(filesDir, "crash_log.txt");
				System.IO.File.AppendAllText(appPath, logText);
			}

			string downloadsPath = "/sdcard/Download/sk70_crash.txt";
			System.IO.File.AppendAllText(downloadsPath, logText);
		}
		catch { }
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

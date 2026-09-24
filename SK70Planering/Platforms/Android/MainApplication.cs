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
			System.Diagnostics.Debug.WriteLine($"[CRASH] Unhandled Android Exception: {args.Exception}");
			args.Handled = true;
		};
		AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
		{
			System.Diagnostics.Debug.WriteLine($"[CRASH] Unhandled AppDomain Exception: {args.ExceptionObject}");
		};
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

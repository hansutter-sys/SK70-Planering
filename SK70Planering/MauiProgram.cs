using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

namespace SK70Planering;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		try
		{
			SQLitePCL.Batteries_V2.Init();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[INIT ERROR] SQLite: {ex}");
		}

		try
		{
			builder.UseBarcodeReader();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[INIT ERROR] ZXing: {ex}");
		}

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

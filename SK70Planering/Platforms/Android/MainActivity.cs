using Android.App;
using Android.Content.PM;
using Android.OS;

namespace SK70Planering;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		try
		{
			base.OnCreate(savedInstanceState);
		}
		catch (System.Exception ex)
		{
			ShowNativeError("MainActivity Startup Error", ex);
		}
	}

	private void ShowNativeError(string title, System.Exception ex)
	{
		RunOnUiThread(() =>
		{
			try
			{
				if (!IsFinishing && !IsDestroyed)
				{
					new Android.App.AlertDialog.Builder(this)
						.SetTitle($"⚠️ {title}")
						.SetMessage(ex?.ToString() ?? "Unknown error")
						.SetPositiveButton("OK", (s, e) => { })
						.Show();
				}
			}
			catch { }
		});
	}
}

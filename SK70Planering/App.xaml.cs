namespace SK70Planering;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		try
		{
			MainPage = new AppShell();
		}
		catch (Exception ex)
		{
			ShowError("Startfel (AppShell)", ex);
		}
	}

	public static void ShowError(string title, Exception ex)
	{
		MainThread.BeginInvokeOnMainThread(() =>
		{
			try
			{
				if (Current != null)
				{
					Current.MainPage = CreateErrorPage(title, ex?.ToString() ?? "Okänt fel");
				}
			}
			catch { }
		});
	}

	private static Page CreateErrorPage(string title, string errorDetails)
	{
		var label = new Label
		{
			Text = $"{title}\n\n{errorDetails}",
			TextColor = Colors.White,
			FontSize = 12,
			FontFamily = "Monospace"
		};

		var copyBtn = new Button
		{
			Text = "Kopiera fel-logg",
			BackgroundColor = Color.FromArgb("#512BD4"),
			TextColor = Colors.White,
			Margin = new Thickness(0, 10, 0, 0)
		};
		copyBtn.Clicked += async (s, e) =>
		{
			try
			{
				await Clipboard.SetTextAsync(errorDetails);
				if (Current?.MainPage != null)
					await Current.MainPage.DisplayAlert("Kopierat", "Loggen har kopierats till urklipp.", "OK");
			}
			catch { }
		};

		var stack = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				new Label { Text = "⚠️ Ett fel uppstod vid start", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Colors.Red },
				copyBtn,
				new ScrollView
				{
					Content = label,
					HeightRequest = 450
				}
			}
		};

		return new ContentPage
		{
			BackgroundColor = Color.FromArgb("#121212"),
			Content = stack
		};
	}
}

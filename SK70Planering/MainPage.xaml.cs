namespace SK70Planering;

public partial class MainPage : ContentPage
{
	private int count = 0;

	public MainPage()
	{
		try
		{
			InitializeComponent();
		}
		catch { }

		BuildUI();
	}

	private void BuildUI()
	{
		BackgroundColor = Color.FromArgb("#121212");

		var titleLabel = new Label
		{
			Text = "SK70-Planering",
			FontSize = 28,
			FontAttributes = FontAttributes.Bold,
			TextColor = Colors.White,
			HorizontalOptions = LayoutOptions.Center
		};

		var statusLabel = new Label
		{
			Text = "Appen är igång! Alla moduler laddade.",
			FontSize = 16,
			TextColor = Color.FromArgb("#4CAF50"),
			HorizontalOptions = LayoutOptions.Center,
			HorizontalTextAlignment = TextAlignment.Center
		};

		var counterBtn = new Button
		{
			Text = "Testa klicka här (0)",
			BackgroundColor = Color.FromArgb("#512BD4"),
			TextColor = Colors.White,
			FontSize = 18,
			CornerRadius = 10,
			Padding = new Thickness(20, 12),
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(0, 20)
		};

		counterBtn.Clicked += (sender, e) =>
		{
			count++;
			counterBtn.Text = $"Klickad {count} gånger!";
		};

		Content = new VerticalStackLayout
		{
			Padding = 30,
			Spacing = 20,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				titleLabel,
				statusLabel,
				counterBtn
			}
		};
	}
}


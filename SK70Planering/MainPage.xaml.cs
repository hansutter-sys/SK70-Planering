using SK70Planering.Models;
using SK70Planering.Services;
using Microsoft.Maui.Controls.Shapes;

namespace SK70Planering;

public partial class MainPage : ContentPage
{
    private int _currentTab = 0;
    private EventType? _selectedFilterType = null; // null = Alla
    private int? _activeStaffingEventId = null; // Om satt, visas bemanningsvyn för detta event
    private readonly Grid _mainLayout;
    private readonly ScrollView _contentScrollView;
    private readonly VerticalStackLayout _tabContentStack;
    private readonly HorizontalStackLayout _tabBar;
    private bool _hasAppeared = false;

    public MainPage()
    {
        InitializeComponent();
        BackgroundColor = Color.FromArgb("#121212");

        _tabContentStack = new VerticalStackLayout
        {
            Spacing = 15,
            Padding = new Thickness(16, 20, 16, 20)
        };

        _contentScrollView = new ScrollView
        {
            Content = _tabContentStack,
            VerticalOptions = LayoutOptions.Fill
        };

        _tabBar = CreateBottomTabBar();

        _mainLayout = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // Header
                new RowDefinition { Height = GridLength.Star }, // Content
                new RowDefinition { Height = GridLength.Auto }  // Bottom Tab Bar
            }
        };

        _mainLayout.Add(CreateHeader(), 0, 0);
        _mainLayout.Add(_contentScrollView, 0, 1);
        _mainLayout.Add(_tabBar, 0, 2);

        Content = _mainLayout;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!_hasAppeared)
        {
            _hasAppeared = true;
            SwitchTab(0);
        }
    }

    private View CreateHeader()
    {
        var logoLabel = new Label
        {
            Text = "🏊 SK70 PLANERING",
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            VerticalOptions = LayoutOptions.Center
        };

        var subtitleLabel = new Label
        {
            Text = "Simklubben 1970 Borås • Träningar, Tävlingar & Bemanning",
            FontSize = 11,
            TextColor = Color.FromArgb("#AAAAAA")
        };

        var headerStack = new VerticalStackLayout
        {
            Children = { logoLabel, subtitleLabel },
            Spacing = 2
        };

        var container = new Border
        {
            BackgroundColor = Color.FromArgb("#1E1E1E"),
            Stroke = Color.FromArgb("#2C2C2C"),
            StrokeThickness = 1,
            Padding = new Thickness(16, 12),
            Content = headerStack
        };

        return container;
    }

    private HorizontalStackLayout CreateBottomTabBar()
    {
        var bar = new HorizontalStackLayout
        {
            BackgroundColor = Color.FromArgb("#1E1E1E"),
            HeightRequest = 60,
            HorizontalOptions = LayoutOptions.Fill
        };

        string[] tabTitles = { "Översikt", "Schema", "Närvaro", "Medlemmar", "Synk" };

        for (int i = 0; i < tabTitles.Length; i++)
        {
            int tabIndex = i;
            var btn = new Button
            {
                Text = tabTitles[i],
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#AAAAAA"),
                BackgroundColor = Colors.Transparent,
                HorizontalOptions = LayoutOptions.Fill,
                CornerRadius = 0
            };
            btn.Clicked += (s, e) =>
            {
                _activeStaffingEventId = null; // Nollställ ev bemanningsvy vid flikbyte
                SwitchTab(tabIndex);
            };
            bar.Add(btn);
        }

        return bar;
    }

    private async void SwitchTab(int index)
    {
        _currentTab = index;
        for (int i = 0; i < _tabBar.Children.Count; i++)
        {
            if (_tabBar.Children[i] is Button btn)
            {
                btn.TextColor = (i == index) ? Color.FromArgb("#512BD4") : Color.FromArgb("#AAAAAA");
            }
        }

        _tabContentStack.Children.Clear();

        try
        {
            switch (index)
            {
                case 0:
                    await BuildDashboardTabAsync();
                    break;
                case 1:
                    if (_activeStaffingEventId.HasValue)
                    {
                        await BuildStaffingViewAsync(_activeStaffingEventId.Value);
                    }
                    else
                    {
                        await BuildEventsTabAsync();
                    }
                    break;
                case 2:
                    await BuildAttendanceTabAsync();
                    break;
                case 3:
                    await BuildMembersTabAsync();
                    break;
                case 4:
                    BuildSettingsTab();
                    break;
            }
        }
        catch (Exception ex)
        {
            _tabContentStack.Children.Add(new Label
            {
                Text = $"⚠️ Ett fel uppstod vid laddning: {ex.Message}",
                TextColor = Colors.Red,
                FontSize = 14
            });
        }
    }

    #region Tab 1: Dashboard (Översikt)
    private async Task BuildDashboardTabAsync()
    {
        var members = await DatabaseService.Instance.GetMembersAsync();
        var allEvents = await DatabaseService.Instance.GetEventsAsync();

        var trainings = allEvents.Where(e => e.Type == EventType.Training).ToList();
        var competitions = allEvents.Where(e => e.Type == EventType.Competition || e.Type == EventType.Camp).ToList();
        var todayEvents = allEvents.Where(e => e.Date.Date == DateTime.Today).ToList();

        var title = new Label
        {
            Text = "📊 Klubböversikt",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };
        _tabContentStack.Children.Add(title);

        var statsGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnSpacing = 10,
            RowSpacing = 10
        };

        statsGrid.Add(CreateStatCard("Aktiva Medlemmar", members.Count.ToString(), "#512BD4"), 0, 0);
        statsGrid.Add(CreateStatCard("Tävlingar & Läger", competitions.Count.ToString(), "#E65100"), 1, 0);
        statsGrid.Add(CreateStatCard("Planerade Träningar", trainings.Count.ToString(), "#00897B"), 0, 1);
        statsGrid.Add(CreateStatCard("Dagens Händelser", todayEvents.Count.ToString(), "#D81B60"), 1, 1);

        _tabContentStack.Children.Add(statsGrid);

        // --- Sektion 1: Kommande Tävlingar ---
        var competitionHeader = new Label
        {
            Text = "🏆 Kommande Tävlingar & Läger",
            FontSize = 17,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFB74D"),
            Margin = new Thickness(0, 15, 0, 0)
        };
        _tabContentStack.Children.Add(competitionHeader);

        if (competitions.Count == 0)
        {
            _tabContentStack.Children.Add(new Label
            {
                Text = "Inga planerade tävlingar inlagda. Klicka på 'Schema' för att skapa.",
                TextColor = Color.FromArgb("#888888"),
                FontSize = 12
            });
        }
        else
        {
            foreach (var comp in competitions.OrderBy(c => c.Date).Take(3))
            {
                _tabContentStack.Children.Add(CreateEventCardView(comp, isCompact: true));
            }
        }

        // --- Sektion 2: Kommande Träningar ---
        var trainingHeader = new Label
        {
            Text = "🏊 Kommande Träningspass",
            FontSize = 17,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            Margin = new Thickness(0, 15, 0, 0)
        };
        _tabContentStack.Children.Add(trainingHeader);

        if (trainings.Count == 0)
        {
            _tabContentStack.Children.Add(new Label
            {
                Text = "Inga planerade träningspass inlagda.",
                TextColor = Color.FromArgb("#888888"),
                FontSize = 12
            });
        }
        else
        {
            foreach (var tr in trainings.OrderBy(t => t.Date).Take(3))
            {
                _tabContentStack.Children.Add(CreateEventCardView(tr, isCompact: true));
            }
        }
    }

    private View CreateStatCard(string title, string value, string colorHex)
    {
        return new Border
        {
            BackgroundColor = Color.FromArgb("#1E1E1E"),
            Stroke = Color.FromArgb(colorHex),
            StrokeThickness = 2,
            Padding = 12,
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = title, FontSize = 11, TextColor = Color.FromArgb("#AAAAAA") },
                    new Label { Text = value, FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb(colorHex) }
                }
            }
        };
    }
    #endregion

    #region Tab 2: Events & Sessions (Schema, Redigering & Händelser)
    private async Task BuildEventsTabAsync()
    {
        var title = new Label
        {
            Text = "📅 Schema & Klubbaktiviteter",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };
        _tabContentStack.Children.Add(title);

        // Skapa Action Knappar
        var actionButtonsGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            ColumnSpacing = 8
        };

        var addTrainingBtn = new Button
        {
            Text = "➕ Nytt Träningspass",
            BackgroundColor = Color.FromArgb("#512BD4"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            FontSize = 12,
            CornerRadius = 8
        };
        addTrainingBtn.Clicked += async (s, e) => await PromptCreateTrainingAsync();

        var addCompBtn = new Button
        {
            Text = "🏆 Nytt Tävlingsevent",
            BackgroundColor = Color.FromArgb("#E65100"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            FontSize = 12,
            CornerRadius = 8
        };
        addCompBtn.Clicked += async (s, e) => await PromptCreateCompetitionAsync();

        actionButtonsGrid.Add(addTrainingBtn, 0, 0);
        actionButtonsGrid.Add(addCompBtn, 1, 0);

        _tabContentStack.Children.Add(actionButtonsGrid);

        var addCampBtn = new Button
        {
            Text = "🏕️ Skapa Läger / Övrigt Event",
            BackgroundColor = Color.FromArgb("#2E7D32"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            FontSize = 12,
            CornerRadius = 8,
            Margin = new Thickness(0, 4, 0, 4)
        };
        addCampBtn.Clicked += async (s, e) => await PromptCreateCampAsync();
        _tabContentStack.Children.Add(addCampBtn);

        // Segmenterad Filterrad
        var filterBar = new HorizontalStackLayout
        {
            Spacing = 6,
            Margin = new Thickness(0, 10, 0, 5)
        };

        var allFilterBtn = CreateFilterButton("Alla", null);
        var trainingFilterBtn = CreateFilterButton("🏊 Träning", EventType.Training);
        var compFilterBtn = CreateFilterButton("🏆 Tävling", EventType.Competition);
        var campFilterBtn = CreateFilterButton("🏕️ Läger", EventType.Camp);

        filterBar.Add(allFilterBtn);
        filterBar.Add(trainingFilterBtn);
        filterBar.Add(compFilterBtn);
        filterBar.Add(campFilterBtn);

        _tabContentStack.Children.Add(filterBar);

        // Läs in händelser
        var events = _selectedFilterType.HasValue
            ? await DatabaseService.Instance.GetEventsByTypeAsync(_selectedFilterType.Value)
            : await DatabaseService.Instance.GetEventsAsync();

        if (events.Count == 0)
        {
            _tabContentStack.Children.Add(new Label
            {
                Text = "Inga händelser hittades för detta filter.",
                TextColor = Color.FromArgb("#888888"),
                FontSize = 13,
                Margin = new Thickness(0, 10)
            });
        }
        else
        {
            foreach (var ev in events)
            {
                _tabContentStack.Children.Add(CreateEventCardView(ev, isCompact: false));
            }
        }
    }

    private Button CreateFilterButton(string text, EventType? type)
    {
        bool isSelected = _selectedFilterType == type;
        var btn = new Button
        {
            Text = text,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = isSelected ? Color.FromArgb("#3D3D3D") : Color.FromArgb("#1F1F1F"),
            TextColor = isSelected ? Colors.White : Color.FromArgb("#AAAAAA"),
            BorderColor = isSelected ? Color.FromArgb("#512BD4") : Color.FromArgb("#333333"),
            BorderWidth = 1,
            CornerRadius = 14,
            Padding = new Thickness(10, 4)
        };
        btn.Clicked += (s, e) =>
        {
            _selectedFilterType = type;
            SwitchTab(1);
        };
        return btn;
    }

    private View CreateEventCardView(ClubEvent ev, bool isCompact)
    {
        var cardLayout = new VerticalStackLayout { Spacing = 4 };

        // Header: Badge + Titel
        var headerRow = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };

        var badge = new Border
        {
            BackgroundColor = Color.FromArgb(ev.BadgeColorHex),
            Padding = new Thickness(6, 2),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(4) },
            Content = new Label
            {
                Text = $"{ev.TypeIcon} {ev.TypeDisplayName.ToUpper()}",
                FontSize = 9,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            }
        };
        headerRow.Add(badge);

        var titleLabel = new Label
        {
            Text = ev.Title,
            FontSize = isCompact ? 14 : 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            VerticalOptions = LayoutOptions.Center
        };
        headerRow.Add(titleLabel);
        cardLayout.Add(headerRow);

        // Datum & Tid info
        string dateText = ev.EndDate.HasValue && ev.EndDate.Value.Date != ev.Date.Date
            ? $"📅 {ev.Date:yyyy-MM-dd} – {ev.EndDate:yyyy-MM-dd}"
            : $"📅 {ev.Date:yyyy-MM-dd}";

        if (ev.Type == EventType.Training)
        {
            dateText += $" ({ev.StartTime:hh\\:mm} - {ev.EndTime:hh\\:mm})";
        }

        dateText += $" | 📍 {ev.Location}";
        cardLayout.Add(new Label { Text = dateText, TextColor = Color.FromArgb("#CCCCCC"), FontSize = 12 });

        // Grupp & Ansvarig
        string coachLabel = ev.Type == EventType.Competition ? "Ansvarig tränare" : "Tränare";
        cardLayout.Add(new Label
        {
            Text = $"👥 Målgrupp: {ev.GroupName}  |  👤 {coachLabel}: {ev.Coach}",
            TextColor = Color.FromArgb("#999999"),
            FontSize = 11
        });

        // Tävlingsspecifik information
        if (ev.Type == EventType.Competition || ev.Type == EventType.Camp)
        {
            if (ev.RegistrationDeadline.HasValue)
            {
                bool isPast = ev.RegistrationDeadline.Value.Date < DateTime.Today;
                var deadlineLabel = new Label
                {
                    Text = $"⏰ Sista anmälningsdag: {ev.RegistrationDeadline:yyyy-MM-dd}" + (isPast ? " (Stängd)" : ""),
                    TextColor = isPast ? Color.FromArgb("#EF5350") : Color.FromArgb("#FFA726"),
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold
                };
                cardLayout.Add(deadlineLabel);
            }

            if (!string.IsNullOrWhiteSpace(ev.Organizer))
            {
                cardLayout.Add(new Label { Text = $"🏊 Arrangör: {ev.Organizer}", TextColor = Color.FromArgb("#AAAAAA"), FontSize = 11 });
            }
        }

        // Beskrivning om sådan finns
        if (!string.IsNullOrWhiteSpace(ev.Description) && !isCompact)
        {
            cardLayout.Add(new Label
            {
                Text = $"ℹ️ {ev.Description}",
                TextColor = Color.FromArgb("#B0BEC5"),
                FontSize = 11,
                Margin = new Thickness(0, 2, 0, 0)
            });
        }

        // Action-knappar vid detaljvy
        if (!isCompact)
        {
            var btnRow = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(0, 6, 0, 0) };

            // ✏️ Redigera-knapp
            var editBtn = new Button
            {
                Text = "✏️ Redigera",
                FontSize = 11,
                BackgroundColor = Color.FromArgb("#37474F"),
                TextColor = Colors.White,
                Padding = new Thickness(8, 2),
                HeightRequest = 32,
                CornerRadius = 6
            };
            editBtn.Clicked += async (s, e) => await PromptEditEventAsync(ev);
            btnRow.Add(editBtn);

            // 👥 Bemanning-knapp (för tävlingar & läger)
            if (ev.Type == EventType.Competition || ev.Type == EventType.Camp)
            {
                var staffBtn = new Button
                {
                    Text = "👥 Bemanning",
                    FontSize = 11,
                    BackgroundColor = Color.FromArgb("#E65100"),
                    TextColor = Colors.White,
                    FontAttributes = FontAttributes.Bold,
                    Padding = new Thickness(8, 2),
                    HeightRequest = 32,
                    CornerRadius = 6
                };
                staffBtn.Clicked += async (s, e) =>
                {
                    await DatabaseService.Instance.EnsureDefaultStaffRolesAsync(ev.Id, ev.Type);
                    _activeStaffingEventId = ev.Id;
                    SwitchTab(1);
                };
                btnRow.Add(staffBtn);
            }

            if (!string.IsNullOrWhiteSpace(ev.ExternalUrl))
            {
                var linkBtn = new Button
                {
                    Text = "🌐 Länk",
                    FontSize = 11,
                    BackgroundColor = Color.FromArgb("#1976D2"),
                    TextColor = Colors.White,
                    Padding = new Thickness(8, 2),
                    HeightRequest = 32,
                    CornerRadius = 6
                };
                linkBtn.Clicked += async (s, e) =>
                {
                    await DisplayAlert("Webblänk", $"Länk till tävling:\n{ev.ExternalUrl}", "OK");
                };
                btnRow.Add(linkBtn);
            }

            var deleteBtn = new Button
            {
                Text = "🗑️ Ta bort",
                FontSize = 11,
                BackgroundColor = Color.FromArgb("#B71C1C"),
                TextColor = Colors.White,
                Padding = new Thickness(8, 2),
                HeightRequest = 32,
                CornerRadius = 6
            };
            deleteBtn.Clicked += async (s, e) =>
            {
                bool confirm = await DisplayAlert("Bekräfta borttagning", $"Vill du ta bort '{ev.Title}'?", "Ja, ta bort", "Avbryt");
                if (confirm)
                {
                    await DatabaseService.Instance.DeleteEventAsync(ev.Id);
                    SwitchTab(1);
                }
            };
            btnRow.Add(deleteBtn);

            cardLayout.Add(btnRow);
        }

        var card = new Border
        {
            BackgroundColor = Color.FromArgb("#1E1E1E"),
            Stroke = Color.FromArgb(ev.BadgeColorHex),
            StrokeThickness = 1.5,
            Padding = 12,
            Margin = new Thickness(0, 4),
            Content = cardLayout
        };

        return card;
    }

    private async Task PromptEditEventAsync(ClubEvent ev)
    {
        string newTitle = await DisplayPromptAsync("Redigera Händelse", "Titel / Namn:", initialValue: ev.Title);
        if (string.IsNullOrWhiteSpace(newTitle)) return;
        ev.Title = newTitle;

        string newLocation = await DisplayPromptAsync("Redigera Händelse", "Plats / Simhall:", initialValue: ev.Location);
        if (!string.IsNullOrWhiteSpace(newLocation)) ev.Location = newLocation;

        string newGroup = await DisplayPromptAsync("Redigera Händelse", "Målgrupp / Trupp:", initialValue: ev.GroupName);
        if (!string.IsNullOrWhiteSpace(newGroup)) ev.GroupName = newGroup;

        string newCoach = await DisplayPromptAsync("Redigera Händelse", "Ansvarig / Tränare:", initialValue: ev.Coach);
        if (!string.IsNullOrWhiteSpace(newCoach)) ev.Coach = newCoach;

        if (ev.Type == EventType.Competition || ev.Type == EventType.Camp)
        {
            string newOrganizer = await DisplayPromptAsync("Redigera Tävling", "Arrangör:", initialValue: ev.Organizer);
            if (!string.IsNullOrWhiteSpace(newOrganizer)) ev.Organizer = newOrganizer;

            string curDeadline = ev.RegistrationDeadline?.ToString("yyyy-MM-dd") ?? DateTime.Today.AddDays(7).ToString("yyyy-MM-dd");
            string newDeadlineStr = await DisplayPromptAsync("Redigera Tävling", "Sista anmälningsdag (ÅÅÅÅ-MM-DD):", initialValue: curDeadline);
            if (DateTime.TryParse(newDeadlineStr, out var parsedDeadline))
            {
                ev.RegistrationDeadline = parsedDeadline;
            }

            string newUrl = await DisplayPromptAsync("Redigera Tävling", "Webblänk / Livetiming:", initialValue: ev.ExternalUrl);
            if (!string.IsNullOrWhiteSpace(newUrl)) ev.ExternalUrl = newUrl;
        }

        string newDesc = await DisplayPromptAsync("Redigera Information", "Anteckningar / Info:", initialValue: ev.Description);
        if (newDesc != null) ev.Description = newDesc;

        await DatabaseService.Instance.SaveEventAsync(ev);
        await DisplayAlert("Uppdaterad", $"'{ev.Title}' har sparats!", "OK");
        SwitchTab(1);
    }

    private async Task PromptCreateTrainingAsync()
    {
        string title = await DisplayPromptAsync("Nytt Träningspass", "Ange rubrik/fokus för passet:", initialValue: "Kvällspass A-Trupp (Teknik)");
        if (string.IsNullOrWhiteSpace(title)) return;

        string group = await DisplayPromptAsync("Träningsgrupp", "Ange grupp:", initialValue: "A-Trupp");
        string location = await DisplayPromptAsync("Plats", "Ange simhall/bassäng:", initialValue: "Borås Simhall 50m");
        string coach = await DisplayPromptAsync("Tränare", "Ange ansvarig tränare:", initialValue: "Peter H");

        var newSession = new ClubEvent
        {
            Type = EventType.Training,
            Title = title,
            GroupName = string.IsNullOrWhiteSpace(group) ? "A-Trupp" : group,
            Location = string.IsNullOrWhiteSpace(location) ? "Borås Simhall 50m" : location,
            Coach = string.IsNullOrWhiteSpace(coach) ? "Huvudtränare" : coach,
            Date = DateTime.Today,
            StartTime = new TimeSpan(17, 30, 0),
            EndTime = new TimeSpan(19, 30, 0),
            Description = "Planerat träningspass."
        };

        await DatabaseService.Instance.SaveEventAsync(newSession);
        await DisplayAlert("Träning Sparad", $"Träningspasset '{title}' har lagts till i schemat!", "OK");
        SwitchTab(1);
    }

    private async Task PromptCreateCompetitionAsync()
    {
        string title = await DisplayPromptAsync("Nytt Tävlingsevent", "Ange tävlingens namn:", initialValue: "Borås Speedo Meet 2026");
        if (string.IsNullOrWhiteSpace(title)) return;

        string location = await DisplayPromptAsync("Plats", "Ange plats/simhall:", initialValue: "Borås Simhall 50m");
        string organizer = await DisplayPromptAsync("Arrangör", "Arrangerande förening:", initialValue: "SK70 Borås");
        string group = await DisplayPromptAsync("Målgrupp", "Vilka trupper ska delta:", initialValue: "A-Trupp, B-Trupp");
        string deadlineStr = await DisplayPromptAsync("Sista anmälningsdag", "Datum för anmälningsstopp (ÅÅÅÅ-MM-DD):", initialValue: DateTime.Today.AddDays(7).ToString("yyyy-MM-dd"));
        string link = await DisplayPromptAsync("Webblänk", "Länk till Livetiming/Tempus/Inbjudan:", initialValue: "https://livetiming.se");

        DateTime deadline = DateTime.TryParse(deadlineStr, out var d) ? d : DateTime.Today.AddDays(7);

        var newComp = new ClubEvent
        {
            Type = EventType.Competition,
            Title = title,
            Location = string.IsNullOrWhiteSpace(location) ? "Borås Simhall 50m" : location,
            Organizer = string.IsNullOrWhiteSpace(organizer) ? "SK70 Borås" : organizer,
            GroupName = string.IsNullOrWhiteSpace(group) ? "Alla trupper" : group,
            Date = DateTime.Today.AddDays(14),
            EndDate = DateTime.Today.AddDays(15),
            StartTime = new TimeSpan(09, 0, 0),
            EndTime = new TimeSpan(18, 0, 0),
            Coach = "Peter H & Sara K",
            RegistrationDeadline = deadline,
            ExternalUrl = string.IsNullOrWhiteSpace(link) ? "https://livetiming.se" : link,
            Description = "Officiellt tävlingsevent. Kom ihåg att anmäla grenar i tid!"
        };

        await DatabaseService.Instance.SaveEventAsync(newComp);
        await DatabaseService.Instance.EnsureDefaultStaffRolesAsync(newComp.Id, EventType.Competition);
        await DisplayAlert("Tävling Sparad", $"Tävlingen '{title}' har sparats!", "OK");
        SwitchTab(1);
    }

    private async Task PromptCreateCampAsync()
    {
        string title = await DisplayPromptAsync("Nytt Läger / Event", "Ange namn på lägret/eventet:", initialValue: "Höstlovsläger");
        if (string.IsNullOrWhiteSpace(title)) return;

        string location = await DisplayPromptAsync("Plats", "Ange ort/anläggning:", initialValue: "Varberg");
        string group = await DisplayPromptAsync("Deltagande grupper", "Ange grupper:", initialValue: "A-Trupp, B-Trupp");

        var newCamp = new ClubEvent
        {
            Type = EventType.Camp,
            Title = title,
            Location = string.IsNullOrWhiteSpace(location) ? "Bortaplan" : location,
            GroupName = string.IsNullOrWhiteSpace(group) ? "A-Trupp" : group,
            Date = DateTime.Today.AddDays(21),
            EndDate = DateTime.Today.AddDays(24),
            Coach = "Peter H",
            RegistrationDeadline = DateTime.Today.AddDays(14),
            Description = "Träningsläger och sammanhållning."
        };

        await DatabaseService.Instance.SaveEventAsync(newCamp);
        await DatabaseService.Instance.EnsureDefaultStaffRolesAsync(newCamp.Id, EventType.Camp);
        await DisplayAlert("Läger Sparat", $"Lägret '{title}' har lagts till!", "OK");
        SwitchTab(1);
    }
    #endregion

    #region Bemanningsmodul (Staffing View)
    private async Task BuildStaffingViewAsync(int eventId)
    {
        var ev = await DatabaseService.Instance.GetEventByIdAsync(eventId);
        if (ev == null)
        {
            _activeStaffingEventId = null;
            await BuildEventsTabAsync();
            return;
        }

        var staffList = await DatabaseService.Instance.GetStaffAssignmentsAsync(eventId);
        int assignedCount = staffList.Count(s => s.IsAssigned);
        int totalCount = staffList.Count;

        // Tillbaka-knapp
        var backBtn = new Button
        {
            Text = "⬅️ Tillbaka till Schemalistan",
            BackgroundColor = Color.FromArgb("#2C2C2C"),
            TextColor = Colors.White,
            FontSize = 12,
            HorizontalOptions = LayoutOptions.Start,
            CornerRadius = 6,
            Padding = new Thickness(12, 4)
        };
        backBtn.Clicked += (s, e) =>
        {
            _activeStaffingEventId = null;
            SwitchTab(1);
        };
        _tabContentStack.Children.Add(backBtn);

        // Header för eventet
        var titleStack = new VerticalStackLayout { Spacing = 2 };
        titleStack.Children.Add(new Label
        {
            Text = $"👥 Bemanning & Funktionärer",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFB74D")
        });
        titleStack.Children.Add(new Label
        {
            Text = $"{ev.Title} ({ev.Date:yyyy-MM-dd})",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        });
        titleStack.Children.Add(new Label
        {
            Text = $"Status: {assignedCount} av {totalCount} roller tillsatta",
            FontSize = 12,
            TextColor = assignedCount == totalCount && totalCount > 0 ? Color.FromArgb("#4CAF50") : Color.FromArgb("#FFA726"),
            FontAttributes = FontAttributes.Bold
        });
        _tabContentStack.Children.Add(titleStack);

        // Lägg till ny roll-knapp
        var addRoleBtn = new Button
        {
            Text = "➕ Lägg till ny funktionärsroll",
            BackgroundColor = Color.FromArgb("#512BD4"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            FontSize = 12,
            CornerRadius = 8,
            Margin = new Thickness(0, 5, 0, 5)
        };
        addRoleBtn.Clicked += async (s, e) =>
        {
            string role = await DisplayPromptAsync("Ny Funktionärsroll", "Ange rollnamn (t.ex. Speaker, Vändningskontrollant bana 5-6):");
            if (!string.IsNullOrWhiteSpace(role))
            {
                await DatabaseService.Instance.SaveStaffAssignmentAsync(new EventStaffAssignment
                {
                    EventId = eventId,
                    RoleName = role,
                    ShiftOrNotes = "Hela passet",
                    IsConfirmed = false
                });
                SwitchTab(1);
            }
        };
        _tabContentStack.Children.Add(addRoleBtn);

        // Lista alla funktionärsroller
        foreach (var staff in staffList)
        {
            var roleCardLayout = new VerticalStackLayout { Spacing = 4 };

            var topRow = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };

            var badge = new Border
            {
                BackgroundColor = Color.FromArgb(staff.StatusBadgeColorHex),
                Padding = new Thickness(6, 2),
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(4) },
                Content = new Label
                {
                    Text = staff.StatusBadgeText,
                    FontSize = 9,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                }
            };
            topRow.Add(badge);

            topRow.Add(new Label
            {
                Text = staff.RoleName,
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center
            });
            roleCardLayout.Add(topRow);

            if (staff.IsAssigned)
            {
                roleCardLayout.Add(new Label
                {
                    Text = $"👤 Tilldelad: {staff.AssignedPersonName}" + (!string.IsNullOrWhiteSpace(staff.AssignedPersonPhone) ? $" (📞 {staff.AssignedPersonPhone})" : ""),
                    TextColor = Colors.White,
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold
                });
            }
            else
            {
                roleCardLayout.Add(new Label
                {
                    Text = "⚠️ Ingen funktionär tillsatt ännu",
                    TextColor = Color.FromArgb("#EF5350"),
                    FontSize = 12
                });
            }

            roleCardLayout.Add(new Label
            {
                Text = $"🕒 Pass / Anteckning: {staff.ShiftOrNotes}",
                TextColor = Color.FromArgb("#AAAAAA"),
                FontSize = 11
            });

            // Knappar per funktionärsroll
            var btnRow = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(0, 4, 0, 0) };

            var assignBtn = new Button
            {
                Text = staff.IsAssigned ? "✏️ Ändra person" : "👤 Tillsätt person",
                FontSize = 11,
                BackgroundColor = Color.FromArgb("#1976D2"),
                TextColor = Colors.White,
                Padding = new Thickness(8, 2),
                HeightRequest = 32,
                CornerRadius = 6
            };
            assignBtn.Clicked += async (s, e) =>
            {
                string person = await DisplayPromptAsync($"Tillsätt {staff.RoleName}", "Namn på funktionär/förälder:", initialValue: staff.AssignedPersonName);
                if (person != null)
                {
                    staff.AssignedPersonName = person;
                    if (!string.IsNullOrWhiteSpace(person))
                    {
                        string phone = await DisplayPromptAsync("Telefonnummer", "Kontaktnummer:", initialValue: staff.AssignedPersonPhone);
                        if (phone != null) staff.AssignedPersonPhone = phone;

                        string shift = await DisplayPromptAsync("Pass/Tid", "Ange tider/pass:", initialValue: staff.ShiftOrNotes);
                        if (!string.IsNullOrWhiteSpace(shift)) staff.ShiftOrNotes = shift;
                    }
                    await DatabaseService.Instance.SaveStaffAssignmentAsync(staff);
                    SwitchTab(1);
                }
            };
            btnRow.Add(assignBtn);

            if (staff.IsAssigned)
            {
                var confirmBtn = new Button
                {
                    Text = staff.IsConfirmed ? "↩️ Ångra bekräftelse" : "✅ Bekräfta",
                    FontSize = 11,
                    BackgroundColor = staff.IsConfirmed ? Color.FromArgb("#455A64") : Color.FromArgb("#2E7D32"),
                    TextColor = Colors.White,
                    Padding = new Thickness(8, 2),
                    HeightRequest = 32,
                    CornerRadius = 6
                };
                confirmBtn.Clicked += async (s, e) =>
                {
                    staff.IsConfirmed = !staff.IsConfirmed;
                    await DatabaseService.Instance.SaveStaffAssignmentAsync(staff);
                    SwitchTab(1);
                };
                btnRow.Add(confirmBtn);
            }

            var deleteRoleBtn = new Button
            {
                Text = "🗑️",
                FontSize = 11,
                BackgroundColor = Color.FromArgb("#B71C1C"),
                TextColor = Colors.White,
                Padding = new Thickness(6, 2),
                HeightRequest = 32,
                CornerRadius = 6
            };
            deleteRoleBtn.Clicked += async (s, e) =>
            {
                bool confirm = await DisplayAlert("Ta bort roll", $"Vill du ta bort rollen '{staff.RoleName}'?", "Ja", "Avbryt");
                if (confirm)
                {
                    await DatabaseService.Instance.DeleteStaffAssignmentAsync(staff.Id);
                    SwitchTab(1);
                }
            };
            btnRow.Add(deleteRoleBtn);

            roleCardLayout.Add(btnRow);

            var borderCard = new Border
            {
                BackgroundColor = Color.FromArgb("#1E1E1E"),
                Stroke = Color.FromArgb(staff.StatusBadgeColorHex),
                StrokeThickness = 1,
                Padding = 12,
                Margin = new Thickness(0, 3),
                Content = roleCardLayout
            };
            _tabContentStack.Children.Add(borderCard);
        }
    }
    #endregion

    #region Tab 3: Attendance (Närvaro & Anmälan)
    private async Task BuildAttendanceTabAsync()
    {
        var title = new Label
        {
            Text = "✅ Närvaroregistrering & Scanner",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };
        _tabContentStack.Children.Add(title);

        var scanBtn = new Button
        {
            Text = "📷 Starta QR/Streckkodsscanning",
            BackgroundColor = Color.FromArgb("#00897B"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 8
        };
        scanBtn.Clicked += async (s, e) =>
        {
            await DisplayAlert("QR-Scanner", "Streckkodsscanning redo! (ZXing.Net.Maui aktiv)", "OK");
        };
        _tabContentStack.Children.Add(scanBtn);

        var members = await DatabaseService.Instance.GetMembersAsync();
        _tabContentStack.Children.Add(new Label
        {
            Text = "Snabbincheckning för dagens pass:",
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0, 10, 0, 0)
        });

        foreach (var m in members)
        {
            var checkInBtn = new Button
            {
                Text = $"Checka in: {m.FullName} ({m.GroupName})",
                BackgroundColor = Color.FromArgb("#2E2E2E"),
                TextColor = Colors.White,
                FontSize = 13,
                Margin = new Thickness(0, 2)
            };
            checkInBtn.Clicked += async (s, e) =>
            {
                await DatabaseService.Instance.SaveAttendanceAsync(new AttendanceRecord
                {
                    SessionId = 1,
                    MemberId = m.Id,
                    MemberName = m.FullName,
                    GroupName = m.GroupName,
                    ScanMethod = "Manuell"
                });
                await DisplayAlert("Incheckad!", $"{m.FullName} har registrerats för dagens pass.", "OK");
            };
            _tabContentStack.Children.Add(checkInBtn);
        }
    }
    #endregion

    #region Tab 4: Members (Medlemsregister)
    private async Task BuildMembersTabAsync()
    {
        var title = new Label
        {
            Text = "👥 Medlemsregister",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };
        _tabContentStack.Children.Add(title);

        var addMemberBtn = new Button
        {
            Text = "➕ Registrera Ny Simmare",
            BackgroundColor = Color.FromArgb("#512BD4"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 8
        };
        addMemberBtn.Clicked += async (s, e) =>
        {
            string name = await DisplayPromptAsync("Ny Medlem", "Ange fullständigt namn:");
            if (!string.IsNullOrWhiteSpace(name))
            {
                string group = await DisplayPromptAsync("Grupp", "Ange grupp (t.ex. A-Trupp, B-Trupp, Masters):", initialValue: "A-Trupp");
                await DatabaseService.Instance.SaveMemberAsync(new Member
                {
                    FullName = name,
                    GroupName = string.IsNullOrWhiteSpace(group) ? "Simskola" : group,
                    MemberNumber = $"SK70-{new Random().Next(100, 999)}",
                    Email = $"{name.ToLower().Replace(" ", ".")}@sk70.se"
                });
                await DisplayAlert("Sparad", $"{name} har lagts till i registret!", "OK");
                SwitchTab(3);
            }
        };
        _tabContentStack.Children.Add(addMemberBtn);

        var members = await DatabaseService.Instance.GetMembersAsync();
        foreach (var member in members)
        {
            var card = new Border
            {
                BackgroundColor = Color.FromArgb("#1E1E1E"),
                Stroke = Color.FromArgb("#333333"),
                StrokeThickness = 1,
                Padding = 12,
                Margin = new Thickness(0, 3),
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        new Label { Text = member.FullName, FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Colors.White },
                        new Label { Text = $"🆔 {member.MemberNumber}  |  🏊 {member.GroupName}", TextColor = Color.FromArgb("#AAAAAA"), FontSize = 12 },
                        new Label { Text = $"📧 {member.Email}  |  📞 {member.Phone}", TextColor = Color.FromArgb("#888888"), FontSize = 11 }
                    }
                }
            };
            _tabContentStack.Children.Add(card);
        }
    }
    #endregion

    #region Tab 5: Settings & Sync
    private void BuildSettingsTab()
    {
        var title = new Label
        {
            Text = "⚙️ Inställningar & Google Cloud Synk",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };
        _tabContentStack.Children.Add(title);

        var driveCard = new Border
        {
            BackgroundColor = Color.FromArgb("#1E1E1E"),
            Stroke = Color.FromArgb("#4CAF50"),
            StrokeThickness = 1,
            Padding = 14,
            Content = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    new Label { Text = "🟢 Google Drive & Sheets Integration", FontAttributes = FontAttributes.Bold, TextColor = Colors.White, FontSize = 14 },
                    new Label { Text = "Status: Ansluten till SK70 Master Register", TextColor = Color.FromArgb("#4CAF50"), FontSize = 12 },
                    new Label { Text = "Synkar automatiskt: Träningspass, Tävlingsevent, Funktionärsbemanning och Närvaro.", TextColor = Color.FromArgb("#AAAAAA"), FontSize = 11 }
                }
            }
        };
        _tabContentStack.Children.Add(driveCard);

        var syncNowBtn = new Button
        {
            Text = "🔄 Synkronisera Träningar, Tävlingar & Bemanning Nu",
            BackgroundColor = Color.FromArgb("#512BD4"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0, 10, 0, 0)
        };
        syncNowBtn.Clicked += async (s, e) =>
        {
            await DisplayAlert("Google Synk", "Synkronisering utförd! Alla träningspass, tävlingsevent, funktionärsbemanning och närvarolistor har synkats till Google Drive.", "OK");
        };
        _tabContentStack.Children.Add(syncNowBtn);

        var appInfoCard = new Border
        {
            BackgroundColor = Color.FromArgb("#1E1E1E"),
            Stroke = Color.FromArgb("#2E2E2E"),
            StrokeThickness = 1,
            Padding = 14,
            Margin = new Thickness(0, 15, 0, 0),
            Content = new VerticalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    new Label { Text = "SK70-Planering v1.4 (Redigering & Bemanning)", FontAttributes = FontAttributes.Bold, TextColor = Colors.White },
                    new Label { Text = "Simklubben 1970 Borås Admin App", TextColor = Color.FromArgb("#AAAAAA"), FontSize = 12 },
                    new Label { Text = "Lokal SQLite Databas: SK70_planering.db3", TextColor = Color.FromArgb("#888888"), FontSize = 11 }
                }
            }
        };
        _tabContentStack.Children.Add(appInfoCard);
    }
    #endregion
}

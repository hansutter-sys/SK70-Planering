using SQLite;

namespace SK70Planering.Models;

public enum EventType
{
    Training,
    Competition,
    Camp,
    Meeting
}

public class ClubEvent
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public EventType Type { get; set; } = EventType.Training;
    public string Title { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; }
    public TimeSpan StartTime { get; set; } = new TimeSpan(17, 0, 0);
    public TimeSpan EndTime { get; set; } = new TimeSpan(19, 0, 0);
    public string Location { get; set; } = "Borås Simhall 50m";
    public string Coach { get; set; } = "Huvudtränare";
    
    // Tävling & Läger fält
    public DateTime? RegistrationDeadline { get; set; }
    public string Organizer { get; set; } = string.Empty;
    public string ExternalUrl { get; set; } = string.Empty; // Livetiming / Tempus / Inbjudan
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;

    [Ignore]
    public string TypeDisplayName => Type switch
    {
        EventType.Training => "Träning",
        EventType.Competition => "Tävling",
        EventType.Camp => "Läger",
        EventType.Meeting => "Möte",
        _ => "Händelse"
    };

    [Ignore]
    public string TypeIcon => Type switch
    {
        EventType.Training => "🏊",
        EventType.Competition => "🏆",
        EventType.Camp => "🏕️",
        EventType.Meeting => "📋",
        _ => "📅"
    };

    [Ignore]
    public string BadgeColorHex => Type switch
    {
        EventType.Training => "#512BD4",
        EventType.Competition => "#E65100", // Amber / Deep Orange
        EventType.Camp => "#2E7D32",        // Green
        EventType.Meeting => "#0288D1",     // Blue
        _ => "#512BD4"
    };
}

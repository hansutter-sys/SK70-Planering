using SQLite;

namespace SK70Planering.Models;

public class EventStaffAssignment
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int EventId { get; set; }
    public string RoleName { get; set; } = string.Empty; // t.ex. "Tävlingsledare", "Starter", "Tidtagare bana 1", "Sekretariat", "Kiosk/Fika"
    public string AssignedPersonName { get; set; } = string.Empty;
    public string AssignedPersonPhone { get; set; } = string.Empty;
    public string ShiftOrNotes { get; set; } = "Hela passet"; // t.ex. "Pass 1 (09:00 - 13:00)"
    public bool IsConfirmed { get; set; } = false;

    [Ignore]
    public bool IsAssigned => !string.IsNullOrWhiteSpace(AssignedPersonName);

    [Ignore]
    public string StatusBadgeText => !IsAssigned
        ? "⚠️ VAKANT"
        : (IsConfirmed ? "✅ BEKRÄFTAD" : "⏳ EJ BEKRÄFTAD");

    [Ignore]
    public string StatusBadgeColorHex => !IsAssigned
        ? "#D32F2F" // Red
        : (IsConfirmed ? "#2E7D32" : "#F57C00"); // Green / Orange
}

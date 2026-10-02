using SQLite;

namespace SK70Planering.Models;

public class AttendanceRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public int SessionId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public DateTime CheckedInAt { get; set; } = DateTime.Now;
    public string ScanMethod { get; set; } = "Manuell";
}

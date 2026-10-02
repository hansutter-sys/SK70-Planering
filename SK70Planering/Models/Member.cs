using SQLite;

namespace SK70Planering.Models;

public class Member
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string MemberNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

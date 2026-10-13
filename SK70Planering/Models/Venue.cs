using SQLite;

namespace SK70Planering.Models;

public class Venue
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int LaneCount { get; set; } = 8;
    public int PoolLengthMeters { get; set; } = 50;
}

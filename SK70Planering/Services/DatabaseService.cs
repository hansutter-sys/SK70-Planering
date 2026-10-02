using SQLite;
using SK70Planering.Models;

namespace SK70Planering.Services;

public class DatabaseService
{
    private static DatabaseService? _instance;
    public static DatabaseService Instance => _instance ??= new DatabaseService();

    private readonly SQLiteAsyncConnection _db;
    private bool _isInitialized = false;

    private DatabaseService()
    {
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "sk70_planering.db3");
        _db = new SQLiteAsyncConnection(dbPath);
    }

    public async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;

        try
        {
            await _db.CreateTableAsync<Member>();
            await _db.CreateTableAsync<ClubEvent>();
            await _db.CreateTableAsync<AttendanceRecord>();

            int memberCount = await _db.Table<Member>().CountAsync();
            if (memberCount == 0)
            {
                var seedMembers = new List<Member>
                {
                    new Member { MemberNumber = "SK70-101", FullName = "Erik Johansson", GroupName = "A-Trupp", Email = "erik@sk70.se", Phone = "070-1234567" },
                    new Member { MemberNumber = "SK70-102", FullName = "Anna Lindberg", GroupName = "A-Trupp", Email = "anna@sk70.se", Phone = "070-2345678" },
                    new Member { MemberNumber = "SK70-103", FullName = "Oscar Karlsson", GroupName = "B-Trupp", Email = "oscar@sk70.se", Phone = "070-3456789" },
                    new Member { MemberNumber = "SK70-104", FullName = "Emma Nilsson", GroupName = "B-Trupp", Email = "emma@sk70.se", Phone = "070-4567890" },
                    new Member { MemberNumber = "SK70-105", FullName = "Lars Persson", GroupName = "Masters", Email = "lars@sk70.se", Phone = "070-5678901" }
                };
                await _db.InsertAllAsync(seedMembers);
            }

            int eventCount = await _db.Table<ClubEvent>().CountAsync();
            if (eventCount == 0)
            {
                var seedEvents = new List<ClubEvent>
                {
                    // Tävlingsevent
                    new ClubEvent
                    {
                        Type = EventType.Competition,
                        Title = "Borås Speedo Meet 2026",
                        GroupName = "A-Trupp, B-Trupp",
                        Date = DateTime.Today.AddDays(14),
                        EndDate = DateTime.Today.AddDays(15),
                        StartTime = new TimeSpan(09, 0, 0),
                        EndTime = new TimeSpan(18, 0, 0),
                        Location = "Borås Simhall 50m",
                        Coach = "Peter H & Sara K",
                        Organizer = "SK70 Borås",
                        RegistrationDeadline = DateTime.Today.AddDays(5),
                        ExternalUrl = "https://livetiming.se",
                        Description = "Klubbens hemmatävling! Funktionärer och föräldrar behövs. Grenar: 50/100/200 alla simsätt."
                    },
                    new ClubEvent
                    {
                        Type = EventType.Competition,
                        Title = "Vårsimiaden Distrikt",
                        GroupName = "B-Trupp, Utmanare",
                        Date = DateTime.Today.AddDays(28),
                        EndDate = DateTime.Today.AddDays(28),
                        StartTime = new TimeSpan(10, 0, 0),
                        EndTime = new TimeSpan(16, 30, 0),
                        Location = "Alidebergsbadet / Borås Simhall",
                        Coach = "Sara K",
                        Organizer = "Västsvenska Simförbundet",
                        RegistrationDeadline = DateTime.Today.AddDays(12),
                        ExternalUrl = "https://tempusopen.se",
                        Description = "Kval- och utvecklingstävling för yngre simmare."
                    },
                    // Träningspass
                    new ClubEvent
                    {
                        Type = EventType.Training,
                        Title = "Kvällspass A-Trupp (Fokus Frisim & Vändningar)",
                        GroupName = "A-Trupp",
                        Date = DateTime.Today,
                        StartTime = new TimeSpan(17, 30, 0),
                        EndTime = new TimeSpan(19, 30, 0),
                        Location = "Borås Simhall 50m",
                        Coach = "Peter H",
                        Description = "Huvudserie: 10x100 frisim i tävlingstempo."
                    },
                    new ClubEvent
                    {
                        Type = EventType.Training,
                        Title = "Morgonpass B-Trupp",
                        GroupName = "B-Trupp",
                        Date = DateTime.Today,
                        StartTime = new TimeSpan(06, 0, 0),
                        EndTime = new TimeSpan(07, 30, 0),
                        Location = "Borås Simhall 25m",
                        Coach = "Sara K",
                        Description = "Teknik- och grundkonditionspass."
                    },
                    new ClubEvent
                    {
                        Type = EventType.Training,
                        Title = "Masters Teknik & Intervaller",
                        GroupName = "Masters",
                        Date = DateTime.Today.AddDays(1),
                        StartTime = new TimeSpan(20, 0, 0),
                        EndTime = new TimeSpan(21, 30, 0),
                        Location = "Borås Simhall 50m",
                        Coach = "Johan M"
                    },
                    // Läger
                    new ClubEvent
                    {
                        Type = EventType.Camp,
                        Title = "Höstlovsläger & Teambuilding",
                        GroupName = "A-Trupp, B-Trupp",
                        Date = DateTime.Today.AddDays(35),
                        EndDate = DateTime.Today.AddDays(38),
                        Location = "Varberg / Borås",
                        Coach = "Peter H & Sara K",
                        RegistrationDeadline = DateTime.Today.AddDays(20),
                        Description = "Intensivläger inför SM/JSM och Sum-Sim region."
                    }
                };
                await _db.InsertAllAsync(seedEvents);
            }

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DB INIT ERROR] {ex}");
        }
    }

    public async Task<List<Member>> GetMembersAsync()
    {
        await EnsureInitializedAsync();
        return await _db.Table<Member>().ToListAsync();
    }

    public async Task<int> SaveMemberAsync(Member member)
    {
        await EnsureInitializedAsync();
        return member.Id != 0 ? await _db.UpdateAsync(member) : await _db.InsertAsync(member);
    }

    public async Task<List<ClubEvent>> GetEventsAsync()
    {
        await EnsureInitializedAsync();
        return await _db.Table<ClubEvent>().OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToListAsync();
    }

    public async Task<List<ClubEvent>> GetEventsByTypeAsync(EventType type)
    {
        await EnsureInitializedAsync();
        return await _db.Table<ClubEvent>().Where(e => e.Type == type).OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToListAsync();
    }

    public async Task<int> SaveEventAsync(ClubEvent ev)
    {
        await EnsureInitializedAsync();
        return ev.Id != 0 ? await _db.UpdateAsync(ev) : await _db.InsertAsync(ev);
    }

    public async Task<int> DeleteEventAsync(int id)
    {
        await EnsureInitializedAsync();
        return await _db.Table<ClubEvent>().DeleteAsync(e => e.Id == id);
    }

    // Backward compatibility helpers
    public async Task<List<TrainingSession>> GetSessionsAsync()
    {
        var events = await GetEventsAsync();
        return events.Select(e => new TrainingSession
        {
            Id = e.Id,
            Type = e.Type,
            Title = e.Title,
            GroupName = e.GroupName,
            Date = e.Date,
            EndDate = e.EndDate,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            Location = e.Location,
            Coach = e.Coach,
            RegistrationDeadline = e.RegistrationDeadline,
            Organizer = e.Organizer,
            ExternalUrl = e.ExternalUrl,
            Description = e.Description,
            IsCompleted = e.IsCompleted
        }).ToList();
    }

    public async Task<int> SaveSessionAsync(TrainingSession session)
    {
        return await SaveEventAsync(session);
    }

    public async Task<List<AttendanceRecord>> GetAttendanceAsync(int sessionId)
    {
        await EnsureInitializedAsync();
        return await _db.Table<AttendanceRecord>().Where(a => a.SessionId == sessionId).ToListAsync();
    }

    public async Task<int> SaveAttendanceAsync(AttendanceRecord record)
    {
        await EnsureInitializedAsync();
        return await _db.InsertAsync(record);
    }
}

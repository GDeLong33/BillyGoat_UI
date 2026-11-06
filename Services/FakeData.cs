using BillyGoat_UI.Models;

namespace BillyGoat_UI.Services;

/// <summary>
/// Hard-coded sample data for the demo. There is no database yet; later this
/// class can be replaced by one that loads the same objects from a database.
/// </summary>
public static class FakeData
{
    // The demo pretends "today" is Tuesday, October 6, 2026.
    public static readonly DateTime Today = new(2026, 10, 6);

    /// <summary>The three demo users. The password for all of them is "demo".</summary>
    public static readonly List<User> Users =
    [
        new() { UserId = "admin",      Password = "demo", FullName = "Bill Gordon",  Role = UserRole.Administrator },
        new() { UserId = "office",     Password = "demo", FullName = "Lena Brandt",  Role = UserRole.Office },
        new() { UserId = "applicator", Password = "demo", FullName = "Marco Alvarez", Role = UserRole.Applicator },
    ];

    /// <summary>The eight sample customers. Haddad, Sam is inactive.</summary>
    public static List<Customer> GetCustomers() =>
    [
        new() { LastName = "Whitfield", FirstName = "Rachel", StreetAddress = "340 Cedar Ct",       CustomerNumber = "C-01842", NextService = "Grub control",    NextServiceDate = new(2026, 10, 6) },
        new() { LastName = "Park",      FirstName = "Jin",    StreetAddress = "22 Maple Grove Dr", CustomerNumber = "C-00977", NextService = "Mowing",          NextServiceDate = new(2026, 10, 6) },
        new() { LastName = "Bennett",   FirstName = "Karen",  StreetAddress = "61 Elm St",         CustomerNumber = "C-01120", NextService = "Fertilization",   NextServiceDate = new(2026, 10, 8) },
        new() { LastName = "Marsh",     FirstName = "Steve",  StreetAddress = "14 Ridge Rd",       CustomerNumber = "C-00415", NextService = "Mowing",          NextServiceDate = new(2026, 10, 7) },
        new() { LastName = "Okafor",    FirstName = "Daniel", StreetAddress = "908 Lakeshore Dr",  CustomerNumber = "C-02033", NextService = "Mulch work",      NextServiceDate = new(2026, 10, 9) },
        new() { LastName = "Lindqvist", FirstName = "Anna",   StreetAddress = "77 Birch Ln",       CustomerNumber = "C-01508", NextService = "Bed maintenance", NextServiceDate = new(2026, 10, 12) },
        new() { LastName = "Moreno",    FirstName = "Carla",  StreetAddress = "5 Harbor View",     CustomerNumber = "C-01877", NextService = "Fertilization",   NextServiceDate = new(2026, 10, 8) },
        new() { LastName = "Haddad",    FirstName = "Sam",    StreetAddress = "120 Pine St",       CustomerNumber = "C-00693", NextService = "Shrub trimming",  NextServiceDate = new(2026, 10, 13), IsActive = false },
    ];

    /// <summary>
    /// The eight sample employees. Office staff have no stops this week.
    /// Hughes, Sam is terminated and has no time card today.
    /// </summary>
    public static List<Employee> GetEmployees() =>
    [
        new() { LastName = "Alvarez",  FirstName = "Marco", Title = "Applicator",     Id = "MA", HasMdardLicense = true,
                StopsThisWeek = 38, DollarsThisWeek = 3420m, TimeIn = new(7, 30), TimeOut = new(15, 45), VehicleMileage = 61 },
        new() { LastName = "Brandt",   FirstName = "Lena",  Title = "Office Manager", Id = "LB",
                StopsThisWeek = 0,  DollarsThisWeek = 0m,    TimeIn = new(8, 0),  TimeOut = new(16, 30), VehicleMileage = 0 },
        new() { LastName = "Kowalski", FirstName = "Ray",   Title = "Mower",          Id = "RK",
                StopsThisWeek = 52, DollarsThisWeek = 2860m, TimeIn = new(7, 0),  TimeOut = new(15, 30), VehicleMileage = 74 },
        new() { LastName = "Nolan",    FirstName = "Pete",  Title = "Crew Lead",      Id = "PN",
                StopsThisWeek = 45, DollarsThisWeek = 3150m, TimeIn = new(6, 45), TimeOut = new(16, 0),  VehicleMileage = 88 },
        new() { LastName = "Okoye",    FirstName = "Tyler", Title = "Mower",          Id = "TO",
                StopsThisWeek = 49, DollarsThisWeek = 2695m, TimeIn = new(7, 0),  TimeOut = new(15, 15), VehicleMileage = 69 },
        new() { LastName = "Pruitt",   FirstName = "Gail",  Title = "Bookkeeper",     Id = "GP",
                StopsThisWeek = 0,  DollarsThisWeek = 0m,    TimeIn = new(8, 30), TimeOut = new(14, 30), VehicleMileage = 0 },
        new() { LastName = "Vance",    FirstName = "Dana",  Title = "Applicator",     Id = "DV", HasMdardLicense = true,
                StopsThisWeek = 34, DollarsThisWeek = 3060m, TimeIn = new(7, 15), TimeOut = new(16, 0),  VehicleMileage = 57 },
        new() { LastName = "Hughes",   FirstName = "Sam",   Title = "Seasonal Mower", Id = "SH", IsActive = false,
                StopsThisWeek = 0,  DollarsThisWeek = 0m },
    ];
}

using System.Globalization;

namespace BillyGoat_UI.Models;

/// <summary>
/// An employee, with this week's work numbers and today's time card.
/// For the demo these come from Services/FakeData.cs.
/// </summary>
public class Employee
{
    private static readonly CultureInfo Us = CultureInfo.GetCultureInfo("en-US");

    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public required string Title { get; init; }          // job title, e.g. "Applicator"
    public required string Id { get; init; }             // initials, e.g. "MA"
    public bool HasMdardLicense { get; init; }           // Michigan pesticide applicator license
    public bool IsActive { get; init; } = true;          // false = terminated

    // This week
    public int StopsThisWeek { get; init; }
    public decimal DollarsThisWeek { get; init; }

    // Today's time card (null = did not clock in today)
    public TimeOnly? TimeIn { get; init; }
    public TimeOnly? TimeOut { get; init; }
    public int? VehicleMileage { get; init; }

    // ----- Read-only text used by the screen -----

    /// <summary>"Alvarez, Marco"</summary>
    public string DisplayName => $"{LastName}, {FirstName}";

    /// <summary>"On file" or a dash.</summary>
    public string MdardText => HasMdardLicense ? "On file" : "-";

    public string StatusText => IsActive ? "Active" : "Terminated";

    public string DollarsText => DollarsThisWeek.ToString("C0", Us);

    /// <summary>Dollars divided by stops, or a dash when there were no stops (avoids dividing by zero).</summary>
    public string AveragePerStopText =>
        StopsThisWeek > 0 ? (DollarsThisWeek / StopsThisWeek).ToString("C0", Us) : "–";

    /// <summary>"7:30 AM / 3:45 PM"</summary>
    public string TimeInOutText =>
        TimeIn.HasValue && TimeOut.HasValue
            ? $"{TimeIn.Value.ToString("h:mm tt", Us)} / {TimeOut.Value.ToString("h:mm tt", Us)}"
            : "–";

    /// <summary>"61 mi"</summary>
    public string MileageText => VehicleMileage.HasValue ? $"{VehicleMileage} mi" : "–";
}

/// <summary>All / Active / Terminated filter under the employee list.</summary>
public enum EmployeeStatusFilter
{
    All,
    Active,
    Terminated
}

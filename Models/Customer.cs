namespace BillyGoat_UI.Models;

/// <summary>
/// A lawn care customer. For the demo these come from Services/FakeData.cs.
/// </summary>
public class Customer
{
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public required string CustomerNumber { get; init; }   // e.g. "C-01842"
    public required string StreetAddress { get; init; }
    public required string NextService { get; init; }      // e.g. "Grub control"
    public required DateTime NextServiceDate { get; init; }
    public bool IsActive { get; init; } = true;

    // ----- Read-only text used by the list columns -----

    /// <summary>"Whitfield, Rachel"</summary>
    public string DisplayName => $"{LastName}, {FirstName}";

    /// <summary>"Grub control · Oct 6"</summary>
    public string NextServiceText => $"{NextService} · {NextServiceDate:MMM d}";
}

/// <summary>Which customer field the Welcome page search box looks in.</summary>
public enum CustomerSearchField
{
    LastName,
    FirstName,
    CustomerNumber,
    StreetAddress
}

/// <summary>Active / All / Inactive filter under the customer list.</summary>
public enum CustomerStatusFilter
{
    Active,
    All,
    Inactive
}

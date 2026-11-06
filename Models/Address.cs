namespace BillyGoat_UI.Models;

/// <summary>
/// A name and street address. Every account has two: the billing address
/// (where the bill goes) and the service address (the property we work on).
/// The street is split into parts like the old Service Pro 2000 screen:
/// prefix ("N"), number ("340"), name ("Cedar"), suffix ("Ct").
/// </summary>
public class Address
{
    public string Salutation { get; set; } = "";   // Mr, Mrs, Ms, Dr...
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Company { get; set; } = "";
    public string StreetPrefix { get; set; } = "";
    public string StreetNumber { get; set; } = "";
    public string StreetName { get; set; } = "";
    public string StreetSuffix { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "MI";
    public string Zip { get; set; } = "";

    /// <summary>"Whitfield, Rachel", or the company name when there is no person.</summary>
    public string DisplayName =>
        LastName.Length > 0 ? $"{LastName}, {FirstName}" : Company;

    /// <summary>"340 Cedar Ct" (the street parts that are filled in, separated by spaces).</summary>
    public string StreetLine =>
        string.Join(" ", new[] { StreetPrefix, StreetNumber, StreetName, StreetSuffix }.Where(p => p.Length > 0));

    /// <summary>"340 Cedar Ct, Saginaw, MI 48603"</summary>
    public string OneLine => $"{StreetLine}, {City}, {State} {Zip}";

    /// <summary>Copies every field from another address (used for "Same as service address").</summary>
    public void CopyFrom(Address other)
    {
        Salutation = other.Salutation;
        FirstName = other.FirstName;
        LastName = other.LastName;
        Company = other.Company;
        StreetPrefix = other.StreetPrefix;
        StreetNumber = other.StreetNumber;
        StreetName = other.StreetName;
        StreetSuffix = other.StreetSuffix;
        City = other.City;
        State = other.State;
        Zip = other.Zip;
    }
}

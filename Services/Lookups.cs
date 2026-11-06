using System.Collections.ObjectModel;

namespace BillyGoat_UI.Services;

/// <summary>
/// One editable list on the Setup tab, e.g. "Property types" with its items.
/// </summary>
public class LookupList
{
    public LookupList(string name, ObservableCollection<string> items)
    {
        Name = name;
        Items = items;
    }

    public string Name { get; }
    public ObservableCollection<string> Items { get; }
}

/// <summary>
/// The shared drop-down lists for the whole app.
///
/// Every ComboBox binds straight to these collections, for example:
///     ItemsSource="{x:Static services:Lookups.Routes}"
/// Because they are ObservableCollections and there is only one copy of each,
/// adding, renaming or removing an item on the Setup tab shows up in every
/// drop-down immediately (Add Service, Details, Add New Customer, ...).
/// </summary>
public static class Lookups
{
    // ===== Lists the user can edit on the Setup tab =====

    public static ObservableCollection<string> PropertyTypes { get; } =
        ["Residential", "Commercial", "Condominium", "Rental", "Church / School"];

    /// <summary>Route names. Kept in step with the Route objects in FakeData (see FakeData.RenameRoute).</summary>
    public static ObservableCollection<string> Routes { get; } = new();

    public static ObservableCollection<string> Salespeople { get; } =
        ["Lena Brandt", "Pete Nolan", "Marco Alvarez", "Bill Gordon"];

    public static ObservableCollection<string> Sources { get; } =
        ["Referral", "Yard sign", "Website", "Newspaper", "Door hanger", "Repeat customer"];

    public static ObservableCollection<string> Services { get; } =
        ["Mowing", "Fertilization", "Grub control", "Mulch work", "Bed maintenance", "Shrub trimming",
         "Spring cleanup", "Fall cleanup", "Aeration"];

    public static ObservableCollection<string> Frequencies { get; } =
        ["Weekly", "Every 2 weeks", "Monthly", "Every 6 weeks", "One time", "Yearly"];

    public static ObservableCollection<string> SortOptions { get; } =
        ["A - Morning", "B - Midday", "C - Afternoon", "D - Any time"];

    public static ObservableCollection<string> PrenotifyTypes { get; } =
        ["None", "Phone call", "Text message", "Email", "Door hanger"];

    public static ObservableCollection<string> Programs { get; } =
        ["5-Step Lawn Program", "Weekly Mowing Season", "Shrub Care Program", "Bed Care Program"];

    public static ObservableCollection<string> DiscountTypes { get; } =
        ["None", "Senior 10%", "Prepay 5%", "Multi-service 7%", "Employee 20%"];

    /// <summary>All the editable lists, in the order shown on the Setup tab.</summary>
    public static List<LookupList> All { get; } =
    [
        new("Property types", PropertyTypes),
        new("Routes", Routes),
        new("Salespeople", Salespeople),
        new("Sources", Sources),
        new("Services", Services),
        new("Frequencies", Frequencies),
        new("Sort options", SortOptions),
        new("Prenotify types", PrenotifyTypes),
        new("Programs", Programs),
        new("Discount types", DiscountTypes),
    ];

    /// <summary>
    /// The services that make up each program (used by the Add Program dialog).
    /// A program added on the Setup tab that is not listed here becomes one service with the program's name.
    /// </summary>
    public static Dictionary<string, string[]> ProgramServices { get; } = new()
    {
        ["5-Step Lawn Program"] = ["Fertilization round 1", "Fertilization round 2", "Grub control", "Fertilization round 3", "Fertilization round 4"],
        ["Weekly Mowing Season"] = ["Mowing"],
        ["Shrub Care Program"] = ["Shrub trimming", "Shrub trimming (fall)"],
        ["Bed Care Program"] = ["Mulch work", "Bed maintenance"],
    };

    // ===== Fixed lists (not editable, they are part of how the program works) =====

    public static string[] AccountStatuses { get; } = ["Active", "Inactive", "Cancelled", "On hold", "Prospect"];
    public static string[] Salutations { get; } = ["", "Mr", "Mrs", "Ms", "Dr"];
    public static string[] ContactTitles { get; } = ["Mr", "Mrs", "Ms", "Other"];
    public static string[] CardTypes { get; } = ["None", "Mastercard", "Discover", "Visa", "American Express", "Other"];
    public static string[] BillingTypes { get; } = ["Normal", "Main", "Sub", "Corporate"];
    public static string[] PayTypes { get; } = ["Normal", "Pay plan", "Monthly - show price", "Monthly - no price"];
    public static string[] PaymentTypes { get; } = ["Check", "Cash", "Credit card", "Money order"];
    public static string[] LetterSizes { get; } = ["Full page (8.5 x 11)", "Half page", "Postcard"];
    public static string[] LetterTypes { get; } = ["Upsell", "Non upsell", "Renewal", "Prepay reminder"];
    public static string[] PaperSizes { get; } = ["Letter", "Legal", "A4"];
    public static string[] QuickDates { get; } = ["Today", "This week", "2 weeks", "This month", "Last month", "This year", "Custom"];
}

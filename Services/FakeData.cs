using System.Collections.ObjectModel;
using BillyGoat_UI.Models;

namespace BillyGoat_UI.Services;

/// <summary>
/// The in-memory "database" for the demo. Everything is hard-coded here and is
/// lost when the app closes. Later this class can be replaced by one that loads
/// the same objects from a real database.
///
/// Because the lists are static, every screen sees the same data: a payment
/// entered on the Office tab shows up on the account's Services screen too.
/// </summary>
public static class FakeData
{
    /// <summary>The demo pretends "today" is Tuesday, October 6, 2026.</summary>
    public static readonly DateTime Today = new(2026, 10, 6);

    /// <summary>Today's demo date with the real clock time (for time stamps on notes).</summary>
    public static DateTime Now => Today + DateTime.Now.TimeOfDay;

    // Route names, also used as the default route on accounts and services.
    private const string R1 = "1 - Saginaw North";
    private const string R2 = "2 - Freeland West";
    private const string R3 = "3 - Cedar Heights";
    private const string R4 = "4 - Bay City South";

    // ===== Number generators =====
    // Each call returns the next free number, like a database would.

    private static int _nextServiceId = 4101;
    private static int _nextInvoice = 50101;
    private static int _nextAccount = 2101;
    private static int _nextBatch = 1043;

    public static int NewServiceId() => _nextServiceId++;
    public static string NewInvoiceNumber() => (_nextInvoice++).ToString();
    public static string NewAccountNumber() => $"A-{_nextAccount++:00000}";

    /// <summary>Production batches start with P, payment batches with R (for "receipts").</summary>
    public static string NewBatchNumber(string kind) => $"{(kind == "Payment" ? "R" : "P")}-{_nextBatch++}";

    // ===== The data =====

    /// <summary>The three demo users. The password for all of them is "demo".</summary>
    public static readonly List<User> Users =
    [
        new() { UserId = "admin",      Password = "demo", FullName = "Bill Gordon",   Role = UserRole.Administrator },
        new() { UserId = "office",     Password = "demo", FullName = "Lena Brandt",   Role = UserRole.Office },
        new() { UserId = "applicator", Password = "demo", FullName = "Marco Alvarez", Role = UserRole.Applicator },
    ];

    public static List<Employee> Employees { get; }
    public static ObservableCollection<Account> Accounts { get; }
    public static ObservableCollection<Route> Routes { get; }
    public static List<Batch> Batches { get; }

    /// <summary>
    /// The static constructor runs once, the first time FakeData is used.
    /// The order matters: routes refer to accounts, so accounts are made first.
    /// </summary>
    static FakeData()
    {
        Employees = CreateEmployees();
        Accounts = CreateAccounts();
        Routes = CreateRoutes();
        Batches =
        [
            new() { Number = "P-1039", Date = new(2026, 10, 2), Kind = "Production", IsPosted = true },
            new() { Number = "R-1040", Date = new(2026, 10, 2), Kind = "Payment",    IsPosted = true },
            new() { Number = "P-1041", Date = new(2026, 10, 5), Kind = "Production" },
            new() { Number = "R-1042", Date = new(2026, 10, 5), Kind = "Payment" },
        ];

        // The Routes drop-down list shows the same routes as the Routing tab.
        foreach (Route route in Routes)
            Lookups.Routes.Add(route.DisplayName);
    }

    // ===== Helpers used by several screens =====

    /// <summary>Names for "Employee" drop-downs: active employees only, e.g. "Alvarez, Marco".</summary>
    public static List<string> ActiveEmployeeNames =>
        Employees.Where(e => e.IsActive).Select(e => e.DisplayName).ToList();

    /// <summary>Finds an account by its number (not case sensitive), or null.</summary>
    public static Account? FindAccount(string accountNumber) =>
        Accounts.FirstOrDefault(a => a.AccountNumber.Equals(accountNumber.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Finds a service (and its account) by invoice number / barcode, or null.</summary>
    public static (Account Account, ServiceRecord Service)? FindInvoice(string invoiceNumber)
    {
        string number = invoiceNumber.Trim();
        foreach (Account account in Accounts)
        {
            ServiceRecord? service = account.Services.FirstOrDefault(s => s.InvoiceNumber == number);
            if (service != null)
                return (account, service);
        }
        return null;
    }

    /// <summary>
    /// Renames a route everywhere: the Route object, the Routes drop-down list,
    /// and the accounts and services that use the old name.
    /// </summary>
    public static void RenameRoute(Route route, string newName)
    {
        string oldText = route.DisplayName;
        route.Name = newName;
        string newText = route.DisplayName;

        int index = Lookups.Routes.IndexOf(oldText);
        if (index >= 0)
            Lookups.Routes[index] = newText;

        foreach (Account account in Accounts)
        {
            if (account.Route == oldText) account.Route = newText;
            foreach (ServiceRecord service in account.Services)
                if (service.Route == oldText) service.Route = newText;
        }
    }

    /// <summary>Adds a new, empty route (used when a route is added on the Setup tab).</summary>
    public static Route AddRoute(string name)
    {
        int number = Routes.Count == 0 ? 1 : Routes.Max(r => r.Number) + 1;
        Route route = new(number, name);
        Routes.Add(route);
        return route;
    }

    // ===== Sample employees =====

    /// <summary>The eight sample employees. Office staff have no stops this week. Hughes, Sam is terminated.</summary>
    private static List<Employee> CreateEmployees() =>
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

    // ===== Sample accounts =====

    private static ObservableCollection<Account> CreateAccounts()
    {
        // 1. Whitfield, Rachel: on a 5-step lawn program and weekly mowing.
        Account whitfield = NewAccount("A-01842", "Mrs", "Rachel", "Whitfield", "340", "Cedar", "Ct", "Saginaw", "48603",
            measurement: 8400, route: R3, program: "5-Step Lawn Program");
        whitfield.HomePhone = "(989) 555-0142";
        whitfield.MobilePhone = "(989) 555-0187";
        whitfield.Email = "rwhitfield@example.com";
        whitfield.ContactTitle = "Mrs";
        whitfield.CrossStreet = "Center Rd";
        whitfield.Directions = "Third house on the left past the park entrance.";
        whitfield.SignatureOnFile = true;
        whitfield.Prenotification = true;
        whitfield.PrenotifyType = "Text message";
        whitfield.SpecialInstructions = "Close the side gate - dog in the yard. Do not spray within 10 ft of the vegetable garden.";
        whitfield.PrepayPercent = 5;
        whitfield.RenewalLetter = "Renewal letter sent 01/12/2026";
        whitfield.Services.Add(Svc("Fertilization round 4", 52m, new(2025, 10, 14), ServiceRecord.Completed, paid: 52m, employee: "Alvarez, Marco", route: R3, program: "5-Step Lawn Program"));
        whitfield.Services.Add(Svc("Fertilization round 1", 52m, new(2026, 4, 14), ServiceRecord.Completed, paid: 52m, employee: "Alvarez, Marco", route: R3, program: "5-Step Lawn Program"));
        whitfield.Services.Add(Svc("Fertilization round 2", 52m, new(2026, 5, 26), ServiceRecord.Completed, paid: 52m, employee: "Vance, Dana", route: R3, program: "5-Step Lawn Program"));
        whitfield.Services.Add(Svc("Grub control", 68m, new(2026, 7, 8), ServiceRecord.Completed, employee: "Alvarez, Marco", route: R3, program: "5-Step Lawn Program"));
        whitfield.Services.Add(Svc("Fertilization round 3", 52m, new(2026, 8, 18), ServiceRecord.Completed, paid: 52m, employee: "Vance, Dana", route: R3, program: "5-Step Lawn Program"));
        whitfield.Services.Add(Svc("Fertilization round 4", 52m, new(2026, 10, 6), ServiceRecord.Printed, employee: "Alvarez, Marco", route: R3, program: "5-Step Lawn Program"));
        whitfield.Services.Add(Svc("Mowing", 45m, new(2026, 10, 8), ServiceRecord.Open, employee: "Kowalski, Ray", route: R1));
        whitfield.Services.Add(Svc("Fertilization round 1", 54m, new(2027, 4, 13), ServiceRecord.Open, route: R3, program: "5-Step Lawn Program"));
        whitfield.Notes.Add(new() { Timestamp = new(2026, 7, 2, 10, 14, 0), EnteredBy = "Lena Brandt", Text = "Customer called about brown patches near the driveway. Scheduled grub control." });
        whitfield.Notes.Add(new() { Timestamp = new(2026, 9, 21, 15, 2, 0), EnteredBy = "Bill Gordon", Text = "Asked for a quote on fall aeration. Will call back after round 4." });
        whitfield.Letters.Add(new() { DateSent = new(2026, 1, 12), LetterType = "Upsell", Services = "5-Step Lawn Program", PrepayPercent = 5, PrepayAmount = 247m, ExpirationDate = new(2026, 3, 1) });

        // 2. Park, Jin: two names on the service address.
        Account park = NewAccount("A-00977", "Mr", "Jin", "Park", "22", "Maple Grove", "Dr", "Saginaw", "48603",
            measurement: 6200, route: R1, program: "Weekly Mowing Season");
        park.SecondServiceName = "Park, Min-ji";
        park.HomePhone = "(989) 555-0233";
        park.WorkPhone = "(989) 555-0400";
        park.WorkExtension = "212";
        park.Email = "jin.park@example.com";
        park.Frequency = "Weekly";
        park.Services.Add(Svc("Mowing", 40m, new(2025, 9, 29), ServiceRecord.Completed, paid: 40m, employee: "Okoye, Tyler", route: R1));
        park.Services.Add(Svc("Shrub trimming", 120m, new(2026, 9, 2), ServiceRecord.Completed, paid: 120m, employee: "Nolan, Pete", route: R4));
        park.Services.Add(Svc("Mowing", 40m, new(2026, 10, 6), ServiceRecord.Printed, employee: "Okoye, Tyler", route: R1));
        park.Services.Add(Svc("Fertilization round 4", 48m, new(2026, 10, 9), ServiceRecord.Open, route: R3));
        park.Notes.Add(new() { Timestamp = new(2026, 5, 4, 9, 30, 0), EnteredBy = "Lena Brandt", Text = "Min-ji Park is also on the account and can approve work." });

        // 3. Bennett, Karen: owes part of a mulch job.
        Account bennett = NewAccount("A-01120", "Ms", "Karen", "Bennett", "61", "Elm", "St", "Saginaw", "48602",
            measurement: 5400, route: R3, program: "Bed Care Program");
        bennett.HomePhone = "(989) 555-0378";
        bennett.Email = "kbennett@example.com";
        bennett.ContactTitle = "Ms";
        bennett.SeniorCitizen = true;
        bennett.DiscountType = "Senior 10%";
        bennett.Services.Add(Svc("Mulch work", 285m, new(2026, 5, 12), ServiceRecord.Completed, paid: 200m, employee: "Nolan, Pete", route: R2));
        bennett.Services.Add(Svc("Bed maintenance", 95m, new(2026, 6, 20), ServiceRecord.Completed, paid: 95m, employee: "Nolan, Pete", route: R2));
        bennett.Services.Add(Svc("Fertilization round 4", 46m, new(2026, 10, 8), ServiceRecord.Open, route: R3));
        bennett.Notes.Add(new() { Timestamp = new(2026, 6, 1, 11, 45, 0), EnteredBy = "Gail Pruitt", Text = "Paying the mulch balance in two parts. Second check expected in October." });

        // 4. Marsh, Steve: inactive (moved), with an unpaid fall cleanup.
        Account marsh = NewAccount("A-00415", "Mr", "Steve", "Marsh", "14", "Ridge", "Rd", "Freeland", "48623",
            measurement: 9800, route: R2, program: "Weekly Mowing Season");
        marsh.Status = "Inactive";
        marsh.StatusDate = new(2026, 9, 18);
        marsh.StatusDescription = "Moved out of the area";
        marsh.HomePhone = "(989) 555-0419";
        marsh.Services.Add(Svc("Mowing", 50m, new(2026, 5, 20), ServiceRecord.Completed, paid: 50m, employee: "Kowalski, Ray", route: R2));
        marsh.Services.Add(Svc("Fall cleanup", 160m, new(2026, 9, 15), ServiceRecord.Completed, employee: "Nolan, Pete", route: R2));
        marsh.Services.Add(Svc("Mowing", 50m, new(2026, 10, 7), ServiceRecord.Inactive, route: R2));

        // 5. Okafor, Daniel: a rental property billed to his company.
        Account okafor = NewAccount("A-02033", "Mr", "Daniel", "Okafor", "908", "Lakeshore", "Dr", "Bay City", "48706",
            measurement: 12500, route: R1, program: "Weekly Mowing Season");
        okafor.Billing.Company = "Okafor Properties LLC";
        okafor.Billing.StreetPrefix = "";
        okafor.Billing.StreetNumber = "";
        okafor.Billing.StreetName = "PO Box 412";
        okafor.Billing.StreetSuffix = "";
        okafor.Billing.City = "Saginaw";
        okafor.Billing.Zip = "48605";
        okafor.PropertyType = "Rental";
        okafor.WorkPhone = "(989) 555-0901";
        okafor.Email = "office@okaforproperties.example.com";
        okafor.BillingType = "Corporate";
        okafor.Services.Add(Svc("Shrub trimming", 180m, new(2026, 6, 16), ServiceRecord.Completed, paid: 180m, employee: "Nolan, Pete", route: R4));
        okafor.Services.Add(Svc("Mowing", 55m, new(2026, 10, 6), ServiceRecord.Printed, employee: "Kowalski, Ray", route: R1));
        okafor.Services.Add(Svc("Mulch work", 340m, new(2026, 10, 9), ServiceRecord.Open, route: R2));

        // 6. Lindqvist, Anna: billed at a different address.
        Account lindqvist = NewAccount("A-01508", "Ms", "Anna", "Lindqvist", "77", "Birch", "Ln", "Freeland", "48623",
            measurement: 7100, route: R2, program: "Shrub Care Program");
        lindqvist.Billing.StreetNumber = "1840";
        lindqvist.Billing.StreetName = "Bay";
        lindqvist.Billing.StreetSuffix = "Rd";
        lindqvist.Billing.City = "Bay City";
        lindqvist.Billing.Zip = "48706";
        lindqvist.MobilePhone = "(989) 555-0777";
        lindqvist.Email = "anna.lindqvist@example.com";
        lindqvist.ContactTitle = "Ms";
        lindqvist.Services.Add(Svc("Fertilization round 3", 50m, new(2026, 8, 20), ServiceRecord.Completed, paid: 50m, employee: "Vance, Dana", route: R3));
        lindqvist.Services.Add(Svc("Bed maintenance", 95m, new(2026, 10, 12), ServiceRecord.Open, route: R2));
        lindqvist.Services.Add(Svc("Shrub trimming", 150m, new(2026, 10, 14), ServiceRecord.Open, route: R4));

        // 7. Moreno, Carla: mowed today, not paid yet.
        Account moreno = NewAccount("A-01877", "Mrs", "Carla", "Moreno", "5", "Harbor View", "", "Bay City", "48708",
            measurement: 4800, route: R3, program: "5-Step Lawn Program");
        moreno.HomePhone = "(989) 555-0588";
        moreno.Email = "cmoreno@example.com";
        moreno.ContactTitle = "Mrs";
        moreno.Services.Add(Svc("Grub control", 62m, new(2026, 7, 9), ServiceRecord.Completed, paid: 62m, employee: "Alvarez, Marco", route: R3));
        moreno.Services.Add(Svc("Mowing", 40m, new(2026, 10, 6), ServiceRecord.Completed, employee: "Okoye, Tyler", route: R1));
        moreno.Services.Add(Svc("Fertilization round 4", 44m, new(2026, 10, 8), ServiceRecord.Open, route: R3));

        // 8. Haddad, Sam: inactive.
        Account haddad = NewAccount("A-00693", "Mr", "Sam", "Haddad", "120", "Pine", "St", "Saginaw", "48604",
            measurement: 5900, route: R4, program: "Shrub Care Program");
        haddad.Status = "Inactive";
        haddad.StatusDate = new(2026, 3, 2);
        haddad.StatusDescription = "Cancelled for the 2026 season";
        haddad.HomePhone = "(989) 555-0693";
        haddad.Services.Add(Svc("Shrub trimming", 130m, new(2025, 10, 13), ServiceRecord.Completed, paid: 130m, employee: "Nolan, Pete", route: R4));
        haddad.Services.Add(Svc("Shrub trimming", 130m, new(2026, 10, 13), ServiceRecord.Inactive, route: R4));

        var accounts = new ObservableCollection<Account> { whitfield, park, bennett, marsh, okafor, lindqvist, moreno, haddad };
        foreach (Account account in accounts)
            AddAccountingHistory(account);
        return accounts;
    }

    /// <summary>Creates an account whose billing and service addresses are the same.</summary>
    private static Account NewAccount(string number, string salutation, string first, string last,
        string streetNumber, string streetName, string suffix, string city, string zip,
        int measurement, string route, string program)
    {
        Account account = new()
        {
            AccountNumber = number,
            StartDate = new(2019 + number[^1] % 6, 4, 1),     // a different start year for each account
            PropertyType = "Residential",
            Measurement = measurement,
            Route = route,
            Program = program,
            Frequency = "Every 6 weeks",
            Sort = "D - Any time",
            Source = "Referral",
            Salesperson = "Lena Brandt",
            PrenotifyType = "None",
            CreatedBy = "Bill Gordon",
            EditedDate = new(2026, 9, 28),
            EditedBy = "Lena Brandt",
            LastStatementSent = new(2026, 9, 30),
            ConfirmationDate = new(2026, 2, 10),
            ConfirmedBy = "Lena Brandt",
            WelcomeLetterSent = true,
        };
        account.CreatedDate = account.StartDate;
        account.StatusDate = account.StartDate;

        Address service = account.Service;
        service.Salutation = salutation;
        service.FirstName = first;
        service.LastName = last;
        service.StreetNumber = streetNumber;
        service.StreetName = streetName;
        service.StreetSuffix = suffix;
        service.City = city;
        service.Zip = zip;
        account.Billing.CopyFrom(service);
        return account;
    }

    /// <summary>Creates one sample service. Printed and completed dates are filled in from the status.</summary>
    private static ServiceRecord Svc(string name, decimal amount, DateTime scheduled, string status,
        decimal paid = 0m, string employee = "", string route = "", string program = "")
    {
        ServiceRecord service = new()
        {
            ServiceId = NewServiceId(),
            InvoiceNumber = NewInvoiceNumber(),
            ServiceName = name,
            ProgramName = program,
            DateSold = new(scheduled.Year, 2, 15),
            OkayedBy = "Customer",
            DateScheduled = scheduled,
            AssignedEmployee = employee,
            Route = route,
            Frequency = name == "Mowing" ? "Weekly" : "One time",
            Sort = "D - Any time",
            Salesperson = "Lena Brandt",
            PrenotifyType = "None",
            SaleAmount = amount,
            Payments = paid,
            Status = status,
        };
        if (status == ServiceRecord.Printed || status == ServiceRecord.Completed)
            service.DatePrinted = scheduled.AddDays(-3);
        if (status == ServiceRecord.Completed)
            service.DateCompleted = scheduled;
        return service;
    }

    /// <summary>
    /// Builds the "Accounting Adjustments" history from the services:
    /// a Sale line when a service was completed, and a Payment line for money received.
    /// </summary>
    private static void AddAccountingHistory(Account account)
    {
        var entries = new List<AdjustmentEntry>();
        foreach (ServiceRecord s in account.Services)
        {
            if (s.DateCompleted is DateTime done)
            {
                entries.Add(new()
                {
                    EntryDate = done, Transaction = "Sale", Type = "Invoice", Memo = $"{s.ServiceName} (invoice {s.InvoiceNumber})",
                    Total = s.SaleAmount, Batch = $"P-{1000 + done.DayOfYear % 40}", Journalized = done < Today.AddDays(-7), Service = s,
                });
            }
            if (s.Payments > 0)
            {
                DateTime paidOn = (s.DateCompleted ?? s.DateSold).AddDays(9);
                entries.Add(new()
                {
                    EntryDate = paidOn, Transaction = "Payment", Type = "Check", Memo = $"Check #{1100 + s.ServiceId % 300}",
                    Total = -s.Payments, Batch = $"R-{1000 + paidOn.DayOfYear % 40}", Journalized = paidOn < Today.AddDays(-7), Service = s,
                });
            }
        }
        foreach (AdjustmentEntry entry in entries.OrderBy(e => e.EntryDate))
            account.Adjustments.Add(entry);
    }

    // ===== Sample routes =====

    /// <summary>
    /// Four routes. Whitfield, Park and Moreno are on both a mowing route (1)
    /// and the fertilization route (3), to show one property on two routes.
    /// </summary>
    private static ObservableCollection<Route> CreateRoutes()
    {
        Account A(string number) => FindAccount(number)!;

        Route saginawNorth = new(1, "Saginaw North");
        saginawNorth.Stops.Add(new(A("A-00977"), "Mowing", new(2026, 10, 6)));
        saginawNorth.Stops.Add(new(A("A-01877"), "Mowing", new(2026, 10, 6)) { IsCompleted = true });
        saginawNorth.Stops.Add(new(A("A-02033"), "Mowing", new(2026, 10, 6)));
        saginawNorth.Stops.Add(new(A("A-01842"), "Mowing", new(2026, 10, 8)));

        Route freelandWest = new(2, "Freeland West");
        freelandWest.Stops.Add(new(A("A-02033"), "Mulch work", new(2026, 10, 9)));
        freelandWest.Stops.Add(new(A("A-01508"), "Bed maintenance", new(2026, 10, 12)));

        Route cedarHeights = new(3, "Cedar Heights");
        cedarHeights.Stops.Add(new(A("A-01842"), "Fertilization round 4", new(2026, 10, 6)));
        cedarHeights.Stops.Add(new(A("A-01120"), "Fertilization round 4", new(2026, 10, 8)));
        cedarHeights.Stops.Add(new(A("A-01877"), "Fertilization round 4", new(2026, 10, 8)));
        cedarHeights.Stops.Add(new(A("A-00977"), "Fertilization round 4", new(2026, 10, 9)));

        Route bayCitySouth = new(4, "Bay City South");
        bayCitySouth.Stops.Add(new(A("A-01508"), "Shrub trimming", new(2026, 10, 14)));

        var routes = new ObservableCollection<Route> { saginawNorth, freelandWest, cedarHeights, bayCitySouth };
        foreach (Route route in routes)
            route.RenumberStops();
        return routes;
    }
}

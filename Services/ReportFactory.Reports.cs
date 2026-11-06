using System.Windows.Documents;
using BillyGoat_UI.Models;

namespace BillyGoat_UI.Services;

/// <summary>
/// The sample reports for the Report Gallery and Employee Reports. Each report is
/// built from the fake data, so changes made in the demo (completing a service,
/// entering a payment) show up in the reports.
///
/// Every report follows the same pattern: pick the rows, turn each row into an
/// array of strings, and hand them to DocumentBuilder.Table.
/// </summary>
public static partial class ReportFactory
{
    // ===== The two report lists =====

    public static List<ReportDefinition> GalleryReports { get; } =
    [
        new() { Name = "Completed production in detail by account", UsesService = true,
                Description = "Every completed service, grouped by account, with the employee and amount." },
        new() { Name = "Completed production in detail by account (all routes for the month)",
                Description = "Completed services for the whole month of the From date, grouped by route." },
        new() { Name = "Completed production in detail by date by service by employee", UsesService = true,
                Description = "Completed services sorted by date, then service, then employee." },
        new() { Name = "Production left to do in detail by service", UsesService = true,
                Description = "Open and printed services of active accounts that are not done yet." },
        new() { Name = "Payment journal listing",
                Description = "Every payment entered in the date range, with its batch." },
        new() { Name = "Production totals by employee",
                Description = "For each employee and day: number of stops, dollar amount and average per stop." },
        new() { Name = "Production totals by service", UsesService = true,
                Description = "Number of services and dollar totals for each service." },
        new() { Name = "Open invoices and work orders with days past due",
                Description = "Completed services that still have a balance, and printed work orders not done yet." },
        new() { Name = "Fertilization applications by route with square footage totals",
                Description = "Fertilization and grub control applications per route, with total square feet treated." },
    ];

    public static List<ReportDefinition> EmployeeReports { get; } =
    [
        new() { Name = "Employee weekly hours report" },
        new() { Name = "Employee weekly service time report" },
        new() { Name = "Time card labels" },
        new() { Name = "Employee roster" },
        new() { Name = "Return per hour commission report" },
        new() { Name = "Employee commission detail" },
        new() { Name = "Employee commission totals" },
        new() { Name = "Sliding scale sales commission report" },
        new() { Name = "Sliding scale production commission report" },
        new() { Name = "One time commissions paid at point of sale" },
        new() { Name = "One time commission on completed production" },
        new() { Name = "One time commissions on paid services" },
        new() { Name = "One time commissions on completed and paid services" },
    ];

    /// <summary>Builds the named report for the date range (and service, when not "All services").</summary>
    public static FlowDocument Report(string name, DateTime from, DateTime to, string service = "All services")
    {
        string subtitle = $"{Formats.Date(from)} to {Formats.Date(to)}" + (service != "All services" ? $"   |   {service}" : "");
        DocumentBuilder doc = new(name, subtitle);

        switch (name)
        {
            // ----- Report Gallery -----
            case "Completed production in detail by account":
                foreach (var group in Completed(from, to, service).GroupBy(c => c.Account))
                {
                    doc.Heading($"{group.Key.AccountNumber}  {group.Key.BillingName}");
                    doc.Table(["Service", "Completed", "Employee", "Amount"],
                        group.Select(c => new[] { c.Service.ServiceName, Formats.Date(c.Service.DateCompleted), c.Service.AssignedEmployee, Formats.Money(c.Service.SaleAmount) }));
                }
                AddTotal(doc, Completed(from, to, service));
                break;

            case "Completed production in detail by account (all routes for the month)":
                DateTime monthStart = new(from.Year, from.Month, 1);
                var month = Completed(monthStart, monthStart.AddMonths(1).AddDays(-1), "All services").ToList();
                doc.Text($"Month of {monthStart.ToString("MMMM yyyy", Formats.Us)}", bold: true);
                foreach (var group in month.GroupBy(c => c.Service.Route).OrderBy(g => g.Key))
                {
                    doc.Heading(group.Key.Length > 0 ? $"Route {group.Key}" : "No route");
                    doc.Table(["Account", "Name", "Service", "Completed", "Amount"],
                        group.Select(c => new[] { c.Account.AccountNumber, c.Account.BillingName, c.Service.ServiceName, Formats.Date(c.Service.DateCompleted), Formats.Money(c.Service.SaleAmount) }));
                }
                AddTotal(doc, month);
                break;

            case "Completed production in detail by date by service by employee":
                var sorted = Completed(from, to, service)
                    .OrderBy(c => c.Service.DateCompleted).ThenBy(c => c.Service.ServiceName).ThenBy(c => c.Service.AssignedEmployee).ToList();
                doc.Table(["Date", "Service", "Employee", "Account", "Amount"],
                    sorted.Select(c => new[] { Formats.Date(c.Service.DateCompleted), c.Service.ServiceName, c.Service.AssignedEmployee, c.Account.BillingName, Formats.Money(c.Service.SaleAmount) }));
                AddTotal(doc, sorted);
                break;

            case "Production left to do in detail by service":
                var toDo = AllServices()
                    .Where(c => c.Account.IsActive && c.Service.IsOutstanding && InRange(c.Service.DateScheduled, from, to) && Matches(c.Service, service))
                    .OrderBy(c => c.Service.ServiceName).ThenBy(c => c.Service.DateScheduled).ToList();
                doc.Table(["Service", "Scheduled", "Status", "Account", "Service address", "Route", "Amount"],
                    toDo.Select(c => new[] { c.Service.ServiceName, Formats.Date(c.Service.DateScheduled), c.Service.Status, c.Account.BillingName, c.Account.ServiceAddressText, c.Service.Route, Formats.Money(c.Service.SaleAmount) }));
                AddTotal(doc, toDo);
                break;

            case "Payment journal listing":
                var payments = FakeData.Accounts
                    .SelectMany(a => a.Adjustments.Where(e => e.Total < 0 && InRange(e.EntryDate, from, to)).Select(e => (Account: a, Entry: e)))
                    .OrderBy(p => p.Entry.EntryDate).ToList();
                doc.Table(["Date", "Account", "Name", "Type", "Memo", "Batch", "Amount"],
                    payments.Select(p => new[] { Formats.Date(p.Entry.EntryDate), p.Account.AccountNumber, p.Account.BillingName, p.Entry.Type, p.Entry.Memo, p.Entry.Batch, Formats.Money(-p.Entry.Total) }));
                doc.Text($"Total payments: {Formats.Money(-payments.Sum(p => p.Entry.Total))}", bold: true);
                break;

            case "Production totals by employee":
                doc.Table(["Employee", "Date", "Number of stops", "Dollar amount", "Average per stop"],
                    Completed(from, to, "All services")
                        .GroupBy(c => (c.Service.AssignedEmployee, c.Service.DateCompleted))
                        .OrderBy(g => g.Key.AssignedEmployee).ThenBy(g => g.Key.DateCompleted)
                        .Select(g => new[] { g.Key.AssignedEmployee, Formats.Date(g.Key.DateCompleted), g.Count().ToString(),
                                             Formats.Money(g.Sum(c => c.Service.SaleAmount)), Formats.Money(g.Average(c => c.Service.SaleAmount)) }));
                break;

            case "Production totals by service":
                doc.Table(["Service", "Number done", "Dollar amount", "Average"],
                    Completed(from, to, service)
                        .GroupBy(c => c.Service.ServiceName).OrderBy(g => g.Key)
                        .Select(g => new[] { g.Key, g.Count().ToString(), Formats.Money(g.Sum(c => c.Service.SaleAmount)), Formats.Money(g.Average(c => c.Service.SaleAmount)) }));
                AddTotal(doc, Completed(from, to, service));
                break;

            case "Open invoices and work orders with days past due":
                // Bills are due 30 days after the work is done.
                var open = AllServices()
                    .Where(c => (c.Service.Status == ServiceRecord.Completed && c.Service.Balance > 0) || c.Service.Status == ServiceRecord.Printed)
                    .OrderBy(c => c.Service.DateCompleted ?? c.Service.DatePrinted).ToList();
                doc.Table(["Account", "Name", "Service", "Invoice", "Status", "Date", "Balance", "Days past due"],
                    open.Select(c =>
                    {
                        DateTime? date = c.Service.DateCompleted ?? c.Service.DatePrinted;
                        int pastDue = c.Service.DateCompleted is DateTime done ? Math.Max(0, (FakeData.Today - done).Days - 30) : 0;
                        return new[] { c.Account.AccountNumber, c.Account.BillingName, c.Service.ServiceName, c.Service.InvoiceNumber,
                                       c.Service.Status == ServiceRecord.Printed ? "Work order" : "Invoice", Formats.Date(date),
                                       Formats.Money(c.Service.Balance), pastDue > 0 ? $"{pastDue} PAST DUE" : "0" };
                    }));
                doc.Text($"Total open balance: {Formats.Money(open.Where(c => c.Service.Status == ServiceRecord.Completed).Sum(c => c.Service.Balance))}", bold: true);
                break;

            case "Fertilization applications by route with square footage totals":
                var applications = AllServices()
                    .Where(c => (c.Service.ServiceName.StartsWith("Fertilization") || c.Service.ServiceName.StartsWith("Grub"))
                                && c.Service.Status != ServiceRecord.Inactive && InRange(c.Service.DateScheduled, from, to))
                    .ToList();
                foreach (var group in applications.GroupBy(c => c.Service.Route).OrderBy(g => g.Key))
                {
                    doc.Heading(group.Key.Length > 0 ? $"Route {group.Key}" : "No route");
                    doc.Table(["Account", "Service address", "Application", "Scheduled", "Status", "Square feet"],
                        group.Select(c => new[] { c.Account.AccountNumber, c.Account.ServiceAddressText, c.Service.ServiceName,
                                                  Formats.Date(c.Service.DateScheduled), c.Service.Status, c.Account.Measurement.ToString("N0", Formats.Us) }));
                    doc.Text($"Route total: {group.Sum(c => c.Account.Measurement).ToString("N0", Formats.Us)} sq ft", bold: true);
                }
                doc.Text($"All routes: {applications.Sum(c => c.Account.Measurement).ToString("N0", Formats.Us)} sq ft in {applications.Count} applications", bold: true);
                break;

            // ----- Employee reports -----
            default:
                AddEmployeeReport(doc, name, from, to);
                break;
        }
        return doc.Document;
    }

    /// <summary>The 13 employee reports (sample numbers based on the fake employees).</summary>
    private static void AddEmployeeReport(DocumentBuilder doc, string name, DateTime from, DateTime to)
    {
        List<Employee> active = FakeData.Employees.Where(e => e.IsActive).ToList();

        // Hours worked per day, from today's time card (demo: the same every day).
        double HoursPerDay(Employee e) => e.TimeIn.HasValue && e.TimeOut.HasValue ? (e.TimeOut.Value - e.TimeIn.Value).TotalHours : 0;
        string H(double hours) => hours.ToString("0.0", Formats.Us);

        // Completed services in the range for an employee (by their "Last, First" name).
        List<(Account Account, ServiceRecord Service)> Done(Employee e) =>
            Completed(from, to, "All services").Where(c => c.Service.AssignedEmployee == e.DisplayName).ToList();

        switch (name)
        {
            case "Employee weekly hours report":
                doc.Table(["Employee", "Mon", "Tue", "Wed", "Thu", "Fri", "Total hours"],
                    active.Select(e => new[] { e.DisplayName, H(HoursPerDay(e)), H(HoursPerDay(e)), H(HoursPerDay(e)), H(HoursPerDay(e)), H(HoursPerDay(e)), H(HoursPerDay(e) * 5) }));
                break;

            case "Employee weekly service time report":
                doc.Table(["Employee", "Stops", "Hours on site", "Average minutes per stop"],
                    active.Where(e => e.StopsThisWeek > 0).Select(e => new[] { e.DisplayName, e.StopsThisWeek.ToString(), H(e.StopsThisWeek * 0.4), "24" }));
                break;

            case "Time card labels":
                foreach (Employee e in active)
                    doc.Fields(("Employee", e.DisplayName), ("ID", e.Id), ("Week ending", Formats.Date(to)), ("Signature", "______________________"));
                break;

            case "Employee roster":
                doc.Table(["Name", "Title", "ID", "MDARD license", "Status"],
                    FakeData.Employees.Select(e => new[] { e.DisplayName, e.Title, e.Id, e.MdardText, e.StatusText }));
                break;

            case "Return per hour commission report":
                doc.Table(["Employee", "Dollars", "Hours", "Return per hour", "Commission (5%)"],
                    active.Where(e => e.StopsThisWeek > 0).Select(e =>
                    {
                        double hours = HoursPerDay(e) * 5;
                        return new[] { e.DisplayName, Formats.Money(e.DollarsThisWeek), H(hours),
                                       Formats.Money(e.DollarsThisWeek / (decimal)hours), Formats.Money(e.DollarsThisWeek * 0.05m) };
                    }));
                break;

            case "Employee commission detail":
                foreach (Employee e in active)
                {
                    var done = Done(e);
                    if (done.Count == 0) continue;
                    doc.Heading(e.DisplayName);
                    doc.Table(["Account", "Service", "Completed", "Amount", "Rate", "Commission"],
                        done.Select(c => new[] { c.Account.BillingName, c.Service.ServiceName, Formats.Date(c.Service.DateCompleted),
                                                 Formats.Money(c.Service.SaleAmount), "8%", Formats.Money(c.Service.SaleAmount * 0.08m) }));
                }
                break;

            case "Employee commission totals":
                doc.Table(["Employee", "Production", "Rate", "Commission"],
                    active.Select(e => (e, total: Done(e).Sum(c => c.Service.SaleAmount))).Where(x => x.total > 0)
                          .Select(x => new[] { x.e.DisplayName, Formats.Money(x.total), "8%", Formats.Money(x.total * 0.08m) }));
                break;

            case "Sliding scale sales commission report":
            case "Sliding scale production commission report":
                doc.Text("Scale: up to $1,000 = 3%,  $1,000 to $5,000 = 5%,  over $5,000 = 7%.");
                bool sales = name.Contains("sales");
                var amounts = sales
                    ? AllServices().Where(c => InRange(c.Service.DateSold, from, to)).GroupBy(c => c.Service.Salesperson)
                        .Select(g => (Name: g.Key, Total: g.Sum(c => c.Service.SaleAmount)))
                    : active.Select(e => (Name: e.DisplayName, Total: e.DollarsThisWeek)).Where(x => x.Total > 0);
                doc.Table([sales ? "Salesperson" : "Employee", sales ? "Sales" : "Production", "Rate", "Commission"],
                    amounts.Select(x =>
                    {
                        decimal rate = x.Total <= 1000 ? 0.03m : x.Total <= 5000 ? 0.05m : 0.07m;
                        return new[] { x.Name, Formats.Money(x.Total), $"{rate * 100:0}%", Formats.Money(x.Total * rate) };
                    }));
                break;

            default:   // the four "one time commission" reports (10% of each matching service)
                Func<ServiceRecord, bool> include = name switch
                {
                    "One time commissions paid at point of sale" => s => InRange(s.DateSold, from, to),
                    "One time commission on completed production" => s => s.Status == ServiceRecord.Completed && InRange(s.DateCompleted, from, to),
                    "One time commissions on paid services" => s => s.Payments > 0 && InRange(s.DateScheduled, from, to),
                    _ => s => s.Status == ServiceRecord.Completed && s.Balance <= 0 && InRange(s.DateCompleted, from, to),
                };
                var rows = AllServices().Where(c => include(c.Service)).OrderBy(c => c.Service.Salesperson).ToList();
                doc.Table(["Salesperson", "Account", "Service", "Sold", "Amount", "Commission (10%)"],
                    rows.Select(c => new[] { c.Service.Salesperson, c.Account.BillingName, c.Service.ServiceName, Formats.Date(c.Service.DateSold),
                                             Formats.Money(c.Service.SaleAmount), Formats.Money(c.Service.SaleAmount * 0.10m) }));
                doc.Text($"Total commission: {Formats.Money(rows.Sum(c => c.Service.SaleAmount) * 0.10m)}", bold: true);
                break;
        }
    }

    // ===== Small helpers =====

    /// <summary>Every service of every account, with its account.</summary>
    private static IEnumerable<(Account Account, ServiceRecord Service)> AllServices() =>
        FakeData.Accounts.SelectMany(a => a.Services.Select(s => (a, s)));

    /// <summary>Services completed in the date range (and matching the service filter).</summary>
    private static IEnumerable<(Account Account, ServiceRecord Service)> Completed(DateTime from, DateTime to, string service) =>
        AllServices().Where(c => c.Service.Status == ServiceRecord.Completed && InRange(c.Service.DateCompleted, from, to) && Matches(c.Service, service));

    private static bool InRange(DateTime? date, DateTime from, DateTime to) =>
        date.HasValue && date.Value.Date >= from.Date && date.Value.Date <= to.Date;

    /// <summary>"All services" matches everything; "Fertilization" matches every fertilization round.</summary>
    private static bool Matches(ServiceRecord s, string service) =>
        service == "All services" || s.ServiceName.StartsWith(service, StringComparison.OrdinalIgnoreCase);

    private static void AddTotal(DocumentBuilder doc, IEnumerable<(Account Account, ServiceRecord Service)> rows)
    {
        var list = rows.ToList();
        doc.Text($"Total: {list.Count} service(s), {Formats.Money(list.Sum(c => c.Service.SaleAmount))}", bold: true);
    }
}

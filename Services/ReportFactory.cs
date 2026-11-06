using System.Windows.Documents;
using BillyGoat_UI.Models;

namespace BillyGoat_UI.Services;

/// <summary>
/// Builds the printable documents used around the app: work orders, statements,
/// letters and the sample reports. Each method returns a FlowDocument for PrintService.
/// </summary>
public static partial class ReportFactory
{
    /// <summary>Work orders: one page per service, with the barcode the crew scans when done.</summary>
    public static FlowDocument WorkOrders(IEnumerable<(Account Account, ServiceRecord Service)> orders, string subtitle = "")
    {
        var list = orders.ToList();
        DocumentBuilder doc = new("Work orders", subtitle.Length > 0 ? subtitle : $"{list.Count} work order(s)");

        bool first = true;
        foreach ((Account account, ServiceRecord service) in list)
        {
            if (!first)
                doc.PageBreak();
            first = false;

            doc.Heading($"Work order - {service.ServiceName}")
               .Barcode(service.InvoiceNumber)
               .Fields(
                   ("Account", $"{account.AccountNumber}  {account.ServiceNameText}"),
                   ("Service address", account.ServiceAddressText),
                   ("Phone", FirstNonEmpty(account.HomePhone, account.MobilePhone, account.WorkPhone)),
                   ("Invoice / Service ID", $"{service.InvoiceNumber} / {service.ServiceId}"),
                   ("Scheduled", Formats.Date(service.DateScheduled)),
                   ("Route", service.Route),
                   ("Assigned employee", service.AssignedEmployee),
                   ("Measurement", account.MeasurementText),
                   ("Amount", Formats.Money(service.SaleAmount)),
                   ("Balance due", Formats.Money(service.Balance)));

            if (account.SpecialInstructions.Length > 0)
                doc.Text("Special instructions: " + account.SpecialInstructions, bold: true);
            if (service.Instructions.Length > 0)
                doc.Text("Service instructions: " + service.Instructions);
            if (account.Directions.Length > 0)
                doc.Text("Directions: " + account.Directions);

            doc.Text("Completed by: ____________________   Date: ____________   Time in: ________   Time out: ________");
            doc.Text("Materials used: ______________________________________________   Amount: ____________");
        }

        if (list.Count == 0)
            doc.Text("No work orders matched.");
        return doc.Document;
    }

    /// <summary>A statement of what the account owes (printed from the Status Change dialog).</summary>
    public static FlowDocument Statement(Account account)
    {
        DocumentBuilder doc = new("Statement", $"Account {account.AccountNumber}");
        doc.Fields(
                ("Bill to", account.Billing.DisplayName),
                ("Address", account.BillingAddressText),
                ("Statement date", Formats.Date(FakeData.Today)))
           .Table(["Service", "Invoice", "Completed", "Sale amount", "Payments", "Balance"],
               account.Services.Where(s => s.Balance != 0 && s.Status != ServiceRecord.Inactive).Select(s => new[]
               {
                   s.ServiceName, s.InvoiceNumber, Formats.Date(s.DateCompleted),
                   Formats.Money(s.SaleAmount), Formats.Money(s.Payments), Formats.Money(s.Balance),
               }))
           .Text($"Total balance due: {Formats.Money(account.Balance)}", bold: true);
        return doc.Document;
    }

    /// <summary>Prepay / renewal letters: one page per account with a payment stub at the bottom.</summary>
    public static FlowDocument Letters(List<(Account Account, List<ServiceRecord> Services)> letters,
        string letterType, string letterSize, decimal prepayPercent, DateTime? expires, bool billingOnStub)
    {
        DocumentBuilder doc = new($"{letterType} letters", $"{letters.Count} letter(s), {letterSize}");
        int nextYear = FakeData.Today.Year + 1;

        bool first = true;
        foreach ((Account account, List<ServiceRecord> services) in letters)
        {
            if (!first)
                doc.PageBreak();
            first = false;

            Address bill = account.Billing;
            decimal total = services.Sum(s => s.SaleAmount);
            decimal prepay = Math.Round(total * (1 - prepayPercent / 100m), 2);
            string greeting = bill.LastName.Length > 0 ? $"Dear {(bill.Salutation.Length > 0 ? bill.Salutation : bill.FirstName)} {bill.LastName}," : "Dear customer,";

            doc.Text(Formats.Date(FakeData.Today))
               .Text($"{bill.DisplayName}\n{bill.StreetLine}\n{bill.City}, {bill.State} {bill.Zip}")
               .Text(greeting)
               .Text(letterType switch
               {
                   "Upsell" => $"Thank you for choosing Billy Goat Lawn Care. Your {nextYear} lawn plan is below. Customers who prepay save {prepayPercent:0.##}%. Ask us about adding aeration or shrub care to your plan.",
                   "Renewal" => $"It is time to renew your lawn care for {nextYear}. Your services are listed below at this year's prices.",
                   "Prepay reminder" => $"This is a friendly reminder that the prepay discount of {prepayPercent:0.##}% ends on {Formats.Date(expires)}.",
                   _ => $"Thank you for choosing Billy Goat Lawn Care. Your {nextYear} lawn plan is below. Customers who prepay save {prepayPercent:0.##}%.",
               })
               .Table(["Service", "Price"], services.Select(s => new[] { s.ServiceName, Formats.Money(s.SaleAmount) }))
               .Text($"Total for the season: {Formats.Money(total)}    Prepay price: {Formats.Money(prepay)} (pay by {Formats.Date(expires)})", bold: true)
               .Text("Thank you,\nBilly Goat Lawn Care")
               .Text("- - - - - - - - - - - - - - - - - - - -  cut here and return with your payment  - - - - - - - - - - - - - - - - - - - -")
               .Barcode(account.AccountNumber.Replace("A-", ""))
               .Fields(
                   ("Account", account.AccountNumber),
                   ("Prepay amount", Formats.Money(prepay)),
                   ("Pay by", Formats.Date(expires)),
                   ("Bill to", billingOnStub ? account.BillingAddressText : bill.DisplayName));
        }
        return doc.Document;
    }

    private static string FirstNonEmpty(params string[] values) => values.FirstOrDefault(v => v.Length > 0) ?? "";
}

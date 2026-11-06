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

    private static string FirstNonEmpty(params string[] values) => values.FirstOrDefault(v => v.Length > 0) ?? "";
}

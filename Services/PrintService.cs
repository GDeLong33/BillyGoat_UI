using System.Windows.Controls;
using System.Windows.Documents;
using BillyGoat_UI.Views;

namespace BillyGoat_UI.Services;

/// <summary>
/// Print preview and printing. We do not build our own print engine: documents are
/// WPF FlowDocuments (made with DocumentBuilder), the preview is a FlowDocument viewer,
/// and printing goes through the standard Windows print dialog.
/// </summary>
public static class PrintService
{
    /// <summary>Opens the print preview window and waits until it is closed.</summary>
    public static void ShowPreview(FlowDocument document, string title)
    {
        PrintPreviewWindow window = new(document, title) { Owner = MessageService.GetOwner() };
        window.ShowDialog();
    }

    /// <summary>
    /// Shows the preview, then asks a Yes/No question when it closes,
    /// e.g. "Mark this work order as printed?". Returns true for Yes.
    /// </summary>
    public static bool PreviewThenAsk(FlowDocument document, string title, string question)
    {
        ShowPreview(document, title);
        return MessageService.Confirm(question);
    }

    /// <summary>Shows the Windows print dialog and prints. Returns false if the user cancelled.</summary>
    public static bool Print(FlowDocument document, string title)
    {
        PrintDialog dialog = new();
        if (dialog.ShowDialog() != true)
            return false;

        // A FlowDocument can hand out a "paginator" that splits it into printed pages.
        IDocumentPaginatorSource source = document;
        dialog.PrintDocument(source.DocumentPaginator, title);
        return true;
    }
}

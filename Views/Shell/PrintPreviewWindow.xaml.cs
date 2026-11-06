using System.Windows;
using System.Windows.Documents;
using BillyGoat_UI.Services;

namespace BillyGoat_UI.Views;

/// <summary>
/// Print preview window. Opened by Services/PrintService.cs with a finished document.
/// </summary>
public partial class PrintPreviewWindow : Window
{
    private readonly FlowDocument _document;
    private readonly string _title;

    public PrintPreviewWindow(FlowDocument document, string title)
    {
        InitializeComponent();
        _document = document;
        _title = title;
        Title = $"Print preview - {title}";
        Viewer.Document = document;

        // Keep the window on screen on small laptops.
        if (Height > SystemParameters.WorkArea.Height)
            Height = SystemParameters.WorkArea.Height - 20;
    }

    /// <summary>True when the user printed from this preview.</summary>
    public bool WasPrinted { get; private set; }

    private void Print_Click(object sender, RoutedEventArgs e)
    {
        if (PrintService.Print(_document, _title))
            WasPrinted = true;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

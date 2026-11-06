using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BillyGoat_UI.Services;

/// <summary>
/// Builds a printable page (a WPF FlowDocument) step by step, for example:
///     var doc = new DocumentBuilder("Payment journal listing", "10/01/2026 to 10/06/2026")
///         .Table(headers, rows)
///         .Document;
/// Every document starts with the company name (from Setup) and the title.
/// The page size follows the paper size chosen on the Setup tab.
/// </summary>
public class DocumentBuilder
{
    private static readonly Brush HeaderFill = new SolidColorBrush(Color.FromRgb(0xE6, 0xEC, 0xE4));
    private static readonly Brush LineColor = new SolidColorBrush(Color.FromRgb(0x9A, 0xA7, 0x9B));
    private static readonly Brush Muted = new SolidColorBrush(Color.FromRgb(0x4A, 0x5B, 0x4F));

    private bool _breakBeforeNext;   // true = the next block starts on a new page

    public DocumentBuilder(string title, string subtitle = "")
    {
        (double width, double height) = GetPageSize(AppSettings.Current.PaperSize);
        Document = new FlowDocument
        {
            PageWidth = width,
            PageHeight = height,
            PagePadding = new Thickness(56),
            ColumnWidth = width,   // one column as wide as the page (FlowDocuments use 2 columns by default)
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 12,
            Foreground = Brushes.Black,
            Background = Brushes.White,
            TextAlignment = TextAlignment.Left,   // FlowDocuments are justified by default
        };
        AddTitle(title, subtitle);
    }

    /// <summary>The finished document, ready for PrintService.</summary>
    public FlowDocument Document { get; }

    /// <summary>Page size in WPF units (1/96 inch).</summary>
    public static (double Width, double Height) GetPageSize(string paperSize) => paperSize switch
    {
        "Legal" => (8.5 * 96, 14 * 96),
        "A4" => (8.27 * 96, 11.69 * 96),
        _ => (8.5 * 96, 11 * 96),   // Letter
    };

    /// <summary>Company name, title and "Printed on" line at the top.</summary>
    private void AddTitle(string title, string subtitle)
    {
        Add(new Paragraph(new Run(AppSettings.Current.CompanyName)) { FontSize = 16, FontWeight = FontWeights.Bold, Margin = new Thickness(0) });
        Add(new Paragraph(new Run(title)) { FontSize = 20, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 2, 0, 0) });

        string printed = $"Printed {FakeData.Now.ToString("MM/dd/yyyy h:mm tt", Models.Formats.Us)} by {Session.UserName}";
        string line = subtitle.Length > 0 ? $"{subtitle}    |    {printed}" : printed;
        Add(new Paragraph(new Run(line))
        {
            FontSize = 11,
            Foreground = Muted,
            BorderBrush = LineColor,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(0, 0, 0, 8),
            Margin = new Thickness(0, 2, 0, 14),
        });
    }

    /// <summary>Makes the next thing added start on a new page.</summary>
    public DocumentBuilder PageBreak()
    {
        _breakBeforeNext = true;
        return this;
    }

    /// <summary>A bold section heading.</summary>
    public DocumentBuilder Heading(string text)
    {
        Add(new Paragraph(new Run(text)) { FontSize = 15, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 6) });
        return this;
    }

    /// <summary>A normal paragraph of text.</summary>
    public DocumentBuilder Text(string text, bool bold = false)
    {
        Add(new Paragraph(new Run(text)) { FontWeight = bold ? FontWeights.Bold : FontWeights.Normal, Margin = new Thickness(0, 0, 0, 8) });
        return this;
    }

    /// <summary>Label / value pairs in two columns, e.g. ("Invoice", "50101").</summary>
    public DocumentBuilder Fields(params (string Label, string Value)[] fields)
    {
        Table table = new() { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 10) };
        // Labels get 1/4 of the width, values 3/4. (Note: mixing a fixed-pixel column
        // with a star column squeezes the star column to nothing in FlowDocument tables.)
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        table.Columns.Add(new TableColumn { Width = new GridLength(3, GridUnitType.Star) });
        TableRowGroup group = new();
        foreach ((string label, string value) in fields)
        {
            TableRow row = new();
            row.Cells.Add(new TableCell(new Paragraph(new Run(label)) { Foreground = Muted, Margin = new Thickness(0, 2, 0, 2) }));
            row.Cells.Add(new TableCell(new Paragraph(new Run(value)) { FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 2, 0, 2) }));
            group.Rows.Add(row);
        }
        table.RowGroups.Add(group);
        Add(table);
        return this;
    }

    /// <summary>A table with a shaded header row. Each row is an array of cell texts.</summary>
    public DocumentBuilder Table(string[] headers, IEnumerable<string[]> rows)
    {
        Table table = new() { CellSpacing = 0, BorderBrush = LineColor, BorderThickness = new Thickness(1), Margin = new Thickness(0, 0, 0, 12) };
        foreach (string _ in headers)
            table.Columns.Add(new TableColumn());

        TableRowGroup group = new();
        group.Rows.Add(MakeRow(headers, isHeader: true));
        int count = 0;
        foreach (string[] cells in rows)
        {
            group.Rows.Add(MakeRow(cells, isHeader: false));
            count++;
        }
        if (count == 0)
        {
            TableRow empty = new();
            empty.Cells.Add(new TableCell(new Paragraph(new Run("Nothing found for these filters.")) { Margin = new Thickness(6) })
            { ColumnSpan = headers.Length });
            group.Rows.Add(empty);
        }
        table.RowGroups.Add(group);
        Add(table);
        return this;
    }

    private static TableRow MakeRow(string[] cells, bool isHeader)
    {
        TableRow row = new();
        if (isHeader)
            row.Background = HeaderFill;
        foreach (string text in cells)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(text)) { Margin = new Thickness(6, 4, 6, 4) })
            {
                BorderBrush = LineColor,
                BorderThickness = new Thickness(0, 0, 0, 1),
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
            });
        }
        return row;
    }

    /// <summary>
    /// A simple barcode picture for an invoice number. Each digit becomes a few
    /// black bars of different widths. (Demo only: a real system would use a
    /// barcode font or library so a scanner can read it.)
    /// </summary>
    public DocumentBuilder Barcode(string number)
    {
        System.Windows.Controls.StackPanel bars = new() { Orientation = System.Windows.Controls.Orientation.Horizontal, Height = 44 };
        foreach (char c in "*" + number + "*")
        {
            int value = char.IsDigit(c) ? c - '0' : 5;
            foreach (int width in new[] { 1 + value % 3, 2, 1 + value / 4 })
            {
                bars.Children.Add(new Rectangle { Width = width * 1.6, Fill = Brushes.Black });
                bars.Children.Add(new Rectangle { Width = 2.2 });
            }
        }
        System.Windows.Controls.StackPanel block = new();
        block.Children.Add(bars);
        block.Children.Add(new System.Windows.Controls.TextBlock { Text = number, FontFamily = new FontFamily("Consolas"), FontSize = 12 });
        Add(new BlockUIContainer(block) { Margin = new Thickness(0, 0, 0, 10) });
        return this;
    }

    private void Add(Block block)
    {
        if (_breakBeforeNext)
        {
            block.BreakPageBefore = true;
            _breakBeforeNext = false;
        }
        Document.Blocks.Add(block);
    }
}

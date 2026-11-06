namespace BillyGoat_UI.Models;

/// <summary>
/// One report in a report list (Report Gallery or Employee Reports).
/// The Name is also the key ReportFactory uses to build the sample report.
/// </summary>
public class ReportDefinition
{
    public required string Name { get; init; }
    public string Description { get; init; } = "";

    /// <summary>True when the report can be limited to one service (shows the Service filter).</summary>
    public bool UsesService { get; init; }

    /// <summary>The name is what screen readers announce for the list item.</summary>
    public override string ToString() => Name;
}

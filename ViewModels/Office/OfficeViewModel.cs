using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// The Office tab: a menu of five tools. Picking a tool shows it in place of
/// the menu; the tool's Close button brings the menu back.
/// </summary>
public partial class OfficeViewModel : ObservableObject
{
    public OfficeViewModel(bool showAllTools)
    {
        ShowAllTools = showAllTools;
    }

    /// <summary>
    /// False for the Applicator role: only "Enter Completed Services" is shown
    /// (see MainViewModel for the role rules).
    /// </summary>
    public bool ShowAllTools { get; }

    /// <summary>The open tool's ViewModel, or null when the menu is showing.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMenuVisible))]
    public partial object? CurrentTool { get; set; }

    public bool IsMenuVisible => CurrentTool == null;

    /// <summary>Every tool calls this from its Close button.</summary>
    private void CloseTool() => CurrentTool = null;

    // ===== The five tools =====

    /// <summary>1. Enter Completed Services (asks for a batch first).</summary>
    [RelayCommand]
    private void OpenCompletedServices()
    {
        Batch? batch = BatchDialogViewModel.Ask("Production");
        if (batch != null)
            CurrentTool = new CompletedServicesViewModel(batch, CloseTool);
    }

    /// <summary>2. Enter Payments (asks for a batch first).</summary>
    [RelayCommand]
    private void OpenPayments()
    {
        Batch? batch = BatchDialogViewModel.Ask("Payment");
        if (batch != null)
            CurrentTool = new EnterPaymentsViewModel(batch, CloseTool, startAccount: null);
    }

    /// <summary>3. Print Work Orders (3-step wizard).</summary>
    [RelayCommand]
    private void OpenPrintWorkOrders() => CurrentTool = new PrintWorkOrdersViewModel(CloseTool);

    /// <summary>4. Service Price Increase.</summary>
    [RelayCommand]
    private void OpenPriceIncrease() => CurrentTool = new PriceIncreaseViewModel(CloseTool);

    /// <summary>5. Print Prepay / Renewal Letters.</summary>
    [RelayCommand]
    private void OpenLetters() => CurrentTool = new PrepayLettersViewModel(CloseTool);
}

using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// The pop-up shown before Enter Completed Services and Enter Payments:
/// pick a batch date and either create a new batch or keep adding to an unposted one.
/// </summary>
public partial class BatchDialogViewModel : DialogViewModel
{
    private readonly string _kind;   // "Production" or "Payment"

    public BatchDialogViewModel(string kind)
    {
        _kind = kind;
        BatchDate = FakeData.Today;
        CreateNew = true;
        ErrorMessage = "";
        UnpostedBatches = FakeData.Batches.Where(b => b.Kind == kind && !b.IsPosted).ToList();
        SelectedBatch = UnpostedBatches.FirstOrDefault();
    }

    /// <summary>Shows the dialog. Returns the chosen batch, or null if the user cancelled.</summary>
    public static Batch? Ask(string kind)
    {
        BatchDialogViewModel dialog = new(kind);
        return DialogService.ShowDialog(dialog) ? dialog.Result : null;
    }

    public override string Title => _kind == "Payment" ? "Payment Batch" : "Production Batch";
    public string Heading => _kind == "Payment" ? "Choose a payment batch" : "Choose a production batch";

    [ObservableProperty] public partial DateTime BatchDate { get; set; }

    /// <summary>true = "Create a new batch", false = "Use an existing unposted batch".</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UseExisting))]
    public partial bool CreateNew { get; set; }

    public bool UseExisting
    {
        get => !CreateNew;
        set => CreateNew = !value;
    }

    public List<Batch> UnpostedBatches { get; }

    [ObservableProperty] public partial Batch? SelectedBatch { get; set; }
    [ObservableProperty] public partial string ErrorMessage { get; set; }

    /// <summary>The batch to use (set when OK is clicked).</summary>
    public Batch? Result { get; private set; }

    [RelayCommand]
    private void Ok()
    {
        if (CreateNew)
        {
            Result = new Batch { Number = FakeData.NewBatchNumber(_kind), Date = BatchDate, Kind = _kind };
            FakeData.Batches.Add(Result);
        }
        else if (SelectedBatch == null)
        {
            ErrorMessage = "There is no unposted batch. Choose \"Create a new batch\".";
            return;
        }
        else
        {
            Result = SelectedBatch;
        }
        Close(true);
    }

    [RelayCommand]
    private void Cancel() => Close(false);
}

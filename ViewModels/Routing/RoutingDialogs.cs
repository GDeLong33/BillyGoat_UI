using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillyGoat_UI.ViewModels;

/// <summary>"Add customer to route" dialog: pick an account, the service done there, and a date.</summary>
public partial class AddStopViewModel : DialogViewModel
{
    private readonly Route _route;

    public AddStopViewModel(Route route)
    {
        _route = route;
        Accounts = FakeData.Accounts.Where(a => a.IsActive).OrderBy(a => a.Service.DisplayName).ToList();
        ScheduledDate = FakeData.Today.AddDays(1);
        ServiceName = Lookups.Services.FirstOrDefault();
        ErrorMessage = "";
    }

    public override string Title => "Add Customer to Route";
    public string Heading => $"Add a customer to {_route.DisplayName}";

    public List<Account> Accounts { get; }

    [ObservableProperty] public partial Account? SelectedAccount { get; set; }
    [ObservableProperty] public partial string? ServiceName { get; set; }
    [ObservableProperty] public partial DateTime ScheduledDate { get; set; }
    [ObservableProperty] public partial string ErrorMessage { get; set; }

    [RelayCommand]
    private void Ok()
    {
        if (SelectedAccount == null || string.IsNullOrEmpty(ServiceName))
        {
            ErrorMessage = "Choose a customer and a service.";
            return;
        }
        Close(true);
    }

    [RelayCommand]
    private void Cancel() => Close(false);
}

/// <summary>A small dialog that asks for one date (used by "Move uncompleted work to a later date").</summary>
public partial class PickDateViewModel : DialogViewModel
{
    private readonly string _title;

    public PickDateViewModel(string title, string message, DateTime date)
    {
        _title = title;
        Message = message;
        Date = date;
    }

    public override string Title => _title;
    public string Message { get; }

    [ObservableProperty] public partial DateTime Date { get; set; }

    [RelayCommand]
    private void Ok() => Close(true);

    [RelayCommand]
    private void Cancel() => Close(false);
}

# BillyGoat_UI

Demo user interface for **Billy Goat Lawn Care LLC**, the replacement for Service Pro 2000.
WPF on .NET 10 with MVVM (CommunityToolkit.Mvvm). All data is fake and kept in memory
(`Services/FakeData.cs`); nothing is saved when the app closes.

## Run it

- Visual Studio: open `BillyGoat_UI.slnx` and press **F5**.
- Command line: `dotnet run` in this folder.

## Demo users (password for all: `demo`)

| User ID      | Role          | Sees                                                    |
|--------------|---------------|---------------------------------------------------------|
| `admin`      | Administrator | Everything                                              |
| `office`     | Office        | Everything except Backup & Export and Setup             |
| `applicator` | Applicator    | Only Office > Enter Completed Services                  |

The demo pretends today is Tuesday, October 6, 2026.

## Things to try

- **Search**: type "Whit", double-click the result to open the account.
- **Account > Services**: Print work order, then answer "Mark this work order as printed?".
- **Office > Enter Completed Services**: try invoice 50106.
- **Office > Enter Payments**: account A-01120, amount 100, check the Mulch work row.
- **Setup**: rename a property type, then look at an account's Details screen.

## Project layout

| Folder       | What is in it                                                                 |
|--------------|-------------------------------------------------------------------------------|
| `Models`     | Data classes (Account, ServiceRecord, Route, Employee...)                     |
| `ViewModels` | One ViewModel per screen or dialog, in subfolders per area                    |
| `Views`      | One UserControl per screen or dialog, in subfolders per area                  |
| `Services`   | Fake data, shared drop-down lists, printing, dialogs and message boxes        |
| `Resources`  | `Styles.xaml` (all colors and styles) and `ViewTemplates.xaml` (ViewModel to View map) |

How navigation works: `MainViewModel.CurrentPage` holds the ViewModel of the page on screen,
and `Resources/ViewTemplates.xaml` tells WPF which view to show for it. Dialogs work the same
way through `Services/DialogService.cs`.

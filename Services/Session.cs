using BillyGoat_UI.Models;

namespace BillyGoat_UI.Services;

/// <summary>
/// Who is signed in. MainViewModel sets this at sign-in and clears it at sign-out.
/// Other screens read it to stamp notes and records with the user's name.
/// </summary>
public static class Session
{
    public static User? CurrentUser { get; set; }

    /// <summary>The signed-in user's full name, e.g. "Bill Gordon".</summary>
    public static string UserName => CurrentUser?.FullName ?? "";
}

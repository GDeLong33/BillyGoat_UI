namespace BillyGoat_UI.Models;

/// <summary>What a signed-in user is allowed to see (see MainViewModel for the rules).</summary>
public enum UserRole
{
    Administrator,
    Office,
    Applicator
}

/// <summary>A person who can sign in. For the demo these come from Services/FakeData.cs.</summary>
public class User
{
    public required string UserId { get; init; }     // typed on the login screen, e.g. "admin"
    public required string Password { get; init; }   // demo only: never store real passwords in plain text
    public required string FullName { get; init; }
    public required UserRole Role { get; init; }
}

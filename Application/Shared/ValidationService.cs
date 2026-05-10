namespace Application.Shared;

public static class ValidationService
{
    public static bool NameExists(IEnumerable<string> existingNames, string name)
    {
        var trimmedName = name?.Trim()?.ToLower() ?? string.Empty;
        return existingNames.Any(n => n.Trim().ToLower() == trimmedName);
    }
}

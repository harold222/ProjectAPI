using Domain.Entities;

namespace Application.Shared;

public static class ValidationService
{
    public static bool CustomerNameExists(IEnumerable<Customer> existingCustomers, string name, int? excludeId = null)
    {
        var trimmedName = name?.Trim()?.ToLower() ?? string.Empty;

        return existingCustomers.Any(c =>
            c.Name.Trim().ToLower() == trimmedName &&
            (!excludeId.HasValue || c.CustomerId != excludeId.Value)
        );
    }
}
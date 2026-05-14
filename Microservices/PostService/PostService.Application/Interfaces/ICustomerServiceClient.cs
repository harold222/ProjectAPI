namespace PostService.Application.Interfaces;

/// <summary>
/// Contract defined by the Application layer (Dependency Inversion).
/// PostService needs to validate customer existence, but it doesn't know HOW.
/// The Infrastructure layer provides the implementation via HttpClient.
/// 
/// This is the SAME principle as IBaseRepository → BaseRepository,
/// but applied to inter-service communication.
/// </summary>
public interface ICustomerServiceClient
{
    /// <summary>
    /// Validates whether a customer exists in the CustomerService.
    /// Returns true if the customer exists, false otherwise.
    /// </summary>
    Task<bool> CustomerExistsAsync(int customerId);
}

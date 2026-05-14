using PostService.Application.Interfaces;

namespace PostService.Infrastructure.Clients;

/// <summary>
/// HTTP client that communicates with CustomerService to validate customer existence.
/// This is the Infrastructure implementation of ICustomerServiceClient — the Application
/// layer defines the interface (Dependency Inversion).
/// 
/// KEY ARCHITECTURAL POINT: This is where the microservice integration happens.
/// When PostService needs to validate a CustomerId, it makes an HTTP GET to /customer/{id}
/// on CustomerService.API. 200 = exists, 404 = doesn't exist.
/// </summary>
public class CustomerServiceClient : ICustomerServiceClient
{
    private readonly HttpClient _httpClient;

    public CustomerServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Calls GET /customer/{id} on CustomerService to validate customer existence.
    /// CustomerService returns 200 + customer body if found, 404 if not found.
    /// </summary>
    public async Task<bool> CustomerExistsAsync(int customerId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/customer/{customerId}");

            // 404 = customer not found (handled gracefully by CustomerService's GetById)
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return false;

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            // If CustomerService is down, we CAN'T validate — fail safe
            return false;
        }
    }
}

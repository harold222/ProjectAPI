using Application.DTOs;
using Application.Shared;
using Domain.Entities;

namespace Application.Services;

public class CustomerAppService
{
    private readonly BaseService<Customer> _customerService;

    public CustomerAppService(BaseService<Customer> customerService)
    {
        _customerService = customerService;
    }

    public async Task<IEnumerable<CustomerDto.Response>> GetAllAsync()
    {
        var customers = await _customerService.GetAllAsync();
        return customers.Select(MapToResponse);
    }

    public async Task<CustomerDto.Response> CreateAsync(CustomerDto.Create dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("El nombre no puede estar vacío");

        var existingCustomers = await _customerService.GetAllAsync();

        if (ValidationService.CustomerNameExists(existingCustomers, dto.Name))
            throw new InvalidOperationException("El nombre ingresado ya existe");

        var entity = new Customer { Name = dto.Name };
        var created = await _customerService.CreateAsync(entity);

        return MapToResponse(created);
    }

    public async Task<CustomerDto.CreateAllResult> CreateAllAsync(IEnumerable<CustomerDto.Create> dtos)
    {
        var result = new CustomerDto.CreateAllResult();

        var dtoList = dtos.ToList();
        if (dtoList.Count == 0)
            return result;

        var existingCustomers = await _customerService.GetAllAsync();

        var namesToCheck = new List<string>();

        foreach (var dto in dtoList)
        {
            var originalName = dto.Name;

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                result.Failed.Add(new CustomerDto.FailedItem
                {
                    Name = originalName,
                    Reason = "El nombre no puede estar vacío",
                });
                continue;
            }

            if (namesToCheck.Contains(dto.Name, StringComparer.OrdinalIgnoreCase))
            {
                result.Failed.Add(new CustomerDto.FailedItem
                {
                    Name = originalName,
                    Reason = "Nombre duplicado en la lista de entrada",
                });
                continue;
            }

            if (ValidationService.CustomerNameExists(existingCustomers, dto.Name))
            {
                result.Failed.Add(new CustomerDto.FailedItem
                {
                    Name = originalName,
                    Reason = "Ya existe un cliente con ese nombre",
                });
                continue;
            }

            namesToCheck.Add(dto.Name);
            result.Created.Add(new CustomerDto.Response
            {
                Id = 0,
                Name = dto.Name
            });
        }

        if (result.Created.Count == 0)
            return result;

        var entitiesToCreate = result.Created.Select(c => new Customer { Name = c.Name }).ToList();
        var created = await _customerService.CreateAllAsync(entitiesToCreate);

        for (int i = 0; i < result.Created.Count; i++)
        {
            result.Created[i].Id = created[i].CustomerId;
        }

        return result;
    }

    public async Task<CustomerDto.Response> UpdateAsync(CustomerDto.Update dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("El nombre no puede estar vacío");

        var existingCustomers = await _customerService.GetAllAsync();

        if (ValidationService.CustomerNameExists(existingCustomers, dto.Name, dto.Id))
            throw new InvalidOperationException("El nombre ingresado ya existe");

        var entity = new Customer { CustomerId = dto.Id, Name = dto.Name };
        var (updated, changed) = await _customerService.UpdateAsync(dto.Id, entity);

        return MapToResponse(updated);
    }

    public async Task<CustomerDto.DeleteResponse> DeleteAsync(int id)
    {
        var response = new CustomerDto.DeleteResponse(){ Status = false };

        var customer = await _customerService.GetAsync(id);

        if (customer == null)
            return response;

        await _customerService.DeleteAsync(customer);

        response.Status = true;
        return response;
    }

    private static CustomerDto.Response MapToResponse(Customer c) => new CustomerDto.Response
    {
        Id = c.CustomerId,
        Name = c.Name
    };
}

using Application.DTOs;
using Domain.Entities;
using Domain.Exceptions;

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
        var entity = Customer.Create(dto.Name);

        var created = await _customerService.CreateOrThrowAsync(entity);
        return MapToResponse(created.entity);
    }

    public async Task<CustomerDto.CreateAllResult> CreateAllAsync(IEnumerable<CustomerDto.Create> dtos)
    {
        var result = new CustomerDto.CreateAllResult();

        var dtoList = dtos.ToList();
        if (dtoList.Count == 0)
            return result;

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

            namesToCheck.Add(dto.Name);
            result.Created.Add(new CustomerDto.Response
            {
                Id = 0,
                Name = dto.Name
            });
        }

        if (result.Created.Count == 0)
            return result;

        var entitiesToCreate = result.Created.Select(c => Customer.Create(c.Name)).ToList();

        try
        {
            var created = await _customerService.CreateAllOrThrowAsync(entitiesToCreate);

            for (int i = 0; i < result.Created.Count; i++)
            {
                result.Created[i].Id = created[i].CustomerId;
            }
        }
        catch (DuplicateNameException)
        {
            result.Failed.AddRange(result.Created.Select(c => new CustomerDto.FailedItem
            {
                Name = c.Name,
                Reason = "Ya existe un cliente con ese nombre",
            }));
            result.Created.Clear();
        }

        return result;
    }

    public async Task<CustomerDto.Response> UpdateAsync(CustomerDto.Update dto)
    {
        Customer entity = Customer.Create(dto.Id, dto.Name);
        var (updated, changed) = await _customerService.UpdateAsync(dto.Id, entity);

        if (!changed)
            throw new KeyNotFoundException($"Customer Id {dto.Id} no encontrado");

        return MapToResponse(updated);
    }

    public async Task<CustomerDto.DeleteResponse> DeleteAsync(int id)
    {
        var response = new CustomerDto.DeleteResponse() { Status = false };

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

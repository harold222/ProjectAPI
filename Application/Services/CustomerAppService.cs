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

        List<Customer> entitiesToCreate = new();

        foreach (var dto in dtoList)
        {
            var originalName = dto.Name;

            try
            {
                entitiesToCreate.Add(Customer.Create(0, dto.Name));
            }
            catch (ArgumentException)
            {
                result.Failed.Add(new CustomerDto.FailedItem
                {
                    Name = originalName,
                    Reason = "El nombre no puede estar vacío",
                });
            }
        }

        if (entitiesToCreate.Count == 0)
            return result;

        try
        {
            var created = await _customerService.CreateAllOrThrowAsync(entitiesToCreate);

            result.Created = created.Select(c => new CustomerDto.Response
            {
                Id = c.CustomerId,
                Name = c.Name
            }).ToList();
        }
        catch (DuplicateNameException)
        {
            result.Failed.AddRange(entitiesToCreate.Select(c => new CustomerDto.FailedItem
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

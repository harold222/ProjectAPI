using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Customers;

[Route("[controller]")]
public class CustomerController : ControllerBase
{
    private readonly CustomerAppService _customerService;

    public CustomerController(CustomerAppService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet()]
    public async Task<ActionResult<IEnumerable<CustomerDto.Response>>> GetAll() => Ok(await _customerService.GetAllAsync());

    [HttpPost()]
    public async Task<ActionResult<CustomerDto.Response>> Create([FromBody] CustomerDto.Create dto) => CreatedAtAction(nameof(GetAll), await _customerService.CreateAsync(dto));

    [HttpPost("CreateAll")]
    public async Task<ActionResult<CustomerDto.CreateAllResult>> CreateAll([FromBody] IEnumerable<CustomerDto.Create> dtos) => Ok(await _customerService.CreateAllAsync(dtos));

    [HttpPut()]
    public async Task<ActionResult<CustomerDto.Response>> Update([FromBody] CustomerDto.Update dto) => Ok(await _customerService.UpdateAsync(dto));

    [HttpDelete("{id}")]
    public async Task<ActionResult<CustomerDto.DeleteResponse>> Delete(int id) => Ok(await _customerService.DeleteAsync(id));
}
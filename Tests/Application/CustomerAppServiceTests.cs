using Application.DTOs;
using Application.Services;
using Domain;
using Domain.Entities;
using Domain.Exceptions;
using Moq;

namespace Tests.Application;

public class CustomerAppServiceTests
{
    private readonly Mock<IBaseRepository<Customer>> _repoMock;
    private readonly BaseService<Customer> _baseService;
    private readonly CustomerAppService _service;

    public CustomerAppServiceTests()
    {
        _repoMock = new Mock<IBaseRepository<Customer>>();
        _baseService = new BaseService<Customer>(_repoMock.Object);
        _service = new CustomerAppService(_baseService);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedCustomers()
    {
        var customers = new List<Customer>
        {
            Customer.Create(1, "Alice"),
            Customer.Create(2, "Bob")
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);

        var result = await _service.GetAllAsync();

        var list = result.ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal("Alice", list[0].Name);
        Assert.Equal("Bob", list[1].Name);
    }

    [Fact]
    public async Task CreateAsync_WithValidName_ReturnsCreatedCustomer()
    {
        var dto = new CustomerDto.Create { Name = "Charlie" };
        var createdEntity = Customer.Create(5, "Charlie");
        _repoMock.Setup(r => r.CreateOrThrowAsync(It.IsAny<Customer>())).ReturnsAsync((createdEntity, true));

        var result = await _service.CreateAsync(dto);

        Assert.Equal(5, result.Id);
        Assert.Equal("Charlie", result.Name);
    }

    [Fact]
    public async Task CreateAllAsync_WithEmptyList_ReturnsEmptyResult()
    {
        var result = await _service.CreateAllAsync([]);

        Assert.Empty(result.Created);
        Assert.Empty(result.Failed);
    }

    [Fact]
    public async Task CreateAllAsync_WithValidItems_ReturnsCreatedItems()
    {
        var dtos = new List<CustomerDto.Create>
        {
            new() { Name = "Alice" },
            new() { Name = "Bob" }
        };
        var createdEntities = new List<Customer>
        {
            Customer.Create(1, "Alice"),
            Customer.Create(2, "Bob")
        };
        _repoMock.Setup(r => r.CreateAllOrThrowAsync(It.IsAny<List<Customer>>())).ReturnsAsync(createdEntities);

        var result = await _service.CreateAllAsync(dtos);

        Assert.Equal(2, result.Created.Count);
        Assert.Empty(result.Failed);
        Assert.Equal(1, result.Created[0].Id);
        Assert.Equal("Alice", result.Created[0].Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAllAsync_WithInvalidNames_AddsToFailed(string? name)
    {
        var dtos = new List<CustomerDto.Create> { new() { Name = name! } };

        var result = await _service.CreateAllAsync(dtos);

        Assert.Empty(result.Created);
        Assert.Single(result.Failed);
        Assert.Equal(name, result.Failed[0].Name);
        Assert.Contains("vacío", result.Failed[0].Reason);
    }

    [Fact]
    public async Task CreateAllAsync_WhenDuplicateNameException_AddsAllToFailed()
    {
        var dtos = new List<CustomerDto.Create> { new() { Name = "Alice" } };
        var entities = new List<Customer> { Customer.Create("Alice") };
        _repoMock.Setup(r => r.CreateAllOrThrowAsync(It.IsAny<List<Customer>>()))
                 .ThrowsAsync(new DuplicateNameException("duplicado"));

        var result = await _service.CreateAllAsync(dtos);

        Assert.Empty(result.Created);
        Assert.Single(result.Failed);
        Assert.Contains("Ya existe", result.Failed[0].Reason);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingId_ReturnsUpdatedCustomer()
    {
        var dto = new CustomerDto.Update { Id = 1, Name = "Updated" };
        var original = Customer.Create(1, "Old");
        var updated = Customer.Create(1, "Updated");
        _repoMock.Setup(r => r.GetAsync(1)).ReturnsAsync(original);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Customer>(), original)).ReturnsAsync((updated, true));

        var result = await _service.UpdateAsync(dto);

        Assert.Equal("Updated", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
    {
        var dto = new CustomerDto.Update { Id = 99, Name = "Updated" };
        _repoMock.Setup(r => r.GetAsync(99)).ReturnsAsync((Customer?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(dto));
        Assert.Contains("99", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ReturnsStatusTrue()
    {
        var customer = Customer.Create(1, "Alice");
        _repoMock.Setup(r => r.GetAsync(1)).ReturnsAsync(customer);
        _repoMock.Setup(r => r.DeleteAsync(customer)).ReturnsAsync(customer);

        var result = await _service.DeleteAsync(1);

        Assert.True(result.Status);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ReturnsStatusFalse()
    {
        _repoMock.Setup(r => r.GetAsync(99)).ReturnsAsync((Customer?)null);

        var result = await _service.DeleteAsync(99);

        Assert.False(result.Status);
    }
}

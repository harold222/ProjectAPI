using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Tests.Infrastructure;

/// <summary>
/// Tests unitarios de BaseRepository usando EF Core InMemory.
/// InMemory NO aplica constraints de DB (UNIQUE, FK), pero sirve para
/// verificar el comportamiento básico de CRUD.
/// </summary>
public class BaseRepositoryUnitTests : IDisposable
{
    private readonly JujuTestContext _context;
    private readonly BaseRepository<Customer> _repo;

    public BaseRepositoryUnitTests()
    {
        var options = new DbContextOptionsBuilder<JujuTestContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new JujuTestContext(options);
        _repo = new BaseRepository<Customer>(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateAsync_PersistsEntity()
    {
        var customer = Customer.Create("Alice");

        var result = await _repo.CreateAsync(customer);

        Assert.NotNull(result);
        Assert.Equal("Alice", result.Name);
        Assert.Equal(1, await _context.Customer.CountAsync());
    }

    [Fact]
    public async Task GetAsync_WithExistingId_ReturnsEntity()
    {
        var customer = Customer.Create("Bob");
        await _repo.CreateAsync(customer);

        var found = await _repo.GetAsync(customer.CustomerId);

        Assert.NotNull(found);
        Assert.Equal("Bob", found.Name);
    }

    [Fact]
    public async Task GetAsync_WithNonExistingId_ReturnsNull()
    {
        var result = await _repo.GetAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        await _repo.CreateAsync(Customer.Create("Alice"));
        await _repo.CreateAsync(Customer.Create("Bob"));

        var result = await _repo.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesExistingEntity()
    {
        var customer = Customer.Create("Old Name");
        await _repo.CreateAsync(customer);

        var edited = Customer.Create(customer.CustomerId, "New Name");
        var (updated, changed) = await _repo.UpdateAsync(edited, customer);

        Assert.True(changed);
        Assert.Equal("New Name", updated.Name);

        var fromDb = await _repo.GetAsync(customer.CustomerId);
        Assert.Equal("New Name", fromDb!.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithNoChanges_ReturnsFalse()
    {
        var customer = Customer.Create("Same Name");
        await _repo.CreateAsync(customer);

        var same = Customer.Create(customer.CustomerId, "Same Name");
        var (_, changed) = await _repo.UpdateAsync(same, customer);

        Assert.False(changed);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntity()
    {
        var customer = Customer.Create("ToDelete");
        await _repo.CreateAsync(customer);

        await _repo.DeleteAsync(customer);

        var result = await _repo.GetAsync(customer.CustomerId);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAllAsync_RemovesAllEntities()
    {
        var c1 = Customer.Create("A");
        var c2 = Customer.Create("B");
        await _repo.CreateAllAsync([c1, c2]);

        var deleted = await _repo.DeleteAllAsync([c1, c2]);

        Assert.Equal(2, deleted);
        Assert.Equal(0, await _context.Customer.CountAsync());
    }

    [Fact]
    public async Task CreateAllAsync_PersistsAllEntities()
    {
        var customers = new List<Customer>
        {
            Customer.Create("X"),
            Customer.Create("Y"),
            Customer.Create("Z")
        };

        var result = await _repo.CreateAllAsync(customers);

        Assert.Equal(3, result.Count);
        Assert.Equal(3, await _context.Customer.CountAsync());
    }
}

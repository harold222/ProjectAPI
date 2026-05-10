using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests.Infrastructure;

/// <summary>
/// Tests de integración de BaseRepository usando SQLite InMemory.
/// SQLite sí aplica UNIQUE constraints y FK constraints (con PRAGMA habilitado),
/// por eso sirve para verificar que CreateOrThrowAsync lanza las excepciones de dominio correctas.
/// </summary>
public class BaseRepositoryIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly JujuTestContext _context;
    private readonly BaseRepository<Customer> _customerRepo;
    private readonly BaseRepository<Post> _postRepo;
    private readonly BaseRepository<Category> _categoryRepo;

    public BaseRepositoryIntegrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Habilitar FK enforcement en SQLite (está OFF por defecto)
        using var pragma = _connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();

        var options = new DbContextOptionsBuilder<JujuTestContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new JujuTestContext(options);
        _context.Database.EnsureCreated();

        _customerRepo = new BaseRepository<Customer>(_context);
        _postRepo = new BaseRepository<Post>(_context);
        _categoryRepo = new BaseRepository<Category>(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    // ────────────────────────────────────────────────────────────
    // UNIQUE constraint violations
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrThrowAsync_WithDuplicateName_ThrowsDuplicateNameException()
    {
        await _customerRepo.CreateOrThrowAsync(Customer.Create("Alice"));

        await Assert.ThrowsAsync<DuplicateNameException>(() =>
            _customerRepo.CreateOrThrowAsync(Customer.Create("Alice")));
    }

    [Fact]
    public async Task CreateOrThrowAsync_WithUniqueName_Succeeds()
    {
        var (customer, changed) = await _customerRepo.CreateOrThrowAsync(Customer.Create("UniqueOne"));

        Assert.True(changed);
        Assert.Equal("UniqueOne", customer.Name);
    }

    [Fact]
    public async Task CreateAllOrThrowAsync_WithDuplicateInBatch_ThrowsDuplicateNameException()
    {
        await _customerRepo.CreateOrThrowAsync(Customer.Create("Existing"));

        var batch = new List<Customer>
        {
            Customer.Create("New"),
            Customer.Create("Existing") // duplicado
        };

        await Assert.ThrowsAsync<DuplicateNameException>(() =>
            _customerRepo.CreateAllOrThrowAsync(batch));
    }

    [Fact]
    public async Task CreateAllOrThrowAsync_WithAllUniqueNames_PersistsAll()
    {
        var batch = new List<Customer>
        {
            Customer.Create("Alpha"),
            Customer.Create("Beta"),
            Customer.Create("Gamma")
        };

        var result = await _customerRepo.CreateAllOrThrowAsync(batch);

        Assert.Equal(3, result.Count);
        Assert.Equal(3, await _context.Customer.CountAsync());
    }

    // ────────────────────────────────────────────────────────────
    // FK constraint violations
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrThrowAsync_WithNonExistingCustomerId_ThrowsForeignKeyViolationException()
    {
        var post = Post.Create(0, "Title", "Body", customerId: 9999, categoryId: null);

        await Assert.ThrowsAsync<ForeignKeyViolationException>(() =>
            _postRepo.CreateOrThrowAsync(post));
    }

    [Fact]
    public async Task CreateOrThrowAsync_WithValidCustomerId_Succeeds()
    {
        var (customer, _) = await _customerRepo.CreateOrThrowAsync(Customer.Create("ValidCustomer"));
        var post = Post.Create(0, "Title", "Body", customer.CustomerId, null);

        var (created, changed) = await _postRepo.CreateOrThrowAsync(post);

        Assert.True(changed);
        Assert.Equal("Title", created.Title);
    }

    [Fact]
    public async Task CreateAllOrThrowAsync_WithNonExistingCustomerId_ThrowsForeignKeyViolationException()
    {
        var (customer, _) = await _customerRepo.CreateOrThrowAsync(Customer.Create("Real"));

        var batch = new List<Post>
        {
            Post.Create(0, "Post1", "Body1", customer.CustomerId, null),
            Post.Create(0, "Post2", "Body2", customerId: 9999, null)  // FK inválida
        };

        await Assert.ThrowsAsync<ForeignKeyViolationException>(() =>
            _postRepo.CreateAllOrThrowAsync(batch));
    }

    [Fact]
    public async Task CreateOrThrowAsync_WithValidCategoryId_Succeeds()
    {
        var (customer, _) = await _customerRepo.CreateOrThrowAsync(Customer.Create("C1"));
        var (category, _) = await _categoryRepo.CreateOrThrowAsync(Category.Create("Tech"));
        var post = Post.Create(0, "Title", "Body", customer.CustomerId, category.CategoryId);

        var (created, _) = await _postRepo.CreateOrThrowAsync(post);

        Assert.Equal(category.CategoryId, created.CategoryId);
    }
}

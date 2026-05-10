using Application.DTOs;
using Application.Services;
using Domain;
using Domain.Entities;
using Moq;
using Tests;

namespace Tests.Application;

public class CategoryAppServiceTests
{
    private readonly Mock<IBaseRepository<Category>> _repoMock;
    private readonly BaseService<Category> _baseService;
    private readonly CategoryAppService _service;

    public CategoryAppServiceTests()
    {
        _repoMock = new Mock<IBaseRepository<Category>>();
        _baseService = new BaseService<Category>(_repoMock.Object);
        _service = new CategoryAppService(_baseService);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_WithExistingCategoryId_ReturnsCategory()
    {
        var category = TestHelpers.CreateCategory(5, "Tech");
        _repoMock.Setup(r => r.GetAsync(5)).ReturnsAsync(category);

        var (id, name) = await _service.ResolveOrCreateAsync(5, null);

        Assert.Equal(5, id);
        Assert.Equal("Tech", name);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_WithNonExistingIdAndCustomCategory_CreatesNewCategory()
    {
        var newCategory = TestHelpers.CreateCategory(10, "NewCat");
        _repoMock.Setup(r => r.GetAsync(5)).ReturnsAsync((Category?)null);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Category>())).ReturnsAsync(newCategory);

        var (id, name) = await _service.ResolveOrCreateAsync(5, "NewCat");

        Assert.Equal(10, id);
        Assert.Equal("NewCat", name);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_WithNonExistingIdAndNoCustomCategory_ThrowsInvalidOperationException()
    {
        _repoMock.Setup(r => r.GetAsync(5)).ReturnsAsync((Category?)null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ResolveOrCreateAsync(5, null));
        Assert.Contains("no existe", ex.Message);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_WithNullIdAndCustomCategory_CreatesNewCategory()
    {
        var newCategory = TestHelpers.CreateCategory(7, "Sports");
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Category>())).ReturnsAsync(newCategory);

        var (id, name) = await _service.ResolveOrCreateAsync(null, "Sports");

        Assert.Equal(7, id);
        Assert.Equal("Sports", name);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_WithNullIdAndEmptyCustomCategory_ThrowsInvalidOperationException()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ResolveOrCreateAsync(null, "  "));
        Assert.Contains("no existe", ex.Message);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_WithCategoryIdZeroAndCustomCategory_CreatesNewCategory()
    {
        // CategoryId = 0 se trata como "sin categoría proveída" por el check > 0
        var newCategory = TestHelpers.CreateCategory(8, "Music");
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Category>())).ReturnsAsync(newCategory);

        var (id, name) = await _service.ResolveOrCreateAsync(0, "Music");

        Assert.Equal(8, id);
        Assert.Equal("Music", name);
    }

    [Fact]
    public async Task ResolveOrCreateAsync_WithEmptyCustomCategory_ThrowsInvalidOperationException()
    {
        _repoMock.Setup(r => r.GetAsync(1)).ReturnsAsync((Category?)null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ResolveOrCreateAsync(1, ""));
        Assert.Contains("no existe", ex.Message);
    }
}

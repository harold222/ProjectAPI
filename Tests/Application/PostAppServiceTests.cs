using Application.DTOs;
using Application.Services;
using Domain;
using Domain.Entities;
using Domain.Exceptions;
using Moq;
using Tests;

namespace Tests.Application;

public class PostAppServiceTests
{
    private readonly Mock<IBaseRepository<Post>> _postRepoMock;
    private readonly Mock<IBaseRepository<Customer>> _customerRepoMock;
    private readonly Mock<IBaseRepository<Category>> _categoryRepoMock;

    private readonly BaseService<Post> _postBaseService;
    private readonly BaseService<Customer> _customerBaseService;
    private readonly CategoryAppService _categoryAppService;
    private readonly PostAppService _service;

    public PostAppServiceTests()
    {
        _postRepoMock = new Mock<IBaseRepository<Post>>();
        _customerRepoMock = new Mock<IBaseRepository<Customer>>();
        _categoryRepoMock = new Mock<IBaseRepository<Category>>();

        _postBaseService = new BaseService<Post>(_postRepoMock.Object);
        _customerBaseService = new BaseService<Customer>(_customerRepoMock.Object);
        _categoryAppService = new CategoryAppService(new BaseService<Category>(_categoryRepoMock.Object));
        _service = new PostAppService(_postBaseService, _customerBaseService, _categoryAppService);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedPosts()
    {
        var posts = new List<Post>
        {
            Post.Create(1, "Title1", "Body1", 10, 5),
            Post.Create(2, "Title2", "Body2", 20, null)
        };
        _postRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(posts);

        var result = await _service.GetAllAsync();
        var list = result.ToList();

        Assert.Equal(2, list.Count);
        Assert.Equal("Title1", list[0].Title);
        Assert.Equal(5, list[0].CategoryId);
        Assert.Null(list[1].CategoryId);
    }

    [Fact]
    public async Task CreateAsync_WithExistingCategory_ReturnsCreatedPost()
    {
        var category = TestHelpers.CreateCategory(5, "Tech");
        var createdPost = Post.Create(1, "Title", "Body", 10, 5);

        _categoryRepoMock.Setup(r => r.GetAsync(5)).ReturnsAsync(category);
        _postRepoMock.Setup(r => r.CreateOrThrowAsync(It.IsAny<Post>())).ReturnsAsync((createdPost, true));

        var dto = new PostDto.Create { Title = "Title", Body = "Body", CustomerId = 10, CategoryId = 5 };
        var result = await _service.CreateAsync(dto);

        Assert.Equal(1, result.Id);
        Assert.Equal("Title", result.Title);
        Assert.Equal(5, result.CategoryId);
    }

    [Fact]
    public async Task CreateAsync_WithCustomCategory_CreatesCategoryAndPost()
    {
        var newCategory = TestHelpers.CreateCategory(8, "NewCat");
        var createdPost = Post.Create(2, "Title", "Body", 10, 8);

        _categoryRepoMock.Setup(r => r.CreateAsync(It.IsAny<Category>())).ReturnsAsync(newCategory);
        _postRepoMock.Setup(r => r.CreateOrThrowAsync(It.IsAny<Post>())).ReturnsAsync((createdPost, true));

        var dto = new PostDto.Create { Title = "Title", Body = "Body", CustomerId = 10, CustomCategory = "NewCat" };
        var result = await _service.CreateAsync(dto);

        Assert.Equal(8, result.CategoryId);
    }

    [Fact]
    public async Task CreateAllAsync_WithEmptyList_ReturnsEmptyResult()
    {
        var result = await _service.CreateAllAsync([]);

        Assert.Empty(result.Created);
        Assert.Empty(result.Failed);
    }

    [Fact]
    public async Task CreateAllAsync_WithInvalidCategory_AddsToFailed()
    {
        var dtos = new List<PostDto.Create>
        {
            new() { Title = "T1", Body = "B1", CustomerId = 1, CategoryId = 99 }
        };
        _categoryRepoMock.Setup(r => r.GetAsync(99)).ReturnsAsync((Category?)null);

        var result = await _service.CreateAllAsync(dtos);

        Assert.Empty(result.Created);
        Assert.Single(result.Failed);
        Assert.Contains("no existe", result.Failed[0].Reason);
    }

    [Fact]
    public async Task CreateAllAsync_WithValidItemsAndBatchSuccess_ReturnsCreatedWithIds()
    {
        var dtos = new List<PostDto.Create>
        {
            new() { Title = "T1", Body = "B1", CustomerId = 1, CategoryId = 5 }
        };
        var category = TestHelpers.CreateCategory(5, "Tech");
        var createdPosts = new List<Post> { Post.Create(100, "T1", "B1", 1, 5) };

        _categoryRepoMock.Setup(r => r.GetAsync(5)).ReturnsAsync(category);
        _postRepoMock.Setup(r => r.CreateAllOrThrowAsync(It.IsAny<List<Post>>())).ReturnsAsync(createdPosts);

        var result = await _service.CreateAllAsync(dtos);

        Assert.Single(result.Created);
        Assert.Equal(100, result.Created[0].Id);
        Assert.Empty(result.Failed);
    }

    [Fact]
    public async Task CreateAllAsync_WhenForeignKeyViolation_MovesAllToFailed()
    {
        var dtos = new List<PostDto.Create>
        {
            new() { Title = "T1", Body = "B1", CustomerId = 1, CategoryId = 5 }
        };
        var category = TestHelpers.CreateCategory(5, "Tech");

        _categoryRepoMock.Setup(r => r.GetAsync(5)).ReturnsAsync(category);
        _postRepoMock.Setup(r => r.CreateAllOrThrowAsync(It.IsAny<List<Post>>()))
                     .ThrowsAsync(new ForeignKeyViolationException("FK fail"));

        var result = await _service.CreateAllAsync(dtos);

        Assert.Empty(result.Created);
        Assert.Single(result.Failed);
        Assert.Contains("no existe", result.Failed[0].Reason);
    }

    [Fact]
    public async Task CreateAllAsync_WhenPartialCategoryFailure_MixedResult()
    {
        var dtos = new List<PostDto.Create>
        {
            new() { Title = "Good", Body = "B1", CustomerId = 1, CategoryId = 5 },
            new() { Title = "Bad", Body = "B2", CustomerId = 2, CategoryId = 99 }
        };
        var category = TestHelpers.CreateCategory(5, "Tech");
        var createdPosts = new List<Post> { Post.Create(1, "Good", "B1", 1, 5) };

        _categoryRepoMock.Setup(r => r.GetAsync(5)).ReturnsAsync(category);
        _categoryRepoMock.Setup(r => r.GetAsync(99)).ReturnsAsync((Category?)null);
        _postRepoMock.Setup(r => r.CreateAllOrThrowAsync(It.IsAny<List<Post>>())).ReturnsAsync(createdPosts);

        var result = await _service.CreateAllAsync(dtos);

        Assert.Single(result.Created);
        Assert.Single(result.Failed);
        Assert.Equal("Good", result.Created[0].Title);
        Assert.Equal("Bad", result.Failed[0].Title);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingPost_ReturnsUpdatedPost()
    {
        var existing = Post.Create(1, "Old", "OldBody", 10, 5);
        var category = TestHelpers.CreateCategory(7, "NewCat");
        var updated = Post.Create(1, "New", "NewBody", 10, 7);

        _postRepoMock.Setup(r => r.GetAsync(1)).ReturnsAsync(existing);
        _categoryRepoMock.Setup(r => r.CreateAsync(It.IsAny<Category>())).ReturnsAsync(category);
        _postRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Post>(), existing)).ReturnsAsync((updated, true));

        var dto = new PostDto.Update { Id = 1, Title = "New", Body = "NewBody", CustomCategory = "NewCat" };
        var result = await _service.UpdateAsync(dto);

        Assert.Equal("New", result.Title);
        Assert.Equal(7, result.CategoryId);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingPost_ThrowsKeyNotFoundException()
    {
        _postRepoMock.Setup(r => r.GetAsync(99)).ReturnsAsync((Post?)null);

        var dto = new PostDto.Update { Id = 99, Title = "New", Body = "Body" };

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(dto));
        Assert.Contains("99", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingPost_ReturnsStatusTrue()
    {
        var post = Post.Create(1, "Title", "Body", 10, null);
        _postRepoMock.Setup(r => r.GetAsync(1)).ReturnsAsync(post);
        _postRepoMock.Setup(r => r.DeleteAsync(post)).ReturnsAsync(post);

        var result = await _service.DeleteAsync(1);

        Assert.True(result.Status);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingPost_ReturnsStatusFalse()
    {
        _postRepoMock.Setup(r => r.GetAsync(99)).ReturnsAsync((Post?)null);

        var result = await _service.DeleteAsync(99);

        Assert.False(result.Status);
    }
}

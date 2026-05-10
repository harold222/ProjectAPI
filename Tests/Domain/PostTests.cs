using Domain.Entities;

namespace Tests.Domain;

public class PostTests
{
    [Fact]
    public void Create_WithValidData_ReturnsPost()
    {
        var post = Post.Create(0, "Título", "Body", 1, 2);

        Assert.Equal("Título", post.Title);
        Assert.Equal("Body", post.Body);
        Assert.Equal(1, post.CustomerId);
        Assert.Equal(2, post.CategoryId);
    }

    [Fact]
    public void Create_WithNullCategory_ReturnsPostWithNullCategoryId()
    {
        var post = Post.Create(0, "Título", "Body", 5, null);

        Assert.Null(post.CategoryId);
        Assert.Equal(5, post.CustomerId);
    }

    [Fact]
    public void Create_WithLongBody_TruncatesAndAppendsEllipsis()
    {
        var longBody = new string('a', 100);
        var post = Post.Create(0, "Título", longBody, 1, null);

        Assert.Equal(100, post.Body.Length);
        Assert.EndsWith("...", post.Body);
    }

    [Theory]
    [InlineData(null, "body", 1)]
    [InlineData("", "body", 1)]
    [InlineData("   ", "body", 1)]
    public void Create_WithInvalidTitle_ThrowsArgumentException(string? title, string body, int customerId)
    {
        var ex = Assert.Throws<ArgumentException>(() => Post.Create(0, title!, body, customerId, null));
        Assert.Contains("título", ex.Message);
    }

    [Theory]
    [InlineData("title", null, 1)]
    [InlineData("title", "", 1)]
    [InlineData("title", "   ", 1)]
    public void Create_WithInvalidBody_ThrowsArgumentException(string title, string? body, int customerId)
    {
        var ex = Assert.Throws<ArgumentException>(() => Post.Create(0, title, body!, customerId, null));
        Assert.Contains("body", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidCustomerId_ThrowsArgumentException(int invalidCustomerId)
    {
        var ex = Assert.Throws<ArgumentException>(() => Post.Create(0, "Title", "Body", invalidCustomerId, null));
        Assert.Contains("customerId", ex.Message);
    }

    [Fact]
    public void Create_PostIdIsAssigned()
    {
        var post = Post.Create(99, "Título", "Body", 1, null);
        Assert.Equal(99, post.PostId);
    }
}

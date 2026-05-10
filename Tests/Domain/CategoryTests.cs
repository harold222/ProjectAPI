using Domain.Entities;

namespace Tests.Domain;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidName_ReturnsCategory()
    {
        var category = Category.Create("Tecnología");

        Assert.NotNull(category);
        Assert.Equal("Tecnología", category.CategoryName);
        Assert.Equal(0, category.CategoryId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ThrowsArgumentException(string? invalidName)
    {
        var ex = Assert.Throws<ArgumentException>(() => Category.Create(invalidName!));
        Assert.Contains("vacío", ex.Message);
    }

    [Fact]
    public void Create_WithNameExceeding200Characters_ThrowsArgumentException()
    {
        var longName = new string('a', 201);

        var ex = Assert.Throws<ArgumentException>(() => Category.Create(longName));
        Assert.Contains("200", ex.Message);
    }

    [Fact]
    public void Create_WithNameExactly200Characters_ReturnsCategory()
    {
        var exactName = new string('a', 200);

        var category = Category.Create(exactName);

        Assert.Equal(exactName, category.CategoryName);
    }
}

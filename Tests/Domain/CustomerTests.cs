using Domain.Entities;

namespace Tests.Domain;

public class CustomerTests
{
    [Fact]
    public void Create_WithValidName_ReturnsCustomer()
    {
        var customer = Customer.Create("Juan");

        Assert.NotNull(customer);
        Assert.Equal("Juan", customer.Name);
        Assert.Equal(0, customer.CustomerId);
    }

    [Fact]
    public void Create_WithIdAndName_ReturnsCustomerWithId()
    {
        var customer = Customer.Create(42, "Maria");

        Assert.Equal(42, customer.CustomerId);
        Assert.Equal("Maria", customer.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        var ex = Assert.Throws<ArgumentException>(() => Customer.Create(invalidName!));
        Assert.Contains("vacío", ex.Message);
    }

    [Fact]
    public void Create_NameIsPrivateSet_CannotBeModifiedExternally()
    {
        var customer = Customer.Create("Pedro");
        
        // Esto no compilaría si descomentás: customer.Name = "Otro";
        // Verificamos que la propiedad existe con private set
        var propInfo = typeof(Customer).GetProperty("Name");
        Assert.NotNull(propInfo);
        Assert.False(propInfo.SetMethod?.IsPublic ?? true);
    }
}

namespace Domain.Entities;

public class Customer
{
    public int CustomerId { get; private set; }
    public string Name { get; private set; }
    public virtual ICollection<Post> Posts { get; set; }

    public static Customer Create(string name)
    {
        Validate(name);
        return new Customer { Name = name };
    }

    public static Customer Create(int id, string name)
    {
        Validate(name);
        return new Customer { CustomerId = id, Name = name };
    }

    private static void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre no puede estar vacío");
    }
}
